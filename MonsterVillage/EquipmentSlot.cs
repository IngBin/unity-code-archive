using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    public MyEquipmentItemData equipmentItemData;
    public StaticItemData _item;

    public bool isEmpty = true;
    public Image ClassImage; // 아이템 등급 이미지
    public Image itemimage; // 아이템 이미지 (속성)
    public Image itemtypeimage; // 아이템 이미지 (스탯타입)?
    public TextMeshProUGUI itemforcetext; // 강화 수치

    [HideInInspector]
    public Button button;
    private Sprite emptyClassSprite;

    private void Awake()
    {
        if (ClassImage != null)
            emptyClassSprite = ClassImage.sprite;
    }

    private void OnEnable()
    {
    }

    private void Start()
    {
        InventoryManager.Instance.LoonSetting(itemimage, itemtypeimage, (int)_item.elementType, _item.imageId);

        if (this.GetComponent<Button>() != null)
            button = this.GetComponent<Button>();
        else
            button = this.gameObject.AddComponent<Button>();

        if (equipmentItemData.id != 0) // 이걸 슬롯에서 관리하는게 아니라 인벤토리 매니저에서 관리하는건?
        {
            //this.gameObject.transform.GetChild(0).GetComponent<AddressableSprite>()._Key = item.image_id.ToString();
            if (equipmentItemData.force < 1)
            {
                itemforcetext.text = "";
            }
            else
            {
                itemforcetext.text = "+" + equipmentItemData.force.ToString();
            }
            isEmpty = false;
            button.interactable = true;
            EquipmentGradeVisualUtility.ApplySlotFrame(ClassImage, _item.rankType, emptyClassSprite);
            //itemtestText.text = item.element_type.ToString();
        }
        else
        {
            isEmpty = true;
            button.interactable = false;
            EquipmentGradeVisualUtility.RestoreEmptySlotFrame(ClassImage, emptyClassSprite);
            itemimage.gameObject.SetActive(false);
            itemtypeimage.gameObject.SetActive(false);
            itemforcetext.text = "";
        }
    }

    public void Init()
    {
        
    }

    public void ApplyClassFrame()
    {
        EquipmentGradeVisualUtility.ApplySlotFrame(ClassImage, _item.rankType, emptyClassSprite);
    }

    public void RestoreEmptyClassFrame()
    {
        EquipmentGradeVisualUtility.RestoreEmptySlotFrame(ClassImage, emptyClassSprite);
    }

    /// <summary>장착 장비를 표시합니다. 전달받은 값만 그리고 상태 판단은 InventoryManager가 합니다.</summary>
    public void ShowItem(MyEquipmentItemData data, StaticItemData staticItem)
    {
        equipmentItemData = data;
        _item = staticItem;

        itemforcetext.text = data.force > 0 ? "+" + data.force : "";
        InventoryManager.Instance.LoonSetting(itemimage, itemtypeimage, (int)_item.elementType, _item.imageId);
        ApplyClassFrame();
        itemimage.gameObject.SetActive(true);
        itemtypeimage.gameObject.SetActive(true);

        isEmpty = false;
        SetInteractable(true);
    }

    /// <summary>빈 슬롯으로 표시합니다.</summary>
    public void ShowEmpty()
    {
        equipmentItemData = new MyEquipmentItemData();
        _item = new StaticItemData();

        itemforcetext.text = "";
        itemimage.gameObject.SetActive(false);
        itemtypeimage.gameObject.SetActive(false);
        RestoreEmptyClassFrame();

        isEmpty = true;
        SetInteractable(false);
    }

    // InventoryManager.Start가 이 슬롯의 Start보다 먼저 실행될 수 있어 button이 아직 비어 있을 수 있습니다.
    void SetInteractable(bool interactable)
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.interactable = interactable;
    }

    public void set_equipmentdata_in_userstat_plus()
    {
        ApplyStats(1f);
    }

    public void set_equipmentdata_in_userstat_minus()
    {
        ApplyStats(-1f);
    }

    /// <summary>룬의 값을 정보창에 표시되는 스탯 종류 그대로 유저 스탯에 더하거나 뺍니다.</summary>
    void ApplyStats(float sign)
    {
        UserDataBase userData = UserDataBase.Instance;
        PlayerStatsInfo stats = userData.PlayerStats;
        int force = equipmentItemData.force;

        float stat = _item.stat == 0 ? 0 : sign * (_item.stat + _item.statAdd * force);
        float element = _item.elementStatType == ElementStatType.None ? 0 : sign * (_item.value + _item.valueAdd * force);
        float resist = _item.resist == 0 ? 0 : sign * (_item.resist + _item.resistAdd * force);

        switch (_item.statType)
        {
            case StatType.atk: stats.equipment_atk = Add(stats.equipment_atk, stat); break;
            case StatType.def: stats.equipment_def = Add(stats.equipment_def, stat); break;
            case StatType.spd: stats.equipment_speed = Add(stats.equipment_speed, stat); break;
            case StatType.hp: stats.equipment_hp = Add(stats.equipment_hp, stat); break;
            case StatType.curere: stats.equipment_curere = Add(stats.equipment_curere, stat); break;
            case StatType.manare: stats.equipment_manare = Add(stats.equipment_manare, stat); break;
            case StatType.cool: stats.equipment_cool = Add(stats.equipment_cool, stat); break;
        }

        float damage = _item.elementStatType == ElementStatType.atk ? element : 0;
        float time = _item.elementStatType == ElementStatType.con ? element : 0;
        float size = _item.elementStatType == ElementStatType.sca ? element : 0;

        // 모든 속성 룬의 지속시간/범위는 모든 스킬 공용 값에 더합니다.
        switch (_item.elementType)
        {
            case ElementType.all:
                stats.equipment_time = Add(stats.equipment_time, time);
                stats.equipment_size = Add(stats.equipment_size, size);
                break;
            case ElementType.fire:
                stats.equipment_time_fire = Add(stats.equipment_time_fire, time);
                stats.equipment_size_fire = Add(stats.equipment_size_fire, size);
                break;
            case ElementType.water:
                stats.equipment_time_water = Add(stats.equipment_time_water, time);
                stats.equipment_size_water = Add(stats.equipment_size_water, size);
                break;
            case ElementType.electric:
                stats.equipment_time_electric = Add(stats.equipment_time_electric, time);
                stats.equipment_size_electric = Add(stats.equipment_size_electric, size);
                break;
            case ElementType.land:
                stats.equipment_time_land = Add(stats.equipment_time_land, time);
                stats.equipment_size_land = Add(stats.equipment_size_land, size);
                break;
        }

        if (_item.elementType == ElementType.fire || _item.elementType == ElementType.all)
        {
            stats.equipment_damage_fire = Add(stats.equipment_damage_fire, damage);
            stats.equipment_resist_fire = Add(stats.equipment_resist_fire, resist);
        }
        if (_item.elementType == ElementType.water || _item.elementType == ElementType.all)
        {
            stats.equipment_damage_water = Add(stats.equipment_damage_water, damage);
            stats.equipment_resist_water = Add(stats.equipment_resist_water, resist);
        }
        if (_item.elementType == ElementType.electric || _item.elementType == ElementType.all)
        {
            stats.equipment_damage_electric = Add(stats.equipment_damage_electric, damage);
            stats.equipment_resist_electric = Add(stats.equipment_resist_electric, resist);
        }
        if (_item.elementType == ElementType.land || _item.elementType == ElementType.all)
        {
            stats.equipment_damage_land = Add(stats.equipment_damage_land, damage);
            stats.equipment_resist_land = Add(stats.equipment_resist_land, resist);
        }

        userData.PlayerStats = stats;
    }

    // 소수 계산 오차로 해제 후 음수가 남지 않게 합니다.
    static float Add(float current, float delta)
    {
        float result = current + delta;
        return result < 0.0001f ? 0 : result;
    }
}
