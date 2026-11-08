using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SkillPageManager : MonoBehaviour
{
    public static SkillPageManager instance;

    public Button[] skillpagebuttons;
    public Sprite[] onoffsprite;

    public GameObject skillview, skillpieceview;
    public Transform skillparent, skillpieceparent;

    //public List<SkillSlot> skillSlots;

    public GameObject skillPrefab;
    public Sprite[] skillPrefabOnOffSprite;

    public Dictionary<int, SkillSlot> myskills;

    public SkillEquipSlot[] equipslot;

    public GameObject SelectSkill;

    public GameObject selectUIParent;
    [SerializeField] Button selectCancelArea; // 교체할 칸 선택 중 칸 바깥을 누르면 취소 (selectUIParent의 첫 자식)
    public GameObject[] selectUI;

    public GameObject ChangeAskPopup;
    public Image ChangeSkillImage;
    public Button ChangeYes;

    public TextMeshProUGUI show_CostText; // 현재 코스트 보여주는 용도

    public bool isFull
    {
        get { return equipCoast >= UserDataBase.Instance.PlayerStats.coast; }
    }

    public float equipCoast = 0;

    // 스킬 장착 변경 실패 안내 (번역 시트 키)
    const string SkillChangeFailedKey = "skill_equip_failed";
    const string SkillChangeSyncedKey = "skill_equip_failed_synced";
    const int SkillResyncAttempts = 3;
    const float SkillResyncRetryDelaySeconds = 2f;

    /// <summary>서버에 보낼 장착 변경 하나 (slotNum -1 = 해제)</summary>
    readonly struct SkillSlotRequest
    {
        public readonly int SkillId;
        public readonly int SlotNum;

        public SkillSlotRequest(int skillId, int slotNum)
        {
            SkillId = skillId;
            SlotNum = slotNum;
        }
    }

    // 서버에 저장이 확인된 장착 상태(칸 번호 → 스킬 id, 빈 칸 -1). 요청이 실패하면 이 상태로 되돌립니다.
    int[] confirmedSkillIds;
    // 화면에 먼저 반영한 변경을 입력 순서대로 하나씩 서버에 보냅니다. (동시에 보내면 해제보다 장착이 먼저 도착해 거절될 수 있음)
    readonly Queue<SkillSlotRequest> pendingRequests = new Queue<SkillSlotRequest>();
    Coroutine skillSyncRoutine;
    bool isSkillResyncing;
    InventoryPopup boundPopup;

    /// <summary>장착 변경을 서버에 저장하거나 실패 후 다시 맞추는 중이면 true</summary>
    public bool IsSkillSyncing => pendingRequests.Count > 0 || skillSyncRoutine != null || isSkillResyncing;

    /// <summary>저장 실패가 날 때마다 1씩 늘어납니다. (기다리던 작업이 실패를 알아차리는 용도)</summary>
    public int SkillSyncFailureCount { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 스킬 사용 아이콘 위치 동일하게 설정
    /// 현재 장착중인 스킬 구분(ex = 0일경우 안끼고있음,0이아닐경우 해당 스킬 id를 끼고있음)
    /// 스킬을 주고 받기 위해선 게임매니저에 저장한 후 활용해야된다.
    /// </summary>

    void Start()
    {
        GameManager.gInstance.equipskill = new int[equipslot.Length];
        myskills = new Dictionary<int, SkillSlot>();
        for (int i = 0; i < GameManager.gInstance.equipskill.Length; i++)
        {
            GameManager.gInstance.equipskill[i] = -1;
        }

        foreach (var c in APIData.Instance.SkillKeyValue)
        {
            GameObject g = Instantiate(skillPrefab, skillparent.transform);
            myskills[c.Key] = g.GetComponent<SkillSlot>();

            //skillSlots.Add(myskills[c.Key]);

            if (UserDataBase.Instance.userData.mySkilldata.ContainsKey(c.Key))
            {
                //if (UserDataBase.Instance.userData.mySkilldata[c.Key].skillpiece < 1)
                //    if (c.Value.dynamicId < 7)
                //    {
                //        StartCoroutine(APIData.Instance.UnityWebRequestgetSkillPiecePost(c.Key, 15));
                //    }

                myskills[c.Key].Init(UserDataBase.Instance.userData.mySkilldata[c.Key]);
            }
            else
            {
                g.SetActive(false); // 계정 데이터가 없는 스킬(미보유)은 목록에 표시하지 않음
            }
        }

        if (skillPrefab.scene.rootCount != 0)
            Destroy(skillPrefab);

        // 로그인 때 받은 칸 번호로 장착 칸을 채웁니다. 이 상태가 서버에 저장된 상태입니다.
        // (범위를 벗어나거나 겹치는 칸 번호는 ReadAccountSlotSkillIds에서 미장착으로 처리)
        // 코스트는 ApplySlotState 안에서 실제 장착 칸 기준으로 계산합니다.
        confirmedSkillIds = ReadAccountSlotSkillIds();
        ApplySlotState(confirmedSkillIds);
        LanguageSingleton.LangInstance.LocalizeChanged += RefreshCostText; // 18차: 언어 바꾸면 '스킬 코스트' 글자도 (기존: 처음 언어로 고정)

        boundPopup = InventoryManager.Instance.InventoryPopup;
        boundPopup.SkillInfoClosed += OnSkillInfoClosed;
        boundPopup.skillinfo_equipButton.onClick.AddListener(() => OnClickEquipSkill());
        boundPopup.skillinfo_unequipButton.onClick.AddListener(() => OnClickUnEquipSkill());
        ChangeYes.onClick.AddListener(() => ChangeYesAction());
        if (selectCancelArea != null)
            selectCancelArea.onClick.AddListener(ChangeNoEvent); // 확인창 '아니요'와 같은 취소 처리
    }

    //public void OnClickButton(int i) // 스킬, 스킬조각 버튼
    //{
    //    switch(i)
    //    {
    //        case 0:
    //            skillpagebuttons[0].image.sprite = onoffsprite[1];
    //            skillpagebuttons[1].image.sprite = onoffsprite[0];
    //            skillview.SetActive(true);
    //            skillpieceview.SetActive(false);
    //            break;

    //        case 1:
    //            skillpagebuttons[0].image.sprite = onoffsprite[0];
    //            skillpagebuttons[1].image.sprite = onoffsprite[1];
    //            skillview.SetActive(false);
    //            skillpieceview.SetActive(true);
    //            break;
    //    }
    //}

    public void EquipSkill(int i) // 장착 버튼 눌렀을 때 켜지는 장착슬롯선택UI 버튼들 이벤트
    {
        SkillSlot slotData = SelectSkill != null ? SelectSkill.GetComponent<SkillSlot>() : null;
        if (slotData == null || isSkillResyncing)
        {
            ChangeNoEvent();
            return;
        }

        SkillSlot current = equipslot[i].skilldata;
        var calCoast = equipCoast - (current != null ? current.skill.coast : 0);

        if (slotData.skill.coast + calCoast > UserDataBase.Instance.PlayerStats.coast)
        {
            Debug.Log("코스트 초과");
            selectUIParent.SetActive(false);
            InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
            GameManager.gInstance.PopUpOpen("coast_over");
            return;
        }
        else
        {
            // 칸을 고르면 한 번 더 확인합니다. '아니요'면 칸 선택 화면에 남아 다른 칸을 고를 수 있습니다.
            GameManager.gInstance.PopUpConfirm(InventoryManager.SwapConfirmKey, () =>
            {
                // 확인창이 떠 있는 동안 선택 화면이 닫혔으면(다른 경로로 취소) 교체하지 않습니다.
                if (SelectSkill == null || !selectUIParent.activeSelf || isSkillResyncing)
                    return;
                SwapSkill(slotData, i);
            });
        }
    }

    /// <summary>칸의 스킬을 바꿉니다. 화면은 바로 바꾸고 칸 선택을 닫아 연타로 두 번 교체되지 않게 하며,
    /// 서버에는 기존 스킬 해제 → 새 스킬 장착 순서로 보냅니다.</summary>
    void SwapSkill(SkillSlot newSkill, int slotIndex)
    {
        SkillSlot previous = equipslot[slotIndex].skilldata;

        AssignSlot(slotIndex, newSkill);
        RefreshCoast();

        if (previous != null)
            EnqueueSkillRequest(previous.skill.id, -1);
        EnqueueSkillRequest(newSkill.skill.id, slotIndex);

        SelectSkill = null;
        selectUIParent.SetActive(false);
        InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
    }

    /// <summary>장착 칸 하나의 표시, 보유 스킬 목록, 계정 스킬 데이터, 인게임 전달 값을 함께 바꿉니다.
    /// 스킬 장착 상태는 이 메서드로만 바꿉니다. (코스트 갱신은 호출한 쪽에서 RefreshCoast)</summary>
    void AssignSlot(int slotIndex, SkillSlot skill)
    {
        SkillEquipSlot slot = equipslot[slotIndex];
        SkillSlot previous = slot.skilldata;

        if (previous != null && previous != skill)
        {
            slot.ClearSkill();
            SetSkillSlotNum(previous, -1);
            previous.gameObject.SetActive(previous.IsOwned); // 보유 목록에 다시 표시 (보유한 스킬만)
        }

        if (skill != null)
        {
            // 다른 칸에 있던 스킬이면 그 칸을 먼저 비웁니다. (되돌리기·서버 재조회로 자리가 바뀌는 경우)
            for (int i = 0; i < equipslot.Length; i++)
                if (i != slotIndex && equipslot[i].skilldata == skill)
                    AssignSlot(i, null);

            // 이미 이 칸에 있는 스킬이어도 데이터를 다시 받으면(SkillSlot.Init) 칸 번호·목록 표시가 바뀌므로 다시 맞춥니다.
            SetSkillSlotNum(skill, slotIndex);
            skill.gameObject.SetActive(false); // 장착한 스킬은 보유 목록에서 숨김
            if (previous != skill)
                slot.SettingSkill(skill);
        }

        GameManager.gInstance.equipskill[slotIndex] = skill != null ? skill.skill.id : -1;
    }

    /// <summary>서버에서 받은 계정 스킬 데이터(조각 수·칸 번호)를 스킬 목록과 장착 칸에 반영합니다.
    /// 장착 변경을 저장하는 중이면 화면의 장착 상태를 유지하고(저장 결과가 우선), 아니면 서버 칸 번호로 맞춥니다.
    /// 계정 스킬 데이터(UserDataBase.mySkilldata)는 호출 전에 갱신되어 있어야 합니다.</summary>
    public void ApplyAccountSkillData(IEnumerable<MySkillData> skills)
    {
        if (skills == null || myskills == null)
            return;

        int[] viewSkillIds = CaptureSlotSkillIds();

        foreach (MySkillData data in skills)
            if (myskills.TryGetValue(data.id, out SkillSlot skillSlot))
                skillSlot.Init(data);

        if (IsSkillSyncing)
        {
            ApplySlotState(viewSkillIds);
            return;
        }

        confirmedSkillIds = ReadAccountSlotSkillIds();
        ApplySlotState(confirmedSkillIds);
    }

    // MySkillData는 struct라 칸 번호를 바꾼 뒤 계정 스킬 데이터에도 다시 넣어야 합니다.
    void SetSkillSlotNum(SkillSlot skill, int slotNum)
    {
        skill.myskill.slotNum = slotNum;
        UserDataBase.Instance.userData.mySkilldata[skill.myskill.id] = skill.myskill;
    }

    /// <summary>모든 장착 칸을 주어진 상태(칸 번호 → 스킬 id, 빈 칸 -1)로 맞춥니다.</summary>
    void ApplySlotState(int[] skillIds)
    {
        for (int i = 0; i < equipslot.Length; i++)
        {
            SkillSlot skill = null;
            if (skillIds != null && i < skillIds.Length && skillIds[i] >= 0)
                myskills.TryGetValue(skillIds[i], out skill);
            AssignSlot(i, skill);
        }

        // 어느 칸에도 없는 스킬은 칸 번호를 비웁니다. (서버 데이터로 다시 초기화된 스킬에 남은 칸 번호 정리)
        Dictionary<int, MySkillData> accountSkills = UserDataBase.Instance.userData.mySkilldata;
        foreach (var pair in myskills)
        {
            SkillSlot skill = pair.Value;
            if (skill.myskill.slotNum == -1 || !accountSkills.ContainsKey(pair.Key) || equipslot.Any(s => s.skilldata == skill))
                continue;
            SetSkillSlotNum(skill, -1);
        }

        RefreshCoast();
    }

    int[] CaptureSlotSkillIds()
    {
        int[] ids = new int[equipslot.Length];
        for (int i = 0; i < equipslot.Length; i++)
            ids[i] = equipslot[i].skilldata != null ? equipslot[i].skilldata.skill.id : -1;
        return ids;
    }

    // 서버에서 다시 받은 계정 스킬 데이터의 칸 번호로 장착 상태를 만듭니다. 범위를 벗어나거나 겹치는 칸은 무시합니다.
    int[] ReadAccountSlotSkillIds()
    {
        int[] ids = Enumerable.Repeat(-1, equipslot.Length).ToArray();
        foreach (var pair in UserDataBase.Instance.userData.mySkilldata)
        {
            int slotNum = pair.Value.slotNum;
            if (slotNum >= 0 && slotNum < ids.Length && ids[slotNum] == -1 && myskills.ContainsKey(pair.Key))
                ids[slotNum] = pair.Key;
        }
        return ids;
    }

    void EnqueueSkillRequest(int skillId, int slotNum)
    {
        pendingRequests.Enqueue(new SkillSlotRequest(skillId, slotNum));
        if (skillSyncRoutine == null)
            skillSyncRoutine = StartCoroutine(SyncSkillRequests());
    }

    /// <summary>대기 중인 변경을 입력 순서대로 하나씩 서버에 보냅니다. 하나라도 실패하면 나머지는 보내지 않고 되돌립니다.</summary>
    IEnumerator SyncSkillRequests()
    {
        while (pendingRequests.Count > 0)
        {
            SkillSlotRequest request = pendingRequests.Peek();
            bool succeeded = false;
            yield return StartCoroutine(APIData.Instance.UnityWebRequestPutSkill(request.SkillId, request.SlotNum, () => succeeded = true));

            // 서버 점검 중에는 요청 함수가 콜백 없이 끝나므로 성공 통지가 없으면 실패로 처리합니다.
            if (!succeeded)
            {
                skillSyncRoutine = null;
                HandleSkillSyncFailure();
                yield break;
            }

            pendingRequests.Dequeue();
            ApplyConfirmedRequest(request);
        }

        skillSyncRoutine = null;
    }

    void ApplyConfirmedRequest(SkillSlotRequest request)
    {
        for (int i = 0; i < confirmedSkillIds.Length; i++)
            if (confirmedSkillIds[i] == request.SkillId)
                confirmedSkillIds[i] = -1;

        if (request.SlotNum >= 0 && request.SlotNum < confirmedSkillIds.Length)
            confirmedSkillIds[request.SlotNum] = request.SkillId;
    }

    void HandleSkillSyncFailure()
    {
        pendingRequests.Clear();
        SkillSyncFailureCount++;

        // 진행 중인 교체·정보창은 바뀐 상태와 맞지 않으므로 닫습니다.
        CancelSkillChange();
        if (SelectSkill != null)
            InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();

        // 먼저 서버에 저장이 확인된 상태로 되돌린 뒤, 응답만 끊긴 경우를 대비해 서버 상태를 다시 받아 맞춥니다.
        ApplySlotState(confirmedSkillIds);
        StartCoroutine(ResyncSkillsFromServer());
    }

    IEnumerator ResyncSkillsFromServer()
    {
        isSkillResyncing = true;

        bool synced = false;
        for (int attempt = 0; attempt < SkillResyncAttempts && !synced; attempt++)
        {
            if (attempt > 0)
                yield return new WaitForSecondsRealtime(SkillResyncRetryDelaySeconds);
            yield return StartCoroutine(APIData.Instance.UnityWebRequestaccountSkillGET(() => synced = true));
        }

        isSkillResyncing = false;

        if (synced)
            ApplyAccountSkillData(UserDataBase.Instance.userData.mySkilldata.Values.ToList()); // 서버 칸 번호로 맞추고 확정 상태로 기록
        else
            Debug.LogWarning("스킬 장착 상태를 서버에서 다시 받아오지 못했습니다. 이전 상태로 되돌린 화면을 유지합니다.");

        GameManager.gInstance.PopUpOpen(synced ? SkillChangeSyncedKey : SkillChangeFailedKey);
    }

    /// <summary>스테이지 입장처럼 서버의 장착 상태에 의존하는 작업 전에 저장이 끝나길 기다립니다.</summary>
    public IEnumerator WaitForSkillSync()
    {
        while (IsSkillSyncing)
            yield return null;
    }

    /// <summary>슬롯 기준으로 코스트를 다시 계산하고 화면 글자까지 갱신한다.
    /// 런타임에 스킬 데이터가 갱신되는 경로(APIData 의 계정 스킬 갱신/조각 획득)에서 호출.</summary>
    public void RefreshCoast()
    {
        RecalculateCoast();
        if (show_CostText != null && LanguageSingleton.LangInstance != null && UserDataBase.Instance != null)
            show_CostText.text = LanguageSingleton.LangInstance.FormatString("coast_current", equipCoast, UserDataBase.Instance.PlayerStats.coast);
    }

    /// <summary>장착 칸에 실제로 들어간 스킬 기준으로 코스트 다시 계산</summary>
    public void RecalculateCoast()
    {
        int sum = 0;
        if (equipslot != null)
            for (int i = 0; i < equipslot.Length; i++)
                if (equipslot[i] != null && equipslot[i].skilldata != null)
                    sum += equipslot[i].skilldata.skill.coast;
        equipCoast = sum;
        if (UserDataBase.Instance != null)
            UserDataBase.Instance.PlayerStats.equipment_coast = sum;
    }

    void ChangeYesAction()
    {
        ChangeAskPopup.SetActive(false);
        selectUIParent.SetActive(true); // 켜진 뒤에야 캔버스 기준 좌표를 계산할 수 있습니다.
        for (int i = 0; i < selectUI.Length; i++)
        {
            GetSelectUIImage(i).sprite = equipslot[i].skillimage.sprite;

            // 선택 칸은 팝업 캔버스에 있어 화면 비율마다 실제 장착 칸과 어긋나므로, 열 때마다 실제 칸 위로 맞춥니다.
            UIOverlayUtility.AlignTo((RectTransform)selectUI[i].transform, (RectTransform)equipslot[i].transform);
        }
    }

    // 선택 칸의 스킬 이미지. 자식 순서가 바뀌어도 찾을 수 있게 이름으로 찾습니다.
    Image GetSelectUIImage(int index)
    {
        return selectUI[index].transform.Find("Image").GetComponent<Image>();
    }

    /// <summary>스킬 교체 확인창 또는 칸 선택 중이면 취소하고 true를 반환합니다. (뒤로 가기 처리용)</summary>
    public bool CancelSkillChange()
    {
        if (!ChangeAskPopup.activeSelf && !selectUIParent.activeSelf)
            return false;

        ChangeNoEvent();
        return true;
    }

    public void ChangeNoEvent()
    {
        selectUIParent.SetActive(false);
        ChangeAskPopup.SetActive(false);
        InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
    }

    void OnClickEquipSkill() // 스킬정보팝업에 있는 장착 버튼 클릭
    {
        // 실패 후 서버 상태로 다시 맞추는 동안에는 변경을 받지 않습니다.
        if (SelectSkill == null || isSkillResyncing)
        {
            InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
            return;
        }

        SkillSlot slotData = SelectSkill.GetComponent<SkillSlot>();

        if (slotData == null) return;

        int equipNum = 0;

        for (int i = 0; i < equipslot.Length; i++)
        {
            if (slotData.myskill.slotNum == -1 && !equipslot[i].isEquip)
            {
                if (isFull)
                {
                    if (slotData.skill.coast > 0)
                    {
                        Debug.Log("풀코스트");
                        //selectUIParent.SetActive(false);
                        InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
                        GameManager.gInstance.PopUpOpen("coast_full");
                        return;
                    }
                }
                else
                {
                    if (slotData.skill.coast + equipCoast > UserDataBase.Instance.PlayerStats.coast)
                    {
                        Debug.Log("코스트 초과");
                        //selectUIParent.SetActive(false);
                        InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
                        GameManager.gInstance.PopUpOpen("coast_over");
                        return;
                    }
                }

                AssignSlot(i, slotData);

                // 코스트는 장착 칸 기준으로만 계산 (단일 소유). 칸을 채운 뒤 다시 계산하고 글자까지 갱신한다.
                RefreshCoast();

                EnqueueSkillRequest(slotData.skill.id, i);

                SelectSkill = null;

                //selectUIParent.SetActive(false);
                InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
                InitSelectUISprite();

                break;
            }
            else
            {
                equipNum++;
                if (equipNum == equipslot.Length)
                {
                    InventoryManager.Instance.InventoryPopup.SkillInfo.SetActive(false);

                    ChangeAskPopup.SetActive(true);
                    ChangeSkillImage.sprite = APIData.Instance.AddressableSprite[$"{slotData.skill.effectId}_{slotData.skill.name}"];
                }
            }
        }
    }

    void OnClickUnEquipSkill() // 스킬정보팝업에 있는 해제 버튼 클릭
    {
        // 고른 장착 칸이 없거나(정보창을 닫은 뒤 등) 서버 상태로 다시 맞추는 중이면 해제하지 않습니다.
        if (SelectSkill == null || isSkillResyncing)
        {
            InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
            return;
        }

        if (!SelectSkill.TryGetComponent<SkillEquipSlot>(out var slotEquipData) || slotEquipData.skilldata == null)
            return;

        int slotIndex = System.Array.IndexOf(equipslot, slotEquipData);
        if (slotIndex < 0)
            return;

        int skillId = slotEquipData.skilldata.skill.id;
        AssignSlot(slotIndex, null);
        RefreshCoast(); // 장착중인 스킬 코스트
        EnqueueSkillRequest(skillId, -1);

        SelectSkill = null;
        InventoryManager.Instance.InventoryPopup.CloseSkillInfoPopupUI();
    }

    // 정보창이 닫히면 고른 스킬 선택을 비웁니다. 교체 확인·칸 선택 중에는 고른 스킬이 필요하므로 유지합니다.
    void OnSkillInfoClosed()
    {
        if (ChangeAskPopup.activeSelf || selectUIParent.activeSelf)
            return;
        SelectSkill = null;
    }

    void InitSelectUISprite()
    {
        for (int i = 0; i < selectUI.Length; i++)
        {
            GetSelectUIImage(i).sprite = null;
        }
    }

    /// <summary>18차: 현재 언어로 코스트 글자 다시 쓰기</summary>
    void RefreshCostText()
    {
        if (show_CostText == null || LanguageSingleton.LangInstance == null || UserDataBase.Instance == null)
            return;
        try
        {
            show_CostText.text = LanguageSingleton.LangInstance.FormatString("coast_current", equipCoast, UserDataBase.Instance.PlayerStats.coast);
        }
        catch (System.Exception) { }
    }

    void OnDestroy()
    {
        if (LanguageSingleton.LangInstance != null)
            LanguageSingleton.LangInstance.LocalizeChanged -= RefreshCostText;
        if (boundPopup != null)
            boundPopup.SkillInfoClosed -= OnSkillInfoClosed;
    }
}