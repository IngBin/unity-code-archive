using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LanguageSingleton;

public class InventoryPopup : MonoBehaviour
{
    public GameObject GuideCanvas_InventoryPopup;

    public GameObject ItemInfo;
    public GameObject Enhance;
    public GameObject SkillInfo;

    public Sprite[] classSprites;

    public Image iteminfo_classImage;
    public Image iteminfo_itemImage;
    public Image iteminfo_typeImage;
    public TextMeshProUGUI iteminfo_name_forceText;
    public TextMeshProUGUI iteminfo_descText;
    public TextMeshProUGUI iteminfo_abilitiesText;
    public Button iteminfo_equipButton;
    public Button iteminfo_unequipButton;
    public Button iteminfo_enhanceButton;
    public Button iteminfo_sellButton;
    public TextMeshProUGUI iteminfo_sellPriceText;
    public GameObject iteminfo_sellPriceGroup; // gold_image + gold_sellprice_Text 묶음 부모

    public GameObject really_sell_questionPopup;
    public TextMeshProUGUI confirm_sellPriceText;
    public Button confirm_yesButton;
    public Button confirm_noButton;

    public Image enhance_classImage;
    public Image enhance_itemImage;
    public Image enhance_typeImage;
    public Image enhance_matImage;
    public Image enhance_matTypeImage;
    public TextMeshProUGUI enhance_currentForceText;
    public TextMeshProUGUI enhance_nextForceText;
    public TextMeshProUGUI enhance_matCount;
    public TextMeshProUGUI enhance_goldCount;
    public Button enhance_enhanceButton;

    public Image skillinfo_skillImage;
    public TextMeshProUGUI skillinfo_skillName;
    public TextMeshProUGUI skillinfo_skillCoast;
    public TextMeshProUGUI skillinfo_descText;
    public Button skillinfo_equipButton;
    public Button skillinfo_unequipButton;
    public Button skillinfo_buyButton;

    /// <summary>스킬 정보창이 닫힐 때 알립니다. (스킬 화면이 고른 스킬 선택을 비우는 데 사용)</summary>
    public event Action SkillInfoClosed;

    /// <summary>
    /// open iteminfo popup in GuideCanvas_inventoryPopup
    /// </summary>
    /// <param name="slotData">inventory slot data</param>
    public void OpenItemInfoPopUpUI(Slot slotData) 
    {
        if (slotData == null)
            return;

        GuideCanvas_InventoryPopup.SetActive(true);
        ItemInfo.SetActive(true);

        ApplyClassImage(iteminfo_classImage, slotData._item.rankType);

        iteminfo_itemImage.sprite = APIData.Instance.AddressableSprite["stoneline_" + (int)slotData._item.elementType];
        iteminfo_typeImage.sprite = APIData.Instance.AddressableSprite["Loon_" + slotData._item.imageId];

        iteminfo_name_forceText.text = slotData._item.LangName + " +" + slotData._myitem.force;
        iteminfo_descText.text = LangInstance.GetData(slotData._item.name + "_Desc");

        string abStr = "";

        if (slotData._item.stat != 0)
            abStr += $"· {LangInstance.GetData("iteminfo_stat_" + slotData._item.statType.ToString())} + {(slotData._item.stat + (slotData._item.statAdd * slotData._myitem.force))}%\n";

        if (slotData._item.elementType != 0 && slotData._item.elementType != ElementType.extra)
        {
            string element = LangInstance.GetData("iteminfo_element_" + slotData._item.elementType.ToString());

            if(slotData._item.elementStatType != 0)
                abStr += $"· {element} {LangInstance.GetData("iteminfo_elementStatType_" + slotData._item.elementStatType.ToString())} + {slotData._item.value + (slotData._item.valueAdd * slotData._myitem.force)}%\n";

            if (slotData._item.resist != 0)
                abStr += $"· {element} {LangInstance.GetData("iteminfo_resist")} + {slotData._item.resist + (slotData._item.resistAdd * slotData._myitem.force)}%\n";
        }
        if(slotData._item.lastType != 0 && slotData._myitem.force == 12)
            abStr += $"· {(slotData._item.lastType != 11 ? LangInstance.FormatString("iteminfo_last_" + slotData._item.lastType, slotData._item.last) : LangInstance.GetData("iteminfo_last_11"))}\n";

        iteminfo_abilitiesText.text = abStr;

        if (iteminfo_sellButton != null)
        {
            iteminfo_sellButton.gameObject.SetActive(true);
            int price = InventoryManager.Instance.GetSellPrice(slotData._item, slotData._myitem.force);
            if (iteminfo_sellPriceText != null)
                iteminfo_sellPriceText.text = price.ToString();
            if (iteminfo_sellPriceGroup != null)
                iteminfo_sellPriceGroup.SetActive(true);
        }
    }

    /// <summary>
    /// open iteminfo popup in GuideCanvas_inventoryPopup
    /// </summary>
    /// <param name="slotData">inventory equipmentslot data</param>
    public void OpenItemInfoPopUpUI(EquipmentSlot slotData)
    {
        if (slotData == null)
            return;

        GuideCanvas_InventoryPopup.SetActive(true);
        ItemInfo.SetActive(true);

        ApplyClassImage(iteminfo_classImage, slotData._item.rankType);

        iteminfo_itemImage.sprite = APIData.Instance.AddressableSprite["stoneline_" + (int)slotData._item.elementType];
        iteminfo_typeImage.sprite = APIData.Instance.AddressableSprite["Loon_" + slotData._item.imageId];

        iteminfo_name_forceText.text = slotData._item.LangName + " +" + slotData.equipmentItemData.force;
        iteminfo_descText.text = LangInstance.GetData(slotData._item.name + "_Desc");

        string abStr = "";

        if (slotData._item.stat != 0)
            abStr += $"· {LangInstance.GetData("iteminfo_stat_" + slotData._item.statType.ToString())} + {(slotData._item.stat + (slotData._item.statAdd * slotData.equipmentItemData.force))}%\n";

        if (slotData._item.elementType != 0 && slotData._item.elementType != ElementType.extra)
        {
            string element = LangInstance.GetData("iteminfo_element_" + slotData._item.elementType.ToString());

            if (slotData._item.elementStatType != 0)
                abStr += $"· {element} {LangInstance.GetData("iteminfo_elementStatType_" + slotData._item.elementStatType.ToString())} + {slotData._item.value + (slotData._item.valueAdd * slotData.equipmentItemData.force)}%\n";

            if (slotData._item.resist != 0)
                abStr += $"· {element} {LangInstance.GetData("iteminfo_resist")} + {slotData._item.resist + (slotData._item.resistAdd * slotData.equipmentItemData.force)}%\n";
        }
        if (slotData._item.lastType != 0 && slotData.equipmentItemData.force == 12)
            abStr += $"· {(slotData._item.lastType != 11 ? LangInstance.FormatString("iteminfo_last_" + slotData._item.lastType, slotData._item.last) : LangInstance.GetData("iteminfo_last_11"))}\n";

        iteminfo_abilitiesText.text = abStr;

        if (iteminfo_sellButton != null)
            iteminfo_sellButton.gameObject.SetActive(false);
        if (iteminfo_sellPriceGroup != null)
            iteminfo_sellPriceGroup.SetActive(false);
    }

    /// <summary>
    /// 가챠 결과창에서 장비 클릭 시 — force 없이 StaticItemData만으로 팝업 열기
    /// </summary>
    public void OpenItemInfoPopUpUI(StaticItemData item)
    {
        GuideCanvas_InventoryPopup.SetActive(true);
        ItemInfo.SetActive(true);

        ApplyClassImage(iteminfo_classImage, item.rankType);

        iteminfo_itemImage.sprite = APIData.Instance.AddressableSprite["stoneline_" + (int)item.elementType];
        iteminfo_typeImage.sprite = APIData.Instance.AddressableSprite["Loon_" + item.imageId];

        iteminfo_name_forceText.text = item.LangName;
        iteminfo_descText.text = LangInstance.GetData(item.name + "_Desc");

        string abStr = "";

        if (item.stat != 0)
            abStr += $"· {LangInstance.GetData("iteminfo_stat_" + item.statType.ToString())} + {item.stat}%\n";

        if (item.elementType != 0 && item.elementType != ElementType.extra)
        {
            string element = LangInstance.GetData("iteminfo_element_" + item.elementType.ToString());

            if (item.elementStatType != 0)
                abStr += $"· {element} {LangInstance.GetData("iteminfo_elementStatType_" + item.elementStatType.ToString())} + {item.value}%\n";

            if (item.resist != 0)
                abStr += $"· {element} {LangInstance.GetData("iteminfo_resist")} + {item.resist}%\n";
        }

        iteminfo_abilitiesText.text = abStr;

        iteminfo_equipButton.gameObject.SetActive(false);
        iteminfo_unequipButton.gameObject.SetActive(false);
        iteminfo_enhanceButton.gameObject.SetActive(false);
        if (iteminfo_sellButton != null)
            iteminfo_sellButton.gameObject.SetActive(false);
        if (iteminfo_sellPriceGroup != null)
            iteminfo_sellPriceGroup.SetActive(false);
    }

    /// <summary>
    /// close iteminfo popup, itempopup close button event
    /// </summary>
    public void CloseItemInfoPopupUI()
    {
        GuideCanvas_InventoryPopup.SetActive(false);
        ItemInfo.SetActive(false);

        ClearClassImage(iteminfo_classImage);
        iteminfo_itemImage.sprite = null;
        iteminfo_typeImage.sprite = null;

        iteminfo_name_forceText.text = "";
        iteminfo_descText.text = "";
        iteminfo_abilitiesText.text = "";

        InventoryManager.Instance.CloseGuidePopup();
    }

    /// <summary>
    /// open enhance popup in GuideCanvas_inventoryPopup
    /// </summary>
    /// <param name="slotData">inventory slot data</param>
    /// <param name="_howmuch">gold value</param>
    public void OpenEnhancePopUpUI(Slot slotData, int _howmuch)
    {
        ItemInfo.SetActive(false);
        Enhance.SetActive(true);

        if(slotData._myitem.force < 12)
            iteminfo_enhanceButton.gameObject.SetActive(true);
        else iteminfo_enhanceButton.gameObject.SetActive(false);

        ApplyClassImage(enhance_classImage, slotData._item.rankType);
        enhance_itemImage.sprite = slotData.itemimage.sprite;
        enhance_typeImage.sprite = slotData.itemtypeimage.sprite;

        if (APIData.Instance.UpgradeKeyValue.ContainsKey(slotData._myitem.force))
        {
            if (!slotData._myitem.isForce)
            { if (!enhance_matImage.gameObject.activeSelf) enhance_matImage.gameObject.SetActive(true); }
            else enhance_matImage.gameObject.SetActive(false);

            enhance_matImage.sprite = slotData.itemimage.sprite;
            enhance_matTypeImage.sprite = slotData.itemtypeimage.sprite;

            enhance_matCount.text = $"{InventoryManager.Instance.materials.Count} / {APIData.Instance.UpgradeKeyValue.GetValueOrDefault(slotData._myitem.force)}";
            enhance_matCount.color = InventoryManager.Instance.materials.Count >= APIData.Instance.UpgradeKeyValue.GetValueOrDefault(slotData._myitem.force) ? Color.green : Color.red;
        }
        else
            if (enhance_matImage.gameObject.activeSelf) enhance_matImage.gameObject.SetActive(false);

        enhance_currentForceText.text = $"+{slotData._myitem.force}";
        enhance_nextForceText.text = $"+{slotData._myitem.force + 1}";

        enhance_goldCount.text = $"{UserDataBase.Instance.userData.gamegold} / {_howmuch}";
        enhance_goldCount.color = UserDataBase.Instance.userData.gamegold >= _howmuch ? Color.green : Color.red;

        RefreshEnhanceButtonState(slotData._myitem.force, slotData._myitem.isForce);
    }

    /// <summary>
    /// open enhance popup in GuideCanvas_inventoryPopup
    /// </summary>
    /// <param name="slotData">inventory equipmentslot data</param>
    /// <param name="_howmuch">gold value</param>
    public void OpenEnhancePopUpUI(EquipmentSlot slotData, int _howmuch)
    {
        ItemInfo.SetActive(false);
        Enhance.SetActive(true);

        if (slotData.equipmentItemData.force < 12)
            iteminfo_enhanceButton.gameObject.SetActive(true);
        else iteminfo_enhanceButton.gameObject.SetActive(false);

        ApplyClassImage(enhance_classImage, slotData._item.rankType);
        enhance_itemImage.sprite = slotData.itemimage.sprite;
        enhance_typeImage.sprite = slotData.itemtypeimage.sprite;

        if (APIData.Instance.UpgradeKeyValue.ContainsKey(slotData.equipmentItemData.force))
        {
            if (!slotData.equipmentItemData.isForce)
            { if (!enhance_matImage.gameObject.activeSelf) enhance_matImage.gameObject.SetActive(true); }
            else enhance_matImage.gameObject.SetActive(false);

            enhance_matImage.sprite = slotData.itemimage.sprite;
            enhance_matTypeImage.sprite = slotData.itemtypeimage.sprite;

            enhance_matCount.text = $"{InventoryManager.Instance.materials.Count} / {APIData.Instance.UpgradeKeyValue.GetValueOrDefault(slotData.equipmentItemData.force)}";
            enhance_matCount.color = InventoryManager.Instance.materials.Count >= APIData.Instance.UpgradeKeyValue.GetValueOrDefault(slotData.equipmentItemData.force) ? Color.green : Color.red;
        }
        else
            if (enhance_matImage.gameObject.activeSelf) enhance_matImage.gameObject.SetActive(false);

        enhance_currentForceText.text = $"+{slotData.equipmentItemData.force}";
        enhance_nextForceText.text = $"+{slotData.equipmentItemData.force + 1}";

        enhance_goldCount.text = $"{UserDataBase.Instance.userData.gamegold} / {_howmuch}";
        enhance_goldCount.color = UserDataBase.Instance.userData.gamegold >= _howmuch ? Color.green : Color.red;

        RefreshEnhanceButtonState(slotData.equipmentItemData.force, slotData.equipmentItemData.isForce);
    }

    /// <summary>
    /// close enhance popup, enhance cancle button event
    /// </summary>
    public void CloseEnhancePopupUI()
    {
        GuideCanvas_InventoryPopup.SetActive(false);
        Enhance.SetActive(false);

        ClearClassImage(enhance_classImage);
        enhance_itemImage.sprite = null;
        enhance_typeImage.sprite = null;

        enhance_matImage.sprite = null;
        enhance_matTypeImage.sprite = null;

        enhance_currentForceText.text = "";
        enhance_nextForceText.text = "";
        enhance_matCount.text = "";
        enhance_goldCount.text = "";

        InventoryManager.Instance.CloseGuidePopup();
        InventoryManager.Instance.FreshSlot();

        enhance_enhanceButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// enhance success, update ui text
    /// </summary>
    /// <param name="_force"></param>
    /// <param name="_howmuch"></param>
    public void UpdateEnhancePopupUIText(MyItemData myItemData, int _howmuch)
    {
        enhance_currentForceText.text = $"+{myItemData.force}";
        enhance_nextForceText.text = $"+{myItemData.force + 1}";

        if (APIData.Instance.UpgradeKeyValue.ContainsKey(myItemData.force))
        {
            if (!myItemData.isForce)
            { if (!enhance_matImage.gameObject.activeSelf) enhance_matImage.gameObject.SetActive(true); }
            else enhance_matImage.gameObject.SetActive(false);
                    
            enhance_matImage.sprite = enhance_itemImage.sprite;
            enhance_matTypeImage.sprite = enhance_typeImage.sprite;

            enhance_matCount.text = $"{InventoryManager.Instance.materials.Count} / {APIData.Instance.UpgradeKeyValue.GetValueOrDefault(myItemData.force)}";
            enhance_matCount.color = InventoryManager.Instance.materials.Count >= APIData.Instance.UpgradeKeyValue.GetValueOrDefault(myItemData.force) ? Color.green : Color.red;
        }
        else
        if (enhance_matImage.gameObject.activeSelf) enhance_matImage.gameObject.SetActive(false);

        enhance_goldCount.text = $"{UserDataBase.Instance.userData.gamegold} / {_howmuch}";
        enhance_goldCount.color = UserDataBase.Instance.userData.gamegold >= _howmuch ? Color.green : Color.red;

        RefreshEnhanceButtonState(myItemData.force, myItemData.isForce);
    }

    /// <summary>
    /// open skillinfo popup, in GuideCanvas_inventoryPopup
    /// </summary>
    /// <param name="_skillData">inventory skillslot skilldata</param>
    /// <param name="_mySkillData">inventory skillslot myskilldata</param>
    /// <summary>17차: 스킬 정보창의 영어 고정 라벨 'coast' → '코스트' (언어별)</summary>
    void FixCostLabel()
    {
        if (SkillInfo == null)
            return;
        foreach (TMPro.TextMeshProUGUI t in SkillInfo.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
        {
            if (t == null || t == skillinfo_skillCoast)
                continue;
            string s = t.text != null ? t.text.Trim().ToLowerInvariant() : "";
            if (s == "coast" || s == "cost" || s == "코스트" || s == "消耗")
            {
                t.text = CardText.Loc("skill_cost_label", "코스트");
                TextFit.Apply(t, true);
            }
        }
    }

    public void OpenSkillInfoPopUpUI(SkillData _skillData, MySkillData _mySkillData)
    {
        FixCostLabel();
        skillinfo_buyButton.gameObject.SetActive(false);
        skillinfo_equipButton.gameObject.SetActive(true);

        GuideCanvas_InventoryPopup.SetActive(true);
        SkillInfo.SetActive(true);

        skillinfo_skillImage.sprite = APIData.Instance.AddressableSprite[$"{_skillData.effectId}_{_skillData.name}"];
        
        skillinfo_skillName.text = _skillData.LangName;
        skillinfo_skillCoast.text = $"{_skillData.coast}";
        skillinfo_descText.text = LangInstance.GetData($"{_skillData.name}_Desc");

        if (_mySkillData.slotNum == -1)
        {
            skillinfo_equipButton.gameObject.SetActive(true);
            skillinfo_unequipButton.gameObject.SetActive(false);
        }
        else
        {
            skillinfo_equipButton.gameObject.SetActive(false);
            skillinfo_unequipButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// close skillinfo popup, skillpopup cancle button event
    /// </summary>
    public void CloseSkillInfoPopupUI()
    {
        if (IAPManager.Instance != null)
            IAPManager.Instance.CancelSkillPurchase(); // 17차: 닫으면 대기 중인 스킬 구매 취소 (구매 버튼은 먼저 꺼내 감)
        GuideCanvas_InventoryPopup.SetActive(false);
        SkillInfo.SetActive(false);

        skillinfo_skillImage.sprite = null;
        skillinfo_skillName.text = "";
        skillinfo_skillCoast.text = "";
        skillinfo_descText.text = "";

        SkillInfoClosed?.Invoke();
    }

    public void OpenSkillInfoPopUpUI_withShop(SkillData _skillData, int price = 0)
    {
        skillinfo_buyButton.gameObject.SetActive(true);
        // 17차: 구매 버튼에 가격 표시 (기존: '구매'만 있어 얼마인지 알 수 없었음)
        TMPro.TextMeshProUGUI buyLabel = skillinfo_buyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (buyLabel != null && price > 0)
        {
            buyLabel.text = CardText.Fmt("shop_skill_buy_price", "구매  {0}", string.Format("{0:#,##0}", price));
            TextFit.Apply(buyLabel, false);
        }
        FixCostLabel();
        skillinfo_equipButton.gameObject.SetActive(false);
        // 상점 스킬은 장착 대상이 아니므로 해제 버튼도 숨깁니다. (직전에 연 장착 스킬 정보창의 버튼이 남아 다른 스킬이 해제되던 문제)
        skillinfo_unequipButton.gameObject.SetActive(false);

        GuideCanvas_InventoryPopup.SetActive(true);
        SkillInfo.SetActive(true);

        skillinfo_skillImage.sprite = APIData.Instance.AddressableSprite[$"{_skillData.effectId}_{_skillData.name}"];

        skillinfo_skillName.text = _skillData.LangName;
        skillinfo_skillCoast.text = $"{_skillData.coast}";
        skillinfo_descText.text = LangInstance.GetData($"{_skillData.name}_Desc");
    }

    void RefreshEnhanceButtonState(int force, bool isForce)
    {
        bool goldOk = InventoryManager.Instance.GoldCheck(force);
        bool matOk = true;
        if (!isForce && APIData.Instance.UpgradeKeyValue.ContainsKey(force))
            matOk = InventoryManager.Instance.materials.Count >= APIData.Instance.UpgradeKeyValue.GetValueOrDefault(force);

        enhance_enhanceButton.interactable = goldOk && matOk;
    }

    private void ApplyClassImage(Image image, ItemClass rankType)
    {
        if (image == null)
            return;

        int rankIndex = (int)rankType;
        Sprite classSprite = null;
        if (classSprites != null && rankIndex >= 0 && rankIndex < classSprites.Length)
            classSprite = classSprites[rankIndex];

        if (classSprite == null)
            classSprite = EquipmentGradeVisualUtility.GetSlotFrameSprite(rankIndex);

        image.sprite = classSprite;
        image.color = Color.white;
        image.enabled = classSprite != null;
    }

    private void ClearClassImage(Image image)
    {
        if (image == null)
            return;

        image.sprite = null;
        image.enabled = false;
    }
}
