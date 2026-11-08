using FirstGearGames.SmoothCameraShaker;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// [미사용] 예전 특성 강화 창(GuideCanvas/AbilityPopup)과 코인 표시. 새 특성 UI(TraitPanel/TraitRules)가
/// 강화·저장을 모두 처리하므로 OBtn/XBtn/GoldCheck/CoinCheck/RefreshActivateButtonState 와 코인 글자는 쓰이지 않는다.
/// (AbilityPopup과 예전 특성 칸 'Abilitys'는 꺼져 있고 TraitPanel.OpenSheet가 항상 먼저 처리함)
/// 사용자 결정(2026-11-08)으로 비활성 상태로 남겨 둔다. Start의 레벨 읽기는 TraitPanel이 기다리는 순서라 그대로 둔다.
/// 제거 조건: 예전 특성 창을 다시 쓰지 않기로 확정될 때. 이때 LobbyManager.abilityManager 참조와 씬의 AbilityPopup도 함께 정리.
/// </summary>
public class AbilityPageManager : MonoBehaviour
{
    public GameObject abilityObj;

    public GameObject[] abillityArray;

    public GameObject GuidePopup;

    public Image typeImage;
    public Sprite[] typeSprites;

    public TextMeshProUGUI CurLevelText;
    public TextMeshProUGUI NextLvText;
    public TextMeshProUGUI goldtext;

    public Image curAbill_img;
    public Image abillType_img;

    public bool isSend = false;

    public Button ActivateBtn;

    public int default_Gold;
    public int plus_Gold;

    public int default_Coin;
    public int plus_Coin;

    public Text fire_coinText, water_coinText, electric_coinText, land_coinText;

    void Start()
    {
        foreach(GameObject ab in abillityArray)
        {
            //KeyValuePair<string,string> pair in UserDataBase.Instance.userData.myAbility
            //Debug.Log("Key : " + pair.Key + ", Value : " + pair.Value);
            //Debug.Log(abillityArray.GetValue(i).ToString());
            string value = "";
            if(UserDataBase.Instance.userData.myAbility.TryGetValue(ab.name, out value))
            {
                ab.GetComponent<OneAbilityUpgrade>().level = int.Parse(value);
                ab.GetComponent<OneAbilityUpgrade>().abilityCheck();
            }
        }

        string[] row = GameDataManager.Instance.GetComponent<AbillityUpgradeData>().Expenselist;

        default_Gold = int.Parse(row[1].Split("\t")[1]);
        plus_Gold = int.Parse(row[1].Split("\t")[2]);

        default_Coin = int.Parse(row[2].Split("\t")[1]);
        plus_Coin = int.Parse(row[2].Split("\t")[2]);
    }

    public void OBtn()
    {
        TraitDataSave saveData = new TraitDataSave();
        OneAbilityUpgrade curabill = abilityObj.GetComponent<OneAbilityUpgrade>();

        if (curabill.element == OneAbilityUpgrade.StatElement.None) // none Element
        {
            if (GoldCheck())
            {
                GameAudioManager.Ensure().PlayTraitUpgradeSfx();

                UserDataBase.Instance.userData.gamegold -= int.Parse(goldtext.text);

                curabill.level++;
                curabill.LvText.text = curabill.typeCheck() + " Lv " + curabill.level.ToString();

                saveData.trait = abilityObj.name + '-' + curabill.level;
                saveData.value = UserDataBase.Instance.userData.gamegold;

                CurLevelText.text = curabill.LvText.text;
                //NextLvText.text = $"Lv {curabill.level + 1}";

                goldtext.text = (default_Gold + plus_Gold * curabill.level).ToString();

                ActivateBtn.interactable = false;
                StartCoroutine(APIData.Instance.UnityWebRequestgetTraitPost(saveData, curabill.abilityCheck));
                APIData.Instance.SetUserData(APIData.PostType.gold);

                abilityObj.GetComponent<OneAbilityUpgrade>().level = curabill.level;
                abilityObj.GetComponent<OneAbilityUpgrade>().LvText.text = "Lv " + curabill.level.ToString();
            }
            else
            {
                GameManager.gInstance.PopUpOpen("nogold");
                XBtn();

                curabill = null;
                Destroy(curabill);
                return;
            }
        }
        else
        {
            if(CoinCheck(curabill.element, curabill.level))
            {
                GameAudioManager.Ensure().PlayTraitUpgradeSfx();

                curabill.level++;
                curabill.LvText.text = curabill.typeCheck() + " Lv " + curabill.level.ToString();

                saveData.trait = abilityObj.name + '-' + curabill.level;
                switch (curabill.element)
                {
                    case OneAbilityUpgrade.StatElement.Fire:
                        saveData.value = UserDataBase.Instance.userData.elementCoin.firecoin - curabill.level;
                        UserDataBase.Instance.userData.elementCoin.firecoin -= curabill.level;
                        fire_coinText.text = UserDataBase.Instance.userData.elementCoin.firecoin.ToString();
                        break;

                    case OneAbilityUpgrade.StatElement.Water:
                        saveData.value = UserDataBase.Instance.userData.elementCoin.watercoin - curabill.level;
                        UserDataBase.Instance.userData.elementCoin.watercoin -= curabill.level;
                        water_coinText.text = UserDataBase.Instance.userData.elementCoin.watercoin.ToString();
                        break;

                    case OneAbilityUpgrade.StatElement.Electric:
                        saveData.value = UserDataBase.Instance.userData.elementCoin.electriccoin - curabill.level;
                        UserDataBase.Instance.userData.elementCoin.electriccoin -= curabill.level;
                        electric_coinText.text = UserDataBase.Instance.userData.elementCoin.electriccoin.ToString();
                        break;

                    case OneAbilityUpgrade.StatElement.Land:
                        saveData.value = UserDataBase.Instance.userData.elementCoin.randcoin - curabill.level;
                        UserDataBase.Instance.userData.elementCoin.randcoin -= curabill.level;
                        land_coinText.text = UserDataBase.Instance.userData.elementCoin.randcoin.ToString();
                        break;
                }

                CurLevelText.text = curabill.LvText.text;
                //NextLvText.text = $"Lv {curabill.level + 1}";
                goldtext.text = (default_Coin + plus_Coin * curabill.level).ToString();

                Debug.Log("saveData.trait : " + saveData.trait + ", saveData.value : " + saveData.value);

                ActivateBtn.interactable = false;
                StartCoroutine(APIData.Instance.UnityWebRequestgetTraitPost(saveData, curabill.abilityCheck));

                abilityObj.GetComponent<OneAbilityUpgrade>().level = curabill.level;
                abilityObj.GetComponent<OneAbilityUpgrade>().LvText.text = "Lv " + curabill.level.ToString();
            }
            else
            {
                GameManager.gInstance.PopUpOpen("nocoin");
                XBtn();
                curabill = null;
                Destroy(curabill);
                return;
            }
        }
    }
            
    public void RefreshActivateButtonState()
    {
        if (abilityObj == null) return;

        OneAbilityUpgrade curabill = abilityObj.GetComponent<OneAbilityUpgrade>();
        bool canUpgrade = curabill.element == OneAbilityUpgrade.StatElement.None
            ? GoldCheck()
            : CoinCheck(curabill.element, curabill.level);

        ActivateBtn.interactable = canUpgrade;
    }

    public void XBtn()
    {
        abilityObj = null;
        GuidePopup.SetActive(false);
    }

    bool GoldCheck()
    {
        if (abilityObj == null)
            return false;

        if (UserDataBase.Instance.userData.gamegold <= int.Parse(goldtext.text))
            return false;
        else
            return true;
    }

    bool CoinCheck(OneAbilityUpgrade.StatElement stat, int _coin)
    {
        switch (stat)
        {
            case OneAbilityUpgrade.StatElement.Fire:
                if (UserDataBase.Instance.userData.elementCoin.firecoin > _coin)
                    return true;
                break;

            case OneAbilityUpgrade.StatElement.Water:
                if (UserDataBase.Instance.userData.elementCoin.watercoin > _coin)
                    return true;
                break;

            case OneAbilityUpgrade.StatElement.Electric:
                if (UserDataBase.Instance.userData.elementCoin.electriccoin > _coin)
                    return true;
                break;

            case OneAbilityUpgrade.StatElement.Land:
                if (UserDataBase.Instance.userData.elementCoin.randcoin > _coin)
                    return true;
                break;
        }

        return false;
    }
}
