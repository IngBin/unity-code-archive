using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillEquipSlot : MonoBehaviour
{
    public Sprite testimage;
    public bool isEquip
    {
        get
        {
            return skilldata != null;
        }
    }

    public Image skillimage;
    [SerializeField]public SkillSlot skilldata;

    private void OnEnable()
    {
        if (skilldata != null)
            StartCoroutine(SettingSkillCor());
        else
            this.GetComponent<Button>().interactable = false;
    }

    private void Awake()
    {
        this.GetComponent<Button>().onClick.AddListener(() => OnClickEquipSkill());
    }
    public void SettingSkill(SkillSlot _slotdata) // 장착
    {
        skilldata = _slotdata;
        this.GetComponent<Button>().interactable = true;
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(SettingSkillCor());
        else
            return;
    }
    IEnumerator SettingSkillCor() // 스킬 이미지 세팅
    {
        // 12차: 아이콘이 없는 스킬이어도 예외로 멈추지 않게 (멈추면 코스트만 남고 칸은 비어 보였음)
        if (skilldata == null)
            yield break;
        if (!APIData.Instance.AddressableSprite.TryGetValue($"{skilldata.skill.effectId}_{skilldata.skill.name}", out Sprite icon) || icon == null)
            yield break;
        skillimage.sprite = icon;

        yield return new WaitUntil(() => skillimage.GetComponent<Image>().sprite != null);

        skillimage.gameObject.SetActive(true);
    }
    /// <summary>칸 표시만 비웁니다. 장착 상태·서버 저장·코스트는 SkillPageManager.AssignSlot이 처리합니다.</summary>
    public void ClearSkill()
    {
        skillimage.gameObject.SetActive(false);
        skillimage.sprite = null;
        skilldata = null;
        this.GetComponent<Button>().interactable = false; // 빈 칸은 눌리지 않게 (OnEnable의 빈 칸 처리와 같음)
    }

    public SkillSlot returnSlotData()
    {
        if (skilldata == null)
            Debug.Log("데이터 없음");

        return skilldata;
    }

    public void OnClickEquipSkill() // 장착중인 슬롯 버튼 클릭
    {
        if (skilldata != null)
        {
            SkillPageManager.instance.SelectSkill = this.gameObject;

            InventoryManager.Instance.InventoryPopup.OpenSkillInfoPopUpUI(skilldata.skill, skilldata.myskill);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
