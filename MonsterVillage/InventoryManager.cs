using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public int Capacity { get; private set; }

    [SerializeField] private Transform inventoryParent;
    private Canvas inventoryCanvas; // 장비/스킬 탭 전환은 오브젝트가 아니라 이 Canvas의 enabled를 바꿉니다.
    [SerializeField] private GameObject slotPrefab;
    [SerializeField, Range(7, 100)] private int maxCapacity = 100;

    [SerializeField] public List<Slot> slots = new List<Slot>(); // 인벤토리 슬롯 리스트
    [SerializeField] public Slot SelectSlot; // 선택한 슬롯

    public InventoryPopup InventoryPopup;

    public EquipmentSlot[] EquipSlot;
    [SerializeField]public EquipmentSlot equipmentSlot;

    public Animator enhanceAnim;
    public Animator enhancefailAnim;

    public GameObject additembutton;
    [SerializeField] private bool showEditorAddItemButton;

    [SerializeField] private Button sortBtn_Name;
    [SerializeField] private Button sortBtn_Force;
    [SerializeField] private Button sortBtn_Grade;

    [SerializeField] private TextMeshProUGUI inventoryCountText;

    [SerializeField] MyItemData _myitem;
    [SerializeField] MyEquipmentItemData _myequipitem;
    private bool isUpgradeRunning;
    private bool isSellRunning;
    private Button[] sellConfirmButtons; // 판매 확인창의 모든 버튼(예/아니오/X)

    // 안내 팝업 문구 키. 문구는 번역 시트에서 관리합니다.
    private const string EquipChangeFailedKey = "inven_equip_failed";
    private const string EquipChangeSyncedKey = "inven_equip_failed_synced";
    public const string SwapConfirmKey = "question_reallyswap"; // 장비·스킬 교체 칸을 고른 뒤 확인 문구
    private const string SellFailedKey = "inven_sell_failed";
    private const string EnhanceFailedKey = "inven_enhance_failed";
    private const string EnhanceMaterialWarningKey = "inven_enhance_material_warning";
    private const string UnequipInventoryFullKey = "inven_unequip_full";

    // 낙관적으로 반영한 장착/해제의 서버 전송 대기열. 코루틴은 이 객체(씬 수명)가 소유합니다.
    private readonly EquipmentChangeQueue equipmentChanges = new EquipmentChangeQueue();
    private Coroutine equipmentSyncRoutine;

    // 장착/해제 실패 뒤 서버 상태를 다시 받아오는 중인지. 이 동안에는 장착/해제 입력과 서버 의존 작업을 막습니다.
    private bool isEquipmentResyncing;
    private const int EquipmentResyncAttempts = 3;
    private const float EquipmentResyncRetryDelaySeconds = 2f;

    // 강화 재료 삭제 요청이 실패했을 때 다시 보내는 횟수(첫 요청 제외)
    private const int MaterialDeleteRetryCount = 2;

    // 장착 슬롯이 가득 찼을 때 교체할 슬롯을 고르는 중인 장비. 0이면 교체 선택 중이 아닙니다.
    private int swapInventoryID;
    [SerializeField] private EquipmentSwapView swapView; // 교체 확인창·칸 선택 View (문구 키: inven_equip_full)
    public bool IsSwapSelecting => swapInventoryID != 0;

    private static List<MyItemData> Inventory => UserDataBase.Instance.userData.Invendata;
    private static MyEquipmentItemData[] EquippedItems => UserDataBase.Instance.userData.equipData;

    public enum SortType { None, Name, Force, Grade }
    private SortType currentSort = SortType.Grade;   // 기본 정렬: 등급
    private bool sortAscending = false;              // 높은 등급 먼저(내림차순)

    // 신규 아이템(빨간점) 판정기. 추후 서버 전환 시 INewItemTracker 구현체만 교체하면 됩니다.
    private readonly INewItemTracker newItemTracker = new LocalNewItemTracker();
    // 현재 화면에서 빨간점을 표시할 대상(inventoryID). FreshSlot 시 미확인 신규 아이템이 자동으로 추가됩니다.
    private readonly HashSet<int> sessionNewIds = new HashSet<int>();
    // 이번 열람에서 신규로 보여준 항목들 → 다음에 장비 탭을 열 때 일괄 해제됩니다.
    private readonly HashSet<int> pendingClearNewIds = new HashSet<int>();

    [Header("선택 판매(다중)")]
    [SerializeField] private Button selectSellModeButton;    // 선택 판매 모드 진입/토글 버튼
    [SerializeField] private Button selectSellConfirmButton;  // 선택한 장비 판매 실행 버튼
    [SerializeField] private Button selectSellCancelButton;   // 선택 판매 취소 버튼
    [SerializeField] private GameObject selectSellBar;        // 선택 판매 UI 묶음(총액/버튼). 모드일 때만 표시
    [SerializeField] private TextMeshProUGUI selectSellTotalText; // 선택 합계 판매액
    [SerializeField] private TextMeshProUGUI selectSellCountText; // 선택 개수(선택)
    private bool isSelectSellMode;
    private readonly HashSet<int> selectedSellIds = new HashSet<int>(); // 선택된 inventoryID

    public enum testEnum
    {
        fire = 4,
        water = 13,
        electric = 14,
        land = 15,
        speed = 29,
        def = 30,
        hp = 31,
        duration = 32,
        recovery = 33,
        luck = 34,
        manaCal = 35,
        cool = 36,
        spa = 37,
        bless = 38,
        curse = 39,
    }

    public testEnum testenum;

    public enum itemTestenum
    {
        불 = 1,
        물 = 2,
        번개 = 3,
        땅 = 4,
        이속 = 5,
        방어 = 6,
        체력 = 7,
        지속 = 8,
        회복 = 9,
        행운 = 10,
        쿨감 = 11,
        범위 = 12,
        부활 = 13,
        디버프 = 14
    }

    public itemTestenum itemTest;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BindSwapView();

#if UNITY_EDITOR
        additembutton.SetActive(showEditorAddItemButton);
        if (showEditorAddItemButton)
            additembutton.GetComponent<Button>().onClick.AddListener(() => OnClickAddItemButton());
#else
        additembutton.SetActive(false);
#endif

        Capacity = maxCapacity;
        inventoryCanvas = inventoryParent.GetComponent<Canvas>();

        for (int i = 0; i < Capacity; i++)
        {
            int slotNum = i;
            GameObject g = Instantiate(slotPrefab, inventoryParent);
            g.name = "Slot" + slotNum;

            slots.Add(g.GetComponent<Slot>());
            slots[i].GetComponent<Button>().onClick.AddListener(() => OnClickSlot(slotNum));
        }

        for (int i = 0; i < EquipSlot.Length; i++)
        {
            int slotNum = i;
            EquipSlot[i].GetComponent<Button>().onClick.AddListener(() => OnClickEquipSlot(slotNum));
        }

        if (slotPrefab.scene.rootCount != 0)
            Destroy(slotPrefab);

        //for (int i = 0; i < APIData.Instance.userData.Invendata.Count; i++)
        //{
        //    slots[i]._myitem = APIData.Instance.userData.Invendata[i];
        //}

        // EquipSlot에 equipData를 넣어야 함
        //APIData.Instance.userData.equipData = new MyEquipmentItemData[APIData.Instance.EquipmentKeyValue.Count];

        UserDataBase.Instance.ResetPlayerStats();


        NormalizeEquippedItems();
        for (int i = 0; i < EquipSlot.Length; i++)
        {
            EquipSlot[i].isEmpty = true;
            SyncEquipSlot(i);
        }
        // 로딩 씬에서 받은 인벤토리는 이 시점에 처음으로 장착 목록과 맞출 수 있습니다.
        EquipmentChangeQueue.RemoveEquipped(Inventory, EquippedItems);
        //int equipnum = 0;
        //foreach (KeyValuePair<int, MyEquipmentItemData> equipslot in APIData.Instance.EquipmentKeyValue)
        //{
        //    if (APIData.Instance.userData.equipData[equipnum] != null)
        //    {
        //        EquipSlot[equipnum].equipmentItemData = equipslot.Value;
        //        EquipSlot[equipnum]._item = APIData.Instance.GetItem<StaticItemData>(EquipSlot[equipnum].equipmentItemData.id);
        //        APIData.Instance.userData.equipData[equipnum] = EquipSlot[equipnum].equipmentItemData;
        //        EquipSlot[equipnum].set_equipmentdata_in_userstat_plus();

        //        EquipSlot[equipnum].itemtypeimage.gameObject.SetActive(true);
        //        EquipSlot[equipnum].itemimage.gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        EquipSlot[equipnum].equipmentItemData = new MyEquipmentItemData();
        //    }
        //        equipnum++;
        //}
        
        // 최초 실행 시 기존 보유 아이템은 신규로 뜨지 않도록 기준선을 세팅합니다.
        newItemTracker.EnsureBaseline(GetOwnedItemIds());

        FreshSlot();
        SelectSlot = null;

        InventoryPopup.iteminfo_enhanceButton.onClick.AddListener(() => SetBtn(0));
        InventoryPopup.enhance_enhanceButton.onClick.AddListener(() => SetBtn(1));
        InventoryPopup.iteminfo_equipButton.onClick.AddListener(() => SetBtn(2));
        InventoryPopup.iteminfo_unequipButton.onClick.AddListener(() => SetBtn(3));
        InventoryPopup.iteminfo_sellButton.onClick.AddListener(() => SetBtn(4));
        InventoryPopup.confirm_yesButton.onClick.AddListener(() => OnClickConfirmSellButton());
        InventoryPopup.confirm_noButton.onClick.AddListener(OnClickConfirmNo);
        sellConfirmButtons = InventoryPopup.really_sell_questionPopup.GetComponentsInChildren<Button>(true);

        if (sortBtn_Name != null) sortBtn_Name.onClick.AddListener(() => SetSort(0));
        if (sortBtn_Force != null) sortBtn_Force.onClick.AddListener(() => SetSort(1));
        if (sortBtn_Grade != null) sortBtn_Grade.onClick.AddListener(() => SetSort(2));

        if (selectSellModeButton != null) selectSellModeButton.onClick.AddListener(ToggleSelectSellMode);
        if (selectSellConfirmButton != null) selectSellConfirmButton.onClick.AddListener(OnClickSelectSellConfirm);
        if (selectSellCancelButton != null) selectSellCancelButton.onClick.AddListener(ExitSelectSellMode);
        if (selectSellBar != null) selectSellBar.SetActive(false);
        // 21차: 장비/스킬 정보 창의 '장착' 버튼이 어두운 바탕 + 어두운 글자라 안 보이던 문제 → 보라 버튼 + 흰 글자
        StyleInfoButton(InventoryPopup != null ? InventoryPopup.iteminfo_equipButton : null, "btn_violet");
        StyleInfoButton(InventoryPopup != null ? InventoryPopup.iteminfo_unequipButton : null, "btn_dark");
        StyleInfoButton(InventoryPopup != null ? InventoryPopup.skillinfo_equipButton : null, "btn_violet");
        StyleInfoButton(InventoryPopup != null ? InventoryPopup.skillinfo_unequipButton : null, "btn_dark");

        LanguageSingleton.LangInstance.LocalizeChanged += UpdateSortButtonTexts;
        UpdateSortButtonTexts();
    }

    private void OnDestroy()
    {
        if (LanguageSingleton.LangInstance != null)
            LanguageSingleton.LangInstance.LocalizeChanged -= UpdateSortButtonTexts;
        UnbindSwapView();
    }

    // 교체 View의 입력을 이 객체의 교체 처리에 연결합니다. 다시 호출해도 중복 등록되지 않습니다.
    void BindSwapView()
    {
        if (swapView == null)
            return;

        UnbindSwapView();
        swapView.Confirmed += OnSwapConfirmed;
        swapView.Cancelled += OnSwapCancelled;
        swapView.SlotSelected += OnSwapSlotSelected;
    }

    void UnbindSwapView()
    {
        if (swapView == null)
            return;

        swapView.Confirmed -= OnSwapConfirmed;
        swapView.Cancelled -= OnSwapCancelled;
        swapView.SlotSelected -= OnSwapSlotSelected;
    }

    private void Update()
    {
        // 21차: '선택 판매'는 장비에만 해당 → 장비 목록이 실제로 보일 때만 버튼 표시
        if (selectSellModeButton != null && inventoryCanvas != null)
        {
            bool equipView = inventoryCanvas.isActiveAndEnabled;
            if (selectSellModeButton.gameObject.activeSelf != equipView)
                selectSellModeButton.gameObject.SetActive(equipView);
            if (!equipView && isSelectSellMode)
                ExitSelectSellMode();
            if (!equipView && IsSwapSelecting)
                CancelSwapSelect();
        }
    }

    static void StyleInfoButton(Button b, string sprite)
    {
        if (b == null)
            return;
        if (b.TryGetComponent(out Image img))
        {
            Sprite sp = Ui2.S(sprite);
            if (sp != null)
            {
                img.sprite = sp;
                img.type = Image.Type.Sliced;
            }
            img.color = Color.white;
        }
        foreach (TMP_Text t in b.GetComponentsInChildren<TMP_Text>(true))
        {
            t.color = Color.white;
            t.fontStyle |= FontStyles.Bold;
        }
    }

    public void OnClickAddItemButton()
    {
        LobbyManager.Instance.AddItem(APIData.Instance.GetItem<StaticItemData>((int)itemTest).id);
    }

    public void LoonSetting(Image stoneimage, Image loonimage, int stoneindex, int loonindex)
    {
        if (stoneindex == 0 && loonindex == 0) return;
        stoneimage.sprite = APIData.Instance.AddressableSprite["stoneline_" + stoneindex];
        loonimage.sprite = APIData.Instance.AddressableSprite["Loon_" + loonindex];
    }

    public void SetSort(int sortType) // 0=이름, 1=강화, 2=등급
    {
        SortType newSort = (SortType)(sortType + 1);
        if (currentSort == newSort)
            sortAscending = !sortAscending;
        else
        {
            currentSort = newSort;
            sortAscending = newSort == SortType.Name; // 이름은 기본 오름차순, 강화/등급은 기본 내림차순
        }
        UpdateSortButtonTexts();
        FreshSlot();
    }

    void UpdateSortButtonTexts()
    {
        UpdateSortBtnText(sortBtn_Name, SortType.Name, "inven_sort_name");
        UpdateSortBtnText(sortBtn_Force, SortType.Force, "inven_sort_enhance");
        UpdateSortBtnText(sortBtn_Grade, SortType.Grade, "inven_sort_grade");
    }

    void UpdateSortBtnText(Button btn, SortType type, string langKey)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null) return;
        string label = LanguageSingleton.LangInstance.GetData(langKey);
        if (currentSort == type)
            tmp.text = label + (sortAscending ? " ▲" : " ▽");
        else
            tmp.text = label;
    }

    List<MyItemData> GetSortedInvendata()
    {
        // 동률 항목의 순서를 고정합니다. 서버 목록 순서가 달라도 응답 도착 시 슬롯이 다시 섞이지 않습니다.
        var data = Inventory.OrderBy(x => x.id).ThenBy(x => x.inventoryID).ToList();
        List<MyItemData> sorted;
        switch (currentSort)
        {
            case SortType.Name:
                sorted = (sortAscending
                    ? data.OrderBy(x => APIData.Instance.GetItem<StaticItemData>(x.id).LangName)
                    : data.OrderByDescending(x => APIData.Instance.GetItem<StaticItemData>(x.id).LangName)).ToList();
                break;
            case SortType.Force:
                sorted = (sortAscending
                    ? data.OrderBy(x => x.force)
                    : data.OrderByDescending(x => x.force)).ToList();
                break;
            case SortType.Grade:
                // 등급 정렬 시 같은 등급 내에서는 강화수치 높은 순으로 2차 정렬
                sorted = (sortAscending
                    ? data.OrderBy(x => APIData.Instance.GetItem<StaticItemData>(x.id).rankType).ThenByDescending(x => x.force)
                    : data.OrderByDescending(x => APIData.Instance.GetItem<StaticItemData>(x.id).rankType).ThenByDescending(x => x.force)).ToList();
                break;
            default:
                sorted = data.ToList();
                break;
        }

        // 신규(빨간점) 아이템을 항상 맨 앞으로, 그 안에서는 가장 최근에 얻은 것(inventoryID 큰 순) 먼저.
        // OrderBy는 안정 정렬이라 신규가 아닌 항목들은 위 기본 정렬 순서를 그대로 유지합니다.
        return sorted
            .OrderByDescending(x => sessionNewIds.Contains(x.inventoryID))
            .ThenByDescending(x => sessionNewIds.Contains(x.inventoryID) ? x.inventoryID : 0)
            .ToList();
    }

    public void FreshSlot() // 아이템을 획득하거나 장착/강화 상태가 바뀐 뒤 슬롯 갱신
    {
        // 정렬 전에 미확인 신규 아이템을 표시 대상에 추가 (신규 아이템을 맨 앞으로 정렬하기 위함)
        foreach (var item in UserDataBase.Instance.userData.Invendata)
        {
            if (newItemTracker.IsNew(item.inventoryID))
                sessionNewIds.Add(item.inventoryID);
        }

        var sortedData = GetSortedInvendata();
        int visibleItemCount = Mathf.Min(slots.Count, sortedData.Count);
        for (int i = 0; i < visibleItemCount; i++)
        {
            int inventoryID = sortedData[i].inventoryID;
            slots[i].SetData(sortedData[i], sessionNewIds.Contains(inventoryID));
            // 선택 판매 모드에서 선택된 슬롯 표시 유지 (재정렬/갱신 후에도 inventoryID 기준으로 복원)
            slots[i].SetSelected(isSelectSellMode && selectedSellIds.Contains(inventoryID));
        }

        for (int i = visibleItemCount; i < slots.Count; i++)
        {
            slots[i].Init();
        }

        // 슬롯 내용이 재배치됐으므로 열려 있는 정보창의 선택도 같은 장비가 있는 슬롯으로 옮깁니다.
        if (SelectSlot != null)
            SelectSlot = slots.Find(slot => !slot.isEmpty && slot._myitem.inventoryID == _myitem.inventoryID);

        if (UserDataBase.Instance.userData.Invendata.Count > slots.Count)
            Debug.LogWarning($"Inventory slots are not enough. item count: {UserDataBase.Instance.userData.Invendata.Count}, slot count: {slots.Count}");

        if (inventoryCountText != null)
            inventoryCountText.text = $"{UserDataBase.Instance.userData.Invendata.Count} / {maxCapacity}";
    }

    /// <summary>
    /// 장비 탭을 열 때 호출됩니다. 아직 확인하지 않은 신규 아이템에 빨간점을 표시하고,
    /// 동시에 현재 보유 아이템을 모두 '확인함'으로 저장해 다음에 열 때는 사라지도록 합니다. (일괄 해제)
    /// </summary>
    public void OnOpenEquipmentTab()
    {
        // InventoryManager 초기화(슬롯 생성) 전에 호출되면 아무것도 하지 않습니다. (스크립트 실행 순서 안전장치)
        if (slots == null || slots.Count == 0) return;

        // 다른 메뉴로 나가면 이 객체의 Update가 멈춰 모드가 남을 수 있으므로, 탭에 들어올 때 기본 상태로 되돌립니다.
        if (isSelectSellMode)
            ExitSelectSellMode();
        CancelSwapSelect();

        // 장착 중인 장비는 인벤토리에 없지만 확인 기록은 유지해야 해제했을 때 신규로 뜨지 않습니다.
        var currentIds = GetOwnedItemIds();

        // 지난 열람에서 이미 보여줬던 신규 표시는 이번에 해제합니다. (일괄 해제)
        sessionNewIds.ExceptWith(pendingClearNewIds);
        pendingClearNewIds.Clear();

        // 아직 확인하지 않은 신규 아이템을 이번 열람에 표시합니다.
        foreach (var item in Inventory)
        {
            if (newItemTracker.IsNew(item.inventoryID))
                sessionNewIds.Add(item.inventoryID);
        }

        // 이번에 표시된 신규 항목은 다음에 탭을 열 때 사라지도록 예약하고, 영구적으로 '확인함' 처리합니다.
        pendingClearNewIds.UnionWith(sessionNewIds);
        newItemTracker.MarkSeenAll(currentIds);

        FreshSlot();
    }

    List<int> GetOwnedItemIds()
    {
        return Inventory.Select(x => x.inventoryID)
            .Concat(EquippedItems.Where(EquipmentChangeQueue.IsOccupied).Select(x => x.inventoryID))
            .ToList();
    }

    public int ReturnCapacity()
    {
        return maxCapacity - UserDataBase.Instance.userData.Invendata.Count;
    }

    //public void ItemCheck(Slot slot)
    //{
    //    if (slot._myitem.id != 0) // 슬롯에서 처리하지 않고 인벤토리 매니저에서 처리할지 확인
    //    {
    //        slot.isEmpty = false;
    //        slot.button.interactable = true;
    //        slot.itemimage.gameObject.SetActive(true); // 이미지 표시

    //        if (slot._myitem.force < 1)
    //        {
    //            slot.itemforcetext.text = "";
    //        }
    //        else
    //        {
    //            slot.itemforcetext.text = "+" + slot._myitem.force.ToString();
    //        }
    //    }
    //    else
    //    {
    //        slot.isEmpty = true;
    //        slot.button.interactable = false;
    //        slot.itemimage.gameObject.SetActive(false);
    //        slot.itemforcetext.text = "";
    //    }
    //}

    // 등급별(None/normal/rare/unique/legend/epic) 판매 기본가 (== 0강 기준 기본 장비가격, 등급 오를 때마다 2배)
    [NonSerialized]public int[] sellPriceByGrade = new int[6] { 0, 1000, 2000, 4000, 8000, 16000 };
    // [사용 안 함] 예전 강화 수치별 판매 추가가. 현재 판매가는 GetSellPrice()에서 (누적 강화 단계 × 5,000)으로 계산합니다.
    [NonSerialized]public int[] sellPriceByForce = new int[13] { 0, 20, 40, 60, 80, 100, 150, 200, 300, 400, 600, 900, 1500 };

    // [사용 안 함] 예전 강화 비용 테이블. 현재 강화 비용은 GetEnhanceCost()에서 (누적 강화 단계 × 1,000)으로 계산합니다.
    // (배열 길이는 강화 가능 최대 단계 경계 검사에만 사용됩니다.)
    [NonSerialized]public int[] goldvalue = new int[12] { 100, 200, 300, 400, 500, 600,
        700, 800, 900, 1000, 1500, 2000 };

    // 강화 비용 = (누적 강화 단계 + 1) × 1,000  → 첫 강화(+0)부터 1,000골드
    public int GetEnhanceCost(int force)
    {
        return (force + 1) * 1000;
    }
    public bool GoldCheck(int force)
    {
        var _gold = UserDataBase.Instance.userData.gamegold;
        bool check = false;
        switch (force)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
                if (_gold >= GetEnhanceCost(force))
                    check = true;
                else
                    check = false;
                break;
        }

        return check;
    }
    public void SetBtn(int _menu)
    {
        switch (_menu)
        {
            case 0:
                OnClickInfoUpgradeButton();
                break;
            case 1:
                OnClickUpgradeButton();
                break;
            case 2:
                OnClickEquipButton();
                break;
            case 3:
                OnClickUnEquipButton();
                break;
            case 4:
                OnClickSellButton();
                break;
        }
    }

    public void CloseGuidePopup()
    {
        SelectSlot = null;
        equipmentSlot = null;
        _myitem = new MyItemData();
        _myequipitem = new MyEquipmentItemData();
    }

    public void OnClickInfoUpgradeButton() // 업그레이드 팝업 열기
    {
        //InvenPopup_Upgrade.SetActive(true);
        //LobbyManager.Instance.GuidePopup.SetActive(false);

        if (SelectSlot != null)
        {
            InventoryPopup.OpenEnhancePopUpUI(SelectSlot, GetEnhanceCost(SelectSlot._myitem.force));
            //LoonSetting(itemimage, itemtypeImage, (int)SelectSlot._item.elementType, SelectSlot._item.imageId);
        }
        else if (equipmentSlot != null)
        {
            InventoryPopup.OpenEnhancePopUpUI(equipmentSlot, GetEnhanceCost(equipmentSlot.equipmentItemData.force));
            //LoonSetting(itemimage, itemtypeImage, (int)equipmentSlot._item.elementType, equipmentSlot._item.imageId);
        }
    }

    public List<MyItemData> materials = new List<MyItemData>();
    public void OnClickUpgradeButton() // 강화 버튼 클릭 이벤트
    {
        int slotnum = -1;

        if (isUpgradeRunning) return;

        InventoryPopup.enhance_enhanceButton.interactable = false;

        if (_myitem != null && SelectSlot != null)
        {
            if (_myitem.force < 12)
            {
                if (!GoldCheck(_myitem.force))
                {
                    //CloseUpgradeGuide();
                    //LobbyManager.Instance.GuidePopup.SetActive(false);
                    InventoryPopup.CloseEnhancePopupUI();
                    GameManager.gInstance.PopUpOpen("nogold");
                    return;
                }
                else
                {
                    if (!_myitem.isForce)
                    {
                        if (APIData.Instance.UpgradeKeyValue.ContainsKey(_myitem.force))
                        {
                            if(APIData.Instance.UpgradeKeyValue.GetValueOrDefault(_myitem.force) > materials.Count)
                            {
                                //CloseUpgradeGuide();
                                //LobbyManager.Instance.GuidePopup.SetActive(false);
                                InventoryPopup.CloseEnhancePopupUI();
                                GameManager.gInstance.PopUpOpen("nomaterial");
                                return;
                            }
                        }
                    }
                    // 강화
                    ConfirmAndStartUpgrade(_myitem);
                }
            }
            else
            {
                return;
            }
        }
        else if (_myequipitem != null && equipmentSlot != null)
        {
            if (_myequipitem.force < 12)
            {
                if (!GoldCheck(_myequipitem.force))
                {
                    //CloseUpgradeGuide();
                    //LobbyManager.Instance.GuidePopup.SetActive(false);
                    InventoryPopup.CloseEnhancePopupUI();
                    GameManager.gInstance.PopUpOpen("nogold");
                    return;
                }
                else
                {
                    if (!_myequipitem.isForce)
                    {
                        if (APIData.Instance.UpgradeKeyValue.ContainsKey(_myequipitem.force))
                        {
                            if (APIData.Instance.UpgradeKeyValue.GetValueOrDefault(_myequipitem.force) > materials.Count)
                            {
                                //CloseUpgradeGuide();
                                //LobbyManager.Instance.GuidePopup.SetActive(false);
                                InventoryPopup.CloseEnhancePopupUI();
                                GameManager.gInstance.PopUpOpen("nomaterial");
                                return;
                            }
                        }
                    }
                    // 강화
                    ConfirmAndStartUpgrade(_myequipitem);
                }
            }
            else
            {
                return;
            }
        }
        else return;
    }

    /// <summary>강화된 장비가 재료로 소모될 때는 확인을 받은 뒤 강화합니다. 취소하면 강화 버튼을 다시 누를 수 있게 둡니다.</summary>
    void ConfirmAndStartUpgrade(MyItemData itemData)
    {
        if (!GetMaterialsToConsume(itemData).Any(material => material.force > 0))
        {
            StartCoroutine(UpgradeEquipment(itemData));
            return;
        }

        GameManager.gInstance.PopUpConfirm(EnhanceMaterialWarningKey,
            () => StartCoroutine(UpgradeEquipment(itemData)),
            () => InventoryPopup.enhance_enhanceButton.interactable = true);
    }

    /// <summary>이번 강화에서 소모될 재료. 강화 수치가 낮은(+0) 재료부터 필요한 개수만큼 씁니다.</summary>
    List<MyItemData> GetMaterialsToConsume(MyItemData itemData)
    {
        if (itemData == null || itemData.isForce || !APIData.Instance.UpgradeKeyValue.TryGetValue(itemData.force, out int requiredMaterialCount))
            return new List<MyItemData>();

        // 목록을 만들 때도 정렬하지만, 소모를 정하는 이 한 곳에서 순서를 보장합니다. (같은 수치면 기존 순서 유지)
        return materials.Where(material => material != null)
            .OrderBy(material => material.force)
            .Take(Mathf.Max(0, requiredMaterialCount))
            .ToList();
    }

    int[] upgradePercent = new int[12] { 0, 10, 15, 20, 25, 30, 35, 40, 50, 60, 75, 95 };
    public bool upgradeCheck(int force)
    {
        int rand = UnityEngine.Random.Range(0, 100);
        bool check = false;
        switch (force)
        {
            case 0:case 1:case 2:case 3:case 4:case 5:case 6:case 7:case 8:case 9:case 10:case 11:
                if (rand >= upgradePercent[force])
                    check = true;
                else
                    check = false;
                break;
        }

        if (GameManager.gInstance.IsDev)
            check = true;

        return check;
    }

    IEnumerator UpgradeEquipment(MyItemData itemData) // 강화 함수
    {
        if (isUpgradeRunning)
            yield break;

        isUpgradeRunning = true;

        // 방금 해제한 장비를 재료로 쓰는 경우처럼 서버가 아직 처리하지 않은 장착/해제와 섞이지 않게 기다립니다.
        yield return WaitForEquipmentSync();

        if (itemData == null || itemData.force < 0 || itemData.force >= goldvalue.Length)
        {
            isUpgradeRunning = false;
            yield break;
        }

        List<MyItemData> consumeMaterials = new List<MyItemData>();
        if (!itemData.isForce && APIData.Instance.UpgradeKeyValue.TryGetValue(itemData.force, out int requiredMaterialCount))
        {
            requiredMaterialCount = Mathf.Max(0, requiredMaterialCount);
            if (materials.Count < requiredMaterialCount)
            {
                InventoryPopup.CloseEnhancePopupUI();
                GameManager.gInstance.PopUpOpen("nomaterial");
                isUpgradeRunning = false;
                yield break;
            }

            consumeMaterials = GetMaterialsToConsume(itemData);
        }

        // 골드 차감은 서버가 처리하고, 강화 성공/실패 응답의 gamegold로 반영됩니다. (사전 잔액 확인은 GoldCheck)
        int upgradeGold = GetEnhanceCost(itemData.force);
        bool upgradeSucceeded = upgradeCheck(itemData.force);
        bool serverConfirmed = false;

        if (upgradeSucceeded)
            yield return StartCoroutine(APIData.Instance.UnityWebRequestequipmentsuccessPut(itemData.inventoryID, upgradeGold, () => serverConfirmed = true));
        else
            yield return StartCoroutine(APIData.Instance.UnityWebRequestequipmentfailPut(itemData.inventoryID, upgradeGold, () => serverConfirmed = true));

        // 서버가 결과를 기록한 뒤에만 재료와 강화 수치를 바꿉니다. 요청이 실패하면 잃는 것이 없어 그대로 다시 시도할 수 있습니다.
        if (serverConfirmed)
        {
            yield return StartCoroutine(ConsumeUpgradeMaterials(consumeMaterials));

            ApplyUpgradeResult(itemData, upgradeSucceeded);

            if (upgradeSucceeded)
                yield return StartCoroutine(EnhanceAnimCor());
            else
                yield return StartCoroutine(EnhanceFailAnimCor());

            FreshSlot();
        }
        else
        {
            GameManager.gInstance.PopUpOpen(EnhanceFailedKey);
        }

        if (itemData.force < 12)
        {
            InventoryPopup.UpdateEnhancePopupUIText(itemData, GetEnhanceCost(itemData.force));
        }
        else
        {
            InventoryPopup.enhance_enhanceButton.gameObject.SetActive(false);
            InventoryPopup.Invoke("CloseEnhancePopupUI", 0.5f);
        }

        isUpgradeRunning = false;
    }

    /// <summary>
    /// 강화 결과가 서버에 기록된 뒤 재료를 삭제합니다. 실패한 재료는 몇 번 더 보내고,
    /// 그래도 남으면 서버 인벤토리를 다시 받아 화면을 서버와 맞춥니다.
    /// (결과 기록과 재료 삭제가 별도 요청이라 완전한 원자성은 서버 API가 묶어 줘야 합니다.)
    /// </summary>
    IEnumerator ConsumeUpgradeMaterials(List<MyItemData> consumeMaterials)
    {
        List<MyItemData> failedMaterials = new List<MyItemData>();

        foreach (MyItemData material in consumeMaterials)
        {
            if (!Inventory.Any(item => item.inventoryID == material.inventoryID))
                continue;

            bool deleted = false;
            for (int attempt = 0; attempt <= MaterialDeleteRetryCount && !deleted; attempt++)
                yield return StartCoroutine(APIData.Instance.UnityWebRequestinventoryPost(material.inventoryID, () => deleted = true));

            if (!deleted)
                failedMaterials.Add(material);
        }

        // 삭제된 재료만 목록에서 뺍니다. 실패한 재료는 아직 서버에 남아 있습니다.
        materials.RemoveAll(material => material != null
            && consumeMaterials.Any(consumeMaterial => consumeMaterial.inventoryID == material.inventoryID)
            && !failedMaterials.Any(failed => failed.inventoryID == material.inventoryID));

        if (failedMaterials.Count > 0)
        {
            Debug.LogWarning($"강화 재료 삭제 실패: {string.Join(", ", failedMaterials.Select(material => material.inventoryID))}. 서버 인벤토리를 다시 받아옵니다.");
            yield return StartCoroutine(APIData.Instance.UnityWebRequestaccountinventoryGET());

            // 서버에 없는 재료가 다음 강화의 재료 개수에 잡히지 않도록 목록을 서버 인벤토리 기준으로 줄입니다.
            materials.RemoveAll(material => material == null || !Inventory.Any(item => item.inventoryID == material.inventoryID));
        }
    }

    // 응답을 기다리는 동안 창을 닫거나 다른 슬롯을 열 수 있으므로, 현재 선택 상태가 아니라 inventoryID로 대상을 찾습니다.
    void ApplyUpgradeResult(MyItemData itemData, bool succeeded)
    {
        int force = succeeded ? itemData.force + 1 : itemData.force;
        bool materialsPaid = !succeeded; // 실패하면 재료를 이미 지불했으므로 다음 시도에는 재료를 받지 않습니다.
        int slotIndex = Array.FindIndex(EquippedItems, item => item.inventoryID == itemData.inventoryID);

        if (slotIndex >= 0)
        {
            EquipmentSlot slot = EquipSlot[slotIndex];
            MyEquipmentItemData equipped = EquippedItems[slotIndex];

            slot.set_equipmentdata_in_userstat_minus();
            equipped.force = force;
            equipped.isForce = materialsPaid;
            slot.ShowItem(equipped, slot._item);
            slot.set_equipmentdata_in_userstat_plus();
        }

        itemData.force = force;
        itemData.isForce = materialsPaid;

        if (slotIndex < 0)
            ReplaceInventoryItem(itemData);
    }

    // 재료 삭제 응답이 Invendata를 서버 객체로 교체했을 수 있어, 강화 대상 객체를 목록에 다시 연결합니다.
    void ReplaceInventoryItem(MyItemData itemData)
    {
        int index = Inventory.FindIndex(item => item.inventoryID == itemData.inventoryID);
        if (index >= 0)
            Inventory[index] = itemData;
    }

    public IEnumerator EnhanceAnimCor()
    {
        GameAudioManager.Ensure().PlayEnhanceSuccessSfx();

        InventoryPopup.enhance_enhanceButton.interactable = false;
        enhanceAnim.gameObject.SetActive(true);
        enhanceAnim.Play("enhanceAnim", -1);

        yield return new WaitForSeconds(enhanceAnim.GetCurrentAnimatorStateInfo(0).length);

        enhanceAnim.Rebind();
        enhanceAnim.gameObject.SetActive(false);
        InventoryPopup.enhance_enhanceButton.interactable = true;
    }

    public IEnumerator EnhanceFailAnimCor()
    {
        GameAudioManager.Ensure().PlayEnhanceFailSfx();

        InventoryPopup.enhance_enhanceButton.interactable = false;
        enhancefailAnim.gameObject.SetActive(true);
        enhancefailAnim.Play("enhancefailAnim", -1);

        yield return new WaitForSeconds(enhancefailAnim.GetCurrentAnimatorStateInfo(0).length);

        enhancefailAnim.Rebind();
        enhancefailAnim.gameObject.SetActive(false);
        InventoryPopup.enhance_enhanceButton.interactable = true;
    }

    void OnClickSlot(int i) // 인벤토리 슬롯 클릭 이벤트
    {

        // 선택 판매 모드에서는 정보 팝업 대신 선택 토글로 동작
        if (isSelectSellMode)
        {
            ToggleSelectItem(slots[i]);
            return;
        }

        if (slots[i]._myitem.force > 11)
            InventoryPopup.iteminfo_enhanceButton.gameObject.SetActive(false);
        else
            InventoryPopup.iteminfo_enhanceButton.gameObject.SetActive(true);

        InventoryPopup.iteminfo_equipButton.gameObject.SetActive(true);
        InventoryPopup.iteminfo_unequipButton.gameObject.SetActive(false);

        InventoryPopup.OpenItemInfoPopUpUI(slots[i]);

        SelectSlot = slots[i]; // 선택한 슬롯 저장
        _myitem = slots[i]._myitem;

        materials.Clear();

        for (int j = 0; j < UserDataBase.Instance.userData.Invendata.Count; j++)
        {
            if (_myitem.id.Equals(UserDataBase.Instance.userData.Invendata[j].id))
            {
                if (!_myitem.inventoryID.Equals(UserDataBase.Instance.userData.Invendata[j].inventoryID)) // 강화 대상 아이템 자신은 재료에서 제외
                {
                    if (_myitem.force >= UserDataBase.Instance.userData.Invendata[j].force)
                        materials.Add(UserDataBase.Instance.userData.Invendata[j]);
                }
            }
            else
                continue;
        }
        materials = materials.OrderBy(x => x.force).ToList();
    }

    void OnClickEquipSlot(int i) // 장비 슬롯 클릭
    {
        // 선택 판매 모드에서는 장착 슬롯을 눌러도 정보 팝업을 열지 않습니다. (판매 선택 UI 위에 겹쳐 보이는 문제 방지)
        if (isSelectSellMode)
            return;

        if (EquipSlot[i].equipmentItemData.force > 11)
            InventoryPopup.iteminfo_enhanceButton.gameObject.SetActive(false);
        else
            InventoryPopup.iteminfo_enhanceButton.gameObject.SetActive(true);

        InventoryPopup.iteminfo_equipButton.gameObject.SetActive(false);
        InventoryPopup.iteminfo_unequipButton.gameObject.SetActive(true);

        InventoryPopup.OpenItemInfoPopUpUI(EquipSlot[i]);

        equipmentSlot = EquipSlot[i];
        _myequipitem = EquipSlot[i].equipmentItemData;

        materials.Clear();

        for (int k = 0; k < UserDataBase.Instance.userData.Invendata.Count; k++)
        {
            if (_myequipitem.id.Equals(UserDataBase.Instance.userData.Invendata[k].id))
            {
                if (!_myequipitem.inventoryID.Equals(UserDataBase.Instance.userData.Invendata[k].inventoryID)) // 강화 대상 아이템 자신은 재료에서 제외
                {
                    if (_myequipitem.force >= UserDataBase.Instance.userData.Invendata[k].force)
                        materials.Add(UserDataBase.Instance.userData.Invendata[k]);
                }
            }
            else
                continue;
        }
        materials = materials.OrderBy(x => x.force).ToList();
    }

    public void OnClickEquipButton() // 장비 장착
    {
        if (SelectSlot == null)
            return;

        EquipItem(_myitem.inventoryID);

        SelectSlot = null;
        _myitem = new MyItemData();
        InventoryPopup.ItemInfo.SetActive(false);
        // 교체 화면이 열렸으면 같은 팝업 캔버스를 쓰므로 닫지 않습니다. (교체를 끝내거나 취소할 때 닫힘)
        if (!IsSwapSelecting)
            InventoryPopup.GuideCanvas_InventoryPopup.SetActive(false);
    }

    public void OnClickUnEquipButton() // 장비 해제
    {
        if (equipmentSlot == null)
            return;

        // 인벤토리 칸을 넘는 장비는 화면에 보이지 않으므로 가득 찼으면 해제하지 않습니다. (교체는 개수가 그대로라 허용)
        if (Inventory.Count >= maxCapacity)
        {
            GameManager.gInstance.PopUpOpen(UnequipInventoryFullKey);
            return;
        }

        UnequipSlot(Array.IndexOf(EquipSlot, equipmentSlot));

        equipmentSlot = null;
        _myequipitem = new MyEquipmentItemData();
        InventoryPopup.GuideCanvas_InventoryPopup.SetActive(false);
        InventoryPopup.ItemInfo.SetActive(false);
    }

    void EquipItem(int inventoryID)
    {
        if (isEquipmentResyncing)
            return;

        int slotIndex = EquipmentChangeQueue.FindEmptySlot(EquippedItems);
        if (slotIndex < 0)
        {
            BeginSwapSelect(inventoryID);
            return;
        }

        if (equipmentChanges.ApplyEquip(Inventory, EquippedItems, inventoryID, slotIndex) == null)
        {
            Debug.LogWarning($"장착 대상을 인벤토리에서 찾지 못했습니다. inventoryID: {inventoryID}");
            return;
        }

        OnEquipmentChanged(slotIndex);
    }

    void UnequipSlot(int slotIndex)
    {
        if (isEquipmentResyncing)
            return;

        if (equipmentChanges.ApplyUnequip(Inventory, EquippedItems, slotIndex) == null)
        {
            Debug.LogWarning($"해제할 장비가 없습니다. slot: {slotIndex}");
            return;
        }

        OnEquipmentChanged(slotIndex);
    }

    // 장착 슬롯이 가득 찼을 때: 스킬 교체처럼 확인창 → 교체할 칸 선택 순서로 진행합니다.
    void BeginSwapSelect(int inventoryID)
    {
        MyItemData item = Inventory.FirstOrDefault(x => x.inventoryID == inventoryID);
        if (item == null || swapView == null)
        {
            Debug.LogError(swapView == null ? "EquipmentSwapView가 연결되지 않았습니다." : $"교체할 장비를 찾지 못했습니다. inventoryID: {inventoryID}");
            return;
        }

        swapInventoryID = inventoryID;
        InventoryPopup.GuideCanvas_InventoryPopup.SetActive(true);
        swapView.ShowAsk(APIData.Instance.GetItem<StaticItemData>(item.id), item.force);
    }

    // 확인창에서 '예': 실제 장착 칸 위에 현재 장착 장비를 겹쳐 보여 줍니다.
    void OnSwapConfirmed()
    {
        if (!IsSwapSelecting)
            return;

        EquipmentSwapView.SlotIcon[] icons = new EquipmentSwapView.SlotIcon[EquipSlot.Length];
        for (int i = 0; i < EquipSlot.Length; i++)
            icons[i] = new EquipmentSwapView.SlotIcon(EquipSlot[i]._item, EquippedItems[i].force, (RectTransform)EquipSlot[i].transform);

        swapView.ShowSlotSelect(icons);
    }

    // 칸을 고르면 한 번 더 확인합니다. '아니요'면 칸 선택 화면에 그대로 남아 다른 칸을 고를 수 있습니다.
    void OnSwapSlotSelected(int slotIndex)
    {
        if (!IsSwapSelecting)
            return;

        GameManager.gInstance.PopUpConfirm(SwapConfirmKey, () =>
        {
            // 확인창이 떠 있는 동안 탭 이동 등으로 교체가 취소됐을 수 있습니다.
            if (IsSwapSelecting)
                SwapEquip(slotIndex);
        });
    }

    void OnSwapCancelled()
    {
        CancelSwapSelect();
    }

    /// <summary>교체 선택을 끝내고 교체 화면을 닫습니다. 선택 중이었으면 true (뒤로 가기 처리에서 사용)</summary>
    public bool CancelSwapSelect()
    {
        if (!IsSwapSelecting)
            return false;

        swapInventoryID = 0;
        swapView.Hide();
        InventoryPopup.GuideCanvas_InventoryPopup.SetActive(false);
        return true;
    }

    // 해제 후 같은 슬롯에 장착하는 두 변경을 대기열에 차례로 넣습니다. 인벤토리 개수는 그대로입니다.
    void SwapEquip(int slotIndex)
    {
        int inventoryID = swapInventoryID;
        CancelSwapSelect();

        if (isEquipmentResyncing || !EquipmentChangeQueue.IsOccupied(EquippedItems[slotIndex])
            || !Inventory.Any(item => item.inventoryID == inventoryID))
            return;

        equipmentChanges.ApplyUnequip(Inventory, EquippedItems, slotIndex);
        equipmentChanges.ApplyEquip(Inventory, EquippedItems, inventoryID, slotIndex);
        OnEquipmentChanged(slotIndex);
    }

    // 탭한 프레임에 화면을 바꾸고, 서버 확정은 대기열이 뒤에서 처리합니다.
    void OnEquipmentChanged(int slotIndex)
    {
        SyncEquipSlot(slotIndex);
        FreshSlot();

        if (equipmentSyncRoutine == null)
            equipmentSyncRoutine = StartCoroutine(SyncEquipmentChanges());
    }

    /// <summary>장착 슬롯 View와 장비 스탯을 모델(equipData)에 맞춥니다. 표시 중이던 장비의 스탯을 빼고 새 장비의 스탯을 더합니다.</summary>
    void SyncEquipSlot(int slotIndex)
    {
        EquipmentSlot slot = EquipSlot[slotIndex];
        MyEquipmentItemData data = EquippedItems[slotIndex];

        if (!slot.isEmpty)
            slot.set_equipmentdata_in_userstat_minus();

        if (EquipmentChangeQueue.IsOccupied(data))
        {
            slot.ShowItem(data, APIData.Instance.GetItem<StaticItemData>(data.id));
            slot.set_equipmentdata_in_userstat_plus();
        }
        else
        {
            slot.ShowEmpty();
        }
    }

    // 서버 응답의 슬롯 수가 화면과 달라도 슬롯 번호로 안전하게 접근할 수 있게 맞춥니다.
    void NormalizeEquippedItems()
    {
        MyEquipmentItemData[] normalized = new MyEquipmentItemData[EquipSlot.Length];
        MyEquipmentItemData[] source = EquippedItems;

        for (int i = 0; i < normalized.Length; i++)
            normalized[i] = source != null && i < source.Length && source[i] != null ? source[i] : new MyEquipmentItemData();

        UserDataBase.Instance.userData.equipData = normalized;
    }

    /// <summary>대기 중인 변경을 입력 순서대로 한 번에 하나씩 서버에 보냅니다. 실패하면 나머지까지 함께 되돌립니다.</summary>
    IEnumerator SyncEquipmentChanges()
    {
        while (equipmentChanges.Current != null)
        {
            EquipmentChange change = equipmentChanges.Current;
            bool succeeded = false;

            if (change.Type == EquipmentChangeType.Equip)
            {
                yield return StartCoroutine(APIData.Instance.UnityWebRequestinventoryPost(change.Item.inventoryID, change.SlotIndex, slotId =>
                {
                    change.Item.slot_id = slotId;
                    succeeded = true;
                }));
            }
            else if (change.Item.slot_id > 0)
            {
                yield return StartCoroutine(APIData.Instance.UnityWebRequestinventoryDelete(change.Item.slot_id, () => succeeded = true));
            }
            else
            {
                Debug.LogWarning($"slot_id가 없어 해제 요청을 보낼 수 없습니다. inventoryID: {change.Item.inventoryID}");
            }

            // 서버 점검 중에는 요청 함수가 콜백 없이 끝나므로 성공 통지가 없으면 모두 실패로 처리합니다.
            if (!succeeded)
            {
                RollbackEquipmentChanges();
                break;
            }

            equipmentChanges.CompleteCurrent();
        }

        equipmentSyncRoutine = null;
    }

    void RollbackEquipmentChanges()
    {
        foreach (int slotIndex in equipmentChanges.RollbackAll(Inventory, EquippedItems))
            SyncEquipSlot(slotIndex);

        FreshSlot();

        // 서버는 처리했는데 응답만 끊긴 경우 되돌린 화면이 서버와 다를 수 있어 서버 상태를 다시 받아옵니다.
        // 안내는 재조회가 끝난 뒤 실제 결과(서버 상태로 맞춤 / 이전 상태로 되돌림)에 맞춰 띄웁니다.
        if (isEquipmentResyncing)
            GameManager.gInstance.PopUpOpen(EquipChangeFailedKey);
        else
            StartCoroutine(ResyncAfterFailure());
    }

    IEnumerator ResyncAfterFailure()
    {
        bool synced = false;
        yield return StartCoroutine(ResyncEquipmentFromServer(result => synced = result));
        GameManager.gInstance.PopUpOpen(synced ? EquipChangeSyncedKey : EquipChangeFailedKey);
    }

    /// <summary>장착 슬롯과 인벤토리를 서버 값으로 다시 맞춥니다. 실패하면 잠시 뒤 다시 시도합니다.</summary>
    IEnumerator ResyncEquipmentFromServer(Action<bool> onFinished)
    {
        isEquipmentResyncing = true;
        CancelSwapSelect();

        bool synced = false;
        for (int attempt = 0; attempt < EquipmentResyncAttempts && !synced; attempt++)
        {
            if (attempt > 0)
                yield return new WaitForSecondsRealtime(EquipmentResyncRetryDelaySeconds);

            bool slotLoaded = false;
            yield return StartCoroutine(APIData.Instance.UnityWebRequestaccountslotGET(() =>
            {
                UserDataBase.Instance.GetSlot();
                slotLoaded = true;
            }));
            if (!slotLoaded)
                continue;

            NormalizeEquippedItems();
            for (int i = 0; i < EquipSlot.Length; i++)
                SyncEquipSlot(i);
            EquipmentChangeQueue.RemoveEquipped(Inventory, EquippedItems);
            FreshSlot();

            // 서버에서 해제된 장비가 인벤토리에 들어오도록 목록도 다시 받습니다. (SetInventoryData에서 화면 갱신)
            yield return StartCoroutine(APIData.Instance.UnityWebRequestaccountinventoryGET(() => synced = true));
        }

        if (!synced)
            Debug.LogWarning("장비 상태를 서버에서 다시 받아오지 못했습니다. 다음 장비 응답이나 재접속 때 맞춰집니다.");

        isEquipmentResyncing = false;
        onFinished?.Invoke(synced);
    }

    /// <summary>서버 인벤토리 목록을 모델에 넣기 전에 현재 장착 상태에 맞춥니다. (APIData의 단일 진입점에서 호출)</summary>
    public void ReconcileServerInventory(List<MyItemData> serverInventory)
    {
        if (EquippedItems != null)
            equipmentChanges.Reconcile(serverInventory, EquippedItems);
    }

    /// <summary>판매·강화·씬 전환처럼 서버의 장착 상태에 의존하는 작업 전에 대기 변경이 끝나길 기다립니다.</summary>
    public IEnumerator WaitForEquipmentSync()
    {
        while (equipmentChanges.HasPending || isEquipmentResyncing)
            yield return null;
    }

    public int GetSellPrice(StaticItemData item, int force)
    {
        // 판매 가격 = 기본 장비가격(등급 0강) + (누적 강화 단계 × 기본 장비가격) = 기본가 × (강화 + 1)
        int gradePrice = (int)item.rankType < sellPriceByGrade.Length ? sellPriceByGrade[(int)item.rankType] : 0;
        return gradePrice + force * gradePrice;
    }

    public void OnClickSellButton()
    {
        if (SelectSlot == null || SelectSlot._myitem.id == 0) return;

        int price = GetSellPrice(SelectSlot._item, SelectSlot._myitem.force);

        if (InventoryPopup.confirm_sellPriceText != null)
            InventoryPopup.confirm_sellPriceText.text = price.ToString();

        InventoryPopup.really_sell_questionPopup.SetActive(true);
    }

    public void OnClickConfirmSellButton()
    {
        // 응답을 기다리는 동안 팝업이 열려 있으므로 연타해도 판매 요청은 한 번만 보냅니다.
        if (isSellRunning)
            return;

        // 선택 판매 모드면 다중 판매로 처리
        if (isSelectSellMode)
        {
            ConfirmSelectSell();
            return;
        }

        if (SelectSlot == null || SelectSlot._myitem.id == 0) return;

        int price = GetSellPrice(SelectSlot._item, SelectSlot._myitem.force);
        StartCoroutine(SellItem(SelectSlot._myitem, price));
    }

    IEnumerator SellItem(MyItemData item, int price)
    {
        // 골드 증가/인벤토리 갱신은 판매 응답(gamegold, inventoryList)으로 반영됩니다.
        yield return StartCoroutine(SellItems(new List<int> { item.inventoryID }, price));
    }

    /// <summary>
    /// 장비 여러 개를 한 번에 판매합니다. (선택 판매 확장용)
    /// </summary>
    /// <param name="inventoryIds">판매할 인벤토리 ID 목록입니다.</param>
    /// <param name="totalPrice">판매로 받을 골드 총액입니다.</param>
    public IEnumerator SellItems(List<int> inventoryIds, int totalPrice)
    {
        SetSellRunning(true);

        // 방금 해제한 장비는 서버 처리 후에 팔 수 있고, 해제가 실패해 장착으로 되돌아갔다면 팔지 않습니다.
        yield return WaitForEquipmentSync();

        bool sellFailed = false;
        if (inventoryIds.All(id => Inventory.Any(item => item.inventoryID == id)))
            yield return StartCoroutine(APIData.Instance.UnityWebRequestinventorySellPost(inventoryIds, totalPrice, FreshSlot, () => sellFailed = true));

        InventoryPopup.really_sell_questionPopup.SetActive(false);
        InventoryPopup.GuideCanvas_InventoryPopup.SetActive(false);
        InventoryPopup.ItemInfo.SetActive(false);
        CloseGuidePopup();

        SetSellRunning(false);

        if (sellFailed)
            GameManager.gInstance.PopUpOpen(SellFailedKey);
    }

    // 처리 중에는 확인창의 버튼을 모두 막습니다. 팝업을 닫고 판매 중인 장비를 장착하는 경합을 차단합니다.
    void SetSellRunning(bool running)
    {
        isSellRunning = running;
        foreach (Button button in sellConfirmButtons)
            button.interactable = !running;
    }

    // ===================== 선택 판매(다중) =====================

    /// <summary>선택 판매 모드 진입/해제를 토글합니다.</summary>
    public void ToggleSelectSellMode()
    {
        if (isSelectSellMode) ExitSelectSellMode();
        else EnterSelectSellMode();
    }

    public void EnterSelectSellMode()
    {
        CancelSwapSelect();
        isSelectSellMode = true;
        selectedSellIds.Clear();

        // 열려 있는 정보/강화 팝업 닫기
        InventoryPopup.GuideCanvas_InventoryPopup.SetActive(false);
        InventoryPopup.ItemInfo.SetActive(false);

        if (selectSellBar != null) selectSellBar.SetActive(true);
        UpdateSelectSellUI();
        FreshSlot();
    }

    public void ExitSelectSellMode()
    {
        isSelectSellMode = false;
        selectedSellIds.Clear();

        if (selectSellBar != null) selectSellBar.SetActive(false);
        if (InventoryPopup.really_sell_questionPopup != null)
            InventoryPopup.really_sell_questionPopup.SetActive(false);

        FreshSlot();
    }

    /// <summary>
    /// 장비/스킬 탭 전환 시 탭 View(MyBagManager)에서 호출합니다.
    /// 선택 판매는 장비에만 해당하므로, 스킬 탭에서는 진입 버튼을 숨기고
    /// 선택 판매 모드였다면 해제합니다. (장비 탭에서만 버튼이 보이도록 소유자가 직접 관리)
    /// </summary>
    public void SetSelectSellAvailable(bool available)
    {
        if (!available && isSelectSellMode)
            ExitSelectSellMode(); // selectSellBar 숨김·선택 초기화 포함

        if (selectSellModeButton != null)
            selectSellModeButton.gameObject.SetActive(available);
    }

    /// <summary>선택 판매 모드에서 슬롯 하나의 선택 상태를 토글합니다.</summary>
    void ToggleSelectItem(Slot slot)
    {
        if (slot == null || slot.isEmpty || slot._myitem == null || slot._myitem.id == 0)
            return;

        int id = slot._myitem.inventoryID;
        if (!selectedSellIds.Remove(id)) // 없으면 추가, 있으면 위 Remove로 해제됨
            selectedSellIds.Add(id);

        slot.SetSelected(selectedSellIds.Contains(id));
        UpdateSelectSellUI();
    }

    int GetSelectedSellTotal()
    {
        int total = 0;
        var inv = UserDataBase.Instance.userData.Invendata;
        foreach (var id in selectedSellIds)
        {
            var item = inv.FirstOrDefault(x => x.inventoryID == id);
            if (item == null) continue;
            var staticData = APIData.Instance.GetItem<StaticItemData>(item.id);
            total += GetSellPrice(staticData, item.force);
        }
        return total;
    }

    void UpdateSelectSellUI()
    {
        if (selectSellTotalText != null) selectSellTotalText.text = GetSelectedSellTotal().ToString("#,##0");
        if (selectSellCountText != null) selectSellCountText.text = selectedSellIds.Count.ToString();
        if (selectSellConfirmButton != null) selectSellConfirmButton.interactable = selectedSellIds.Count > 0;
    }

    /// <summary>선택 판매 실행 버튼: 확인 팝업을 엽니다.</summary>
    public void OnClickSelectSellConfirm()
    {
        if (selectedSellIds.Count == 0) return;

        if (InventoryPopup.confirm_sellPriceText != null)
            InventoryPopup.confirm_sellPriceText.text = GetSelectedSellTotal().ToString("#,##0");

        // really_sell_questionPopup은 이제 ItemInfo의 자식이 아니라 GuideCanvas_InventoryPopup 바로 아래
        // 독립된 형제 팝업이라, ItemInfo를 켤 필요 없이 부모(GuideCanvas_InventoryPopup)만 켜면 된다.
        InventoryPopup.GuideCanvas_InventoryPopup.SetActive(true);
        InventoryPopup.really_sell_questionPopup.SetActive(true);
    }

    /// <summary>판매 확인 팝업의 '아니오': 팝업을 닫습니다. 선택 판매 모드면 함께 켰던 부모 팝업도 닫아 선택 모드로 복귀합니다.</summary>
    public void OnClickConfirmNo()
    {
        // 판매 처리 중에는 뒤로 가기로도 닫히지 않습니다.
        if (isSellRunning)
            return;

        InventoryPopup.really_sell_questionPopup.SetActive(false);
        if (isSelectSellMode)
        {
            InventoryPopup.GuideCanvas_InventoryPopup.SetActive(false);
        }
    }

    /// <summary>확인 팝업의 예 버튼에서 호출: 선택한 장비들을 한 번에 판매합니다.</summary>
    void ConfirmSelectSell()
    {
        if (selectedSellIds.Count == 0)
        {
            InventoryPopup.really_sell_questionPopup.SetActive(false);
            return;
        }

        // 선택된 ID 중 현재 인벤토리에 실제로 존재하는 것만 전송합니다.
        // (선택 후 다른 경로로 사라진 아이템의 오래된 inventoryID가 섞여 있으면 서버가 요청 전체를 거부(400)할 수 있음)
        var inv = UserDataBase.Instance.userData.Invendata;
        List<int> ids = selectedSellIds.Where(id => inv.Any(x => x.inventoryID == id)).ToList();

        if (ids.Count == 0)
        {
            InventoryPopup.really_sell_questionPopup.SetActive(false);
            ExitSelectSellMode();
            return;
        }

        int total = 0;
        foreach (var id in ids)
        {
            var item = inv.FirstOrDefault(x => x.inventoryID == id);
            var staticData = APIData.Instance.GetItem<StaticItemData>(item.id);
            total += GetSellPrice(staticData, item.force);
        }

        StartCoroutine(SellSelectedItemsCor(ids, total));
    }

    IEnumerator SellSelectedItemsCor(List<int> ids, int total)
    {
        yield return StartCoroutine(SellItems(ids, total));
        ExitSelectSellMode();
    }
}
