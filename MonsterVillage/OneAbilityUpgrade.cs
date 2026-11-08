using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneAbilityUpgrade : MonoBehaviour
{
    public enum StatType
    {
        None,
        Speed,
        //Coast,
        Defend,
        Hp,
        ElementDamage,
        //ElementTime,
        ElementCool,
        ElementResist, // 범위
    }

    public enum StatElement
    {
        None,
        Fire,
        Water,
        Electric,
        Land
    }

    public StatElement element;
    public StatType type;

    public int level = 0;
    public int maxLv = 200;
    public Text LvText;

    public float _statvalue; // level time value (level * _statvalue = curStat)

    // 19차: 효과/최대 레벨은 TraitRules 에서 (레벨당 0.05% → 1% / 0.5%, 최대 30)
    public float curStat
    {
        get { return TraitRules.Value(type, level); }
    }

    private void Start()
    {
        maxLv = TraitRules.MaxLevel;
        level = TraitRules.Clamp(level);
        this.gameObject.AddComponent<Button>().onClick.AddListener(() => Click());
        LvText = this.transform.GetChild(1).GetComponent<Text>();

        if (level == maxLv)
            LvText.text = "Lv Max";
        else
            LvText.text = $"Lv {level}";
    }

    // [미사용] 예전 특성 칸('Abilitys')은 TraitPanel이 꺼 두므로 눌리지 않는다. 눌려도 새 강화 창이 먼저 열린다.
    // 아래 예전 팝업 경로는 사용자 결정(2026-11-08)으로 비활성 상태로 남겨 둔다.
    // 단, StatType/StatElement enum과 이 오브젝트의 아이콘은 TraitRules/TraitPanel/Ui2가 쓰므로 클래스는 지우면 안 된다.
    // 제거 조건: enum과 아이콘 참조를 TraitRules/TraitPanel로 옮긴 뒤.
    void Click()
    {
        if (TraitPanel.OpenSheet(gameObject.name))
            return;
        if (level >= maxLv)
            return;

        AbilityPageManager apm = LobbyManager.Instance.abilityManager;
        
        apm.GuidePopup.SetActive(true);

        apm.abilityObj = this.gameObject;
        apm.curAbill_img.sprite = this.gameObject.GetComponent<Image>().sprite;
        apm.abillType_img.sprite = this.transform.GetChild(0).GetComponent<Image>().sprite;

        if (element == StatElement.None)
        {
            apm.typeImage.sprite = apm.typeSprites[0];
            apm.goldtext.text = (apm.default_Gold + apm.plus_Gold * level).ToString();
        }
        else
        {
            apm.typeImage.sprite = apm.typeSprites[1];
            apm.goldtext.text = (apm.default_Coin + apm.plus_Coin * level).ToString();
        }

        apm.CurLevelText.text = typeCheck() + " " + LvText.text;
        //apm.NextLvText.text = $"Lv {level + 1}";

        apm.RefreshActivateButtonState();
    }

    public void abilityCheck()
    {
        // 19차: 모든 특성을 새 규칙으로 한 번에 반영 (예전 개별 계산은 아래에 남겨 두지만 사용하지 않음)
        TraitRules.ApplyAll();
        if (LobbyManager.Instance != null && LobbyManager.Instance.abilityManager != null && LobbyManager.Instance.abilityManager.ActivateBtn != null)
            LobbyManager.Instance.abilityManager.ActivateBtn.interactable = true;
        return;
#pragma warning disable CS0162
        switch (type)
        {
            case StatType.None:
                break;

            case StatType.Speed:
                UserDataBase.Instance.PlayerStats.characteristic_speed = curStat;
                break;
            //case StatType.Coast:
            //    UserDataBase.Instance.PlayerStats.characteristic_coast = curStat;
            //    break;
            case StatType.Defend:
                UserDataBase.Instance.PlayerStats.characteristic_def = curStat;
                break;
            case StatType.Hp:
                UserDataBase.Instance.PlayerStats.characteristic_hp = curStat;
                break;
            case StatType.ElementDamage:
                elementDamageCheck(curStat);
                break;
            //case StatType.ElementTime:
            //    UserDataBase.Instance.PlayerStats.characteristic_time = curStat;
            //    break;
            case StatType.ElementCool:
                elementCoolCheck(curStat);
                break;
            case StatType.ElementResist:
                elementResistCheck(curStat);
                break;
        }

        LobbyManager.Instance.abilityManager.ActivateBtn.interactable = true;
#pragma warning restore CS0162
    }

    void elementDamageCheck(float value)
    {
        switch (element)
        {
            case StatElement.None:
                break;
            case StatElement.Fire:
                UserDataBase.Instance.PlayerStats.characteristic_damage_fire = value;
                break;
            case StatElement.Water:
                UserDataBase.Instance.PlayerStats.characteristic_damage_water = value;
                break;
            case StatElement.Electric:
                UserDataBase.Instance.PlayerStats.characteristic_damage_electric = value;
                break;
            case StatElement.Land:
                UserDataBase.Instance.PlayerStats.characteristic_damage_land = value;
                break;
        }
    }

    /// <summary>
    /// 이름만 resist고 범위(size)체크
    /// </summary>
    /// <param name="value"></param>
    void elementResistCheck(float value) // 범위
    {
        switch (element)
        {
            case StatElement.Fire:
                UserDataBase.Instance.PlayerStats.characteristic_size = value;
                break;
            case StatElement.Water:
                UserDataBase.Instance.PlayerStats.characteristic_size = value;
                break;
            case StatElement.Electric:
                UserDataBase.Instance.PlayerStats.characteristic_size = value;
                break;
            case StatElement.Land:
                UserDataBase.Instance.PlayerStats.characteristic_size = value;
                break;
        }
    }

    void elementCoolCheck(float value)
    {
        switch (element)
        {
            case StatElement.Fire:
                UserDataBase.Instance.PlayerStats.characteristic_cool_fire = value;
                break;
            case StatElement.Water:
                UserDataBase.Instance.PlayerStats.characteristic_cool_water = value;
                break;
            case StatElement.Electric:
                UserDataBase.Instance.PlayerStats.characteristic_cool_electric = value;
                break;
            case StatElement.Land:
                UserDataBase.Instance.PlayerStats.characteristic_cool_land = value;
                break;
        }
    }

    public string typeCheck()
    {
        switch(type)
        {
            case StatType.None:
                break;

            case StatType.Speed:
            case StatType.Defend:
            case StatType.Hp:
                return LanguageSingleton.LangInstance.GetData("ability_" + type.ToString());

            case StatType.ElementDamage:
            case StatType.ElementCool:
            case StatType.ElementResist:
                return LanguageSingleton.LangInstance.GetData(element.ToString()) + " " + LanguageSingleton.LangInstance.GetData("ability_" + type.ToString());
        }

        return "";
    }
}
