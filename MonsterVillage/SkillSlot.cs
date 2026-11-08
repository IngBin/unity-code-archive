using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class SkillSlot : MonoBehaviour
{
    public MySkillData myskill;
    public SkillData skill;

    [SerializeField] private int id;

    public Image skillImage;
    public TextMeshProUGUI pieceText;

    [SerializeField] public int maxPiece { get; private set; } // 미정

    //private void OnEnable()
    //{
    //    SetSkillImage();
    //    id = skill.id;
    //}

    public void Init(MySkillData myskilldata) // 스킬 데이터 초기화
    {
        maxPiece = 15;

        myskill = myskilldata;

        skill = APIData.Instance.SkillKeyValue[myskill.id];

        id = skill.id;

        // Init 은 로딩뿐 아니라 런타임(계정 스킬 데이터 갱신/조각 획득)에도 다시 불린다.
        // 리스너를 먼저 제거하지 않으면 재호출마다 중복 등록되어 버튼 한 번에 여러 번 실행된다.
        var skillButton = this.gameObject.GetComponent<Button>();
        skillButton.onClick.RemoveListener(OnClickSkillButton);
        skillButton.onClick.AddListener(OnClickSkillButton); // 버튼 이벤트 추가


        // 장착 칸 배치·인게임 전달 값·코스트는 SkillPageManager(ApplySlotState/AssignSlot)가 맡습니다.
        // 여기서는 보유 목록 표시만 정하고, 장착된 스킬은 SkillPageManager가 바로 뒤에 숨깁니다.
        // (기존: 이미 장착된 스킬을 다시 초기화하면 자기 칸을 '다른 스킬이 있는 칸'으로 보고 미장착 처리하던 문제)
        // 보유(조각 15개 이상)한 스킬만 목록에 표시합니다. (기존: 조각 1~14개면 표시 상태를 바꾸지 않아 템플릿대로 보였음)
        this.gameObject.SetActive(IsOwned);

        SetSkillImage();
    }

    /// <summary>조각이 다 모여 보유 목록에 표시되는 스킬인지</summary>
    public bool IsOwned => maxPiece > 0 && myskill.skillpiece >= maxPiece;
    void SetSkillImage() // 스킬 이미지 세팅
    {
        if (skill.effectId == 0)
            return;

        if(APIData.Instance.AddressableSprite.TryGetValue($"{skill.effectId}_{skill.name}", out var sprite))
        {
            skillImage.sprite = sprite;
            return;
        }
    }

    public void OnClickSkillButton() // 스킬 버튼
    {
        SkillPageManager.instance.SelectSkill = this.gameObject;
        InventoryManager.Instance.InventoryPopup.OpenSkillInfoPopUpUI(skill, myskill);
    }
}
