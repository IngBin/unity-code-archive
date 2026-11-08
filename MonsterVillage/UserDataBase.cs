using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserDataBase : MonoBehaviour
{
    public static UserDataBase Instance { get; private set; }
    [System.Serializable]
    public struct UserData
    {
        public string nickname, energytime, accessToken;
        public int energy, gamegold, cash, currentStage, upgradeTicketCount;
        public SkillPiece skillpiece;
        public ElementCoin elementCoin;
        public List<GachaUseCountData> gachaUseList;

        public List<MyItemData> Invendata;
        public MyEquipmentItemData[] equipData;

        public Dictionary<int, MySkillData> mySkilldata;

        public Dictionary<string, string> myAbility;

        public List<ElementalList> skillFragmentList;
        public List<ElementalList> elementalCoinList;
        public List<DailyAdRewardData> dailyAdRewardList;

        [SerializeField]
        public List<StageClearData> userStageList;

        // 캐릭터 (로그인 응답에 포함, CharacterApi가 조회·구매·장착 성공 시 갱신)
        public ShopCharacterData[] characterList;
        public int[] ownedCharacterIds;
        public int equippedCharacterId;

        public int TotalEnergy
        {
            get
            {
                return energy + UserDataBase.Instance.PlusEnergy;
            }
        }
    }

    public UserData userData;
    public PlayerStatsInfo PlayerStats;
    PlayerStatsInfo originPlayerStats;

    public int Min; // 최소 에너지
    public int Max; // 최대 에너지
    public int EnergyResponTime; // 에너지 충전 시간
    public int PlusEnergy; // 마지막 접속시간체크해서 몇개의 에너지갖고있는지

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Min = 30;
        Max = 999;
        EnergyResponTime = 3;
    }

    public void SetPlayerStats(PlayerStatsInfo _info)
    {
        PlayerStats = _info;
        originPlayerStats = _info;
    }
    public void ResetPlayerStats()
    {
        PlayerStats = originPlayerStats;
    }
    public void InitElementalPieceNCoin()
    {
        foreach (var _data in userData.skillFragmentList)
        {
            switch (_data.elementalType)
            {
                case "COMMON":
                    break;
                case "FIRE":
                    userData.skillpiece.firepiece = _data.totalQty;
                    break;
                case "WATER":
                    userData.skillpiece.waterpiece = _data.totalQty;
                    break;
                case "ELECTRIC":
                    userData.skillpiece.electriciece = _data.totalQty;
                    break;
                case "LAND":
                    userData.skillpiece.randpiece = _data.totalQty;
                    break;
            }
        }

        foreach (var _data in userData.elementalCoinList)
        {
            switch (_data.elementalType)
            {
                case "COMMON":
                    break;
                case "FIRE":
                    userData.elementCoin.firecoin = _data.totalQty;
                    break;
                case "WATER":
                    userData.elementCoin.watercoin = _data.totalQty;
                    break;
                case "ELECTRIC":
                    userData.elementCoin.electriccoin = _data.totalQty;
                    break;
                case "LAND":
                    userData.elementCoin.randcoin = _data.totalQty;
                    break;
            }
        }
    }
    public void PlusZero()
    {
        PlusEnergy = 0;
    }
    public void GetSlot()
    {
        int equipnum = 0;
        foreach (KeyValuePair<int, MyEquipmentItemData> equipslot in APIData.Instance.EquipmentKeyValue)
        {
            if (userData.equipData[equipnum] != null)
            {
                userData.equipData[equipnum].slot_id = equipslot.Value.id;
                userData.equipData[equipnum].id = equipslot.Value.equipmentId;
                userData.equipData[equipnum].inventoryID = equipslot.Value.inventoryID;
                userData.equipData[equipnum].force = equipslot.Value.force;
                userData.equipData[equipnum].isForce = equipslot.Value.isForce;
                userData.equipData[equipnum].equipmentId = 0;
            }
            else
            {
                userData.equipData[equipnum] = new MyEquipmentItemData();
            }
            equipnum++;
        }
    }
    public void SetDefaultElemental()
    {
        int enumMemberCount = Enum.GetValues(typeof(ElementType)).Length;

        for (int i = 0; i < enumMemberCount; i++)
        {
            string type = ((ElementType)i).ToString().ToUpper();
            if (!userData.skillFragmentList.Select(a => a.elementalType).Contains(type))
            {
                userData.skillFragmentList.Add(new ElementalList(0, type));
            }
            if (!userData.elementalCoinList.Select(a => a.elementalType).Contains(type))
            {
                userData.elementalCoinList.Add(new ElementalList(0, type));
            }
        }
    }
}
#region userDatas
[System.Serializable]
public class MyItemData // 유저가 보유한 아이템 데이터, 수시로 변경되는 데이터 (서버 유저데이터에 세이브/로드)
{
    // 초기값 id = 0, force = 0, inventoryID , isForce = false
    public int id;      // 딕셔너리에 있는 데이터와 id 값을 비교해서 같은 아이디 값을 가진 데이터가 있다면 가져오도록 구성해야됨
                        //public StaticItemData itemdata;
    public int force;   // 강화 수치는 플레이어가 직접 들고 있어서 수치에 따라 증가폭 값 계산하도록 구성해야됨

    public int inventoryID; // 현명이형 쪽에서 부여되는 관계형디비 아이디
    public bool isForce;

    public MyItemData SetMyItemData(int _id, int _force, int _inventoryID, bool _isForce = false)
    {
        id = _id;
        force = _force;
        inventoryID = _inventoryID;
        isForce = _isForce;

        return this;
    }
}
[System.Serializable]
public class MyEquipmentItemData : MyItemData // 장착하고 있는 아이템 정보
{
    public int slot_id;
    public int equipmentId;
}
[System.Serializable]
public struct MySkillData // 유저의 스킬 데이터
{
    public int id;
    public int skillpiece;
    public int slotNum;

    public MySkillData(int _id, int _skillpiece, int _equip_slot_num = -1)
    {
        id = _id;
        skillpiece = _skillpiece;
        slotNum = _equip_slot_num;
    }

    public void SetPiece(int piece)
    {
        skillpiece = piece;
    }

    // 스킬 6개는 스킬조각을 max로 세팅해서 보내줄 수 있냐 (특정 스킬 ID 넘겨주고) - 해결, 현명이형한테 전달
    // PUT 함수는 두개(ID+SKILLPIECE), (ID+EQUIP_SLOT_NUM) ( EQUIP_SLOT_NUM은 디폴트 -1 )
    // GET 함수는 한개
    // 가챠로 뽑으면 해당 아이디에 해당되는 스킬만 피스 한개 늘려주는 함수

    // 스킬 총 개수를 디폴트로 뿌려주는거 or put을 할 때 아이디랑 스킬개수를 보냄 -> 해당 아이디가 없으면 디폴트로 만들어서 저장
}
[System.Serializable]
public struct PlayerStatsInfo // 유저의 스탯 정보
{
    public int projectileCount;
    public float atk, equipment_atk, characteristic_atk;
    public float def, equipment_def, characteristic_def;
    public float speed, equipment_speed, characteristic_speed;
    public float hp, equipment_hp, characteristic_hp;
    public float curere, equipment_curere, characteristic_curere;
    public float mana, equipment_mana, characteristic_mana;
    public float manare, equipment_manare, characteristic_manare;
    public float damage_fire, equipment_damage_fire, characteristic_damage_fire;
    public float damage_water, equipment_damage_water, characteristic_damage_water;
    public float damage_electric, equipment_damage_electric, characteristic_damage_electric;
    public float damage_land, equipment_damage_land, characteristic_damage_land;
    public float resist_fire, equipment_resist_fire, characteristic_resist_fire;
    public float resist_water, equipment_resist_water, characteristic_resist_water;
    public float resist_electric, equipment_resist_electric, characteristic_resist_electric;
    public float resist_land, equipment_resist_land, characteristic_resist_land;
    public float cool, equipment_cool, characteristic_cool;
    public float characteristic_cool_fire, characteristic_cool_water, characteristic_cool_land, characteristic_cool_electric;
    public float size, equipment_size, characteristic_size;
    // 특정 속성 룬의 스킬 범위(%). equipment_size는 모든 속성 룬의 값입니다.
    public float equipment_size_fire, equipment_size_water, equipment_size_electric, equipment_size_land;
    public float final_size; // 12강 룬의 스킬 범위(%)
    // 19차: 속성별 스킬 범위 특성 (%)
    public float characteristic_size_fire, characteristic_size_water, characteristic_size_electric, characteristic_size_land;
    /// <summary>시트 데미지 공식의 (1 + 장비 증가율). 룬의 스킬 데미지 값을 속성별로 적용합니다.</summary>
    public float EquipmentDamageRate(ElementType e)
    {
        switch (e)
        {
            case ElementType.fire: return 1 + (equipment_damage_fire * 0.01f);
            case ElementType.water: return 1 + (equipment_damage_water * 0.01f);
            case ElementType.electric: return 1 + (equipment_damage_electric * 0.01f);
            case ElementType.land: return 1 + (equipment_damage_land * 0.01f);
            default: return 1f;
        }
    }
    /// <summary>룬의 스킬 지속시간 배율. 모든 속성 룬은 모든 스킬에, 특정 속성 룬은 해당 속성 스킬에만 적용됩니다.</summary>
    public float EquipmentTimeRate(ElementType e)
    {
        float elementPercent = 0f;
        switch (e)
        {
            case ElementType.fire: elementPercent = equipment_time_fire; break;
            case ElementType.water: elementPercent = equipment_time_water; break;
            case ElementType.electric: elementPercent = equipment_time_electric; break;
            case ElementType.land: elementPercent = equipment_time_land; break;
        }
        return (1 + ((equipment_time + elementPercent) * 0.01f)) * (1 + (final_time * 0.01f));
    }
    /// <summary>룬의 스킬 범위 배율. 모든 속성 룬은 모든 스킬에, 특정 속성 룬은 해당 속성 스킬에만 적용됩니다.</summary>
    public float EquipmentSizeRate(ElementType e)
    {
        float elementPercent = 0f;
        switch (e)
        {
            case ElementType.fire: elementPercent = equipment_size_fire; break;
            case ElementType.water: elementPercent = equipment_size_water; break;
            case ElementType.electric: elementPercent = equipment_size_electric; break;
            case ElementType.land: elementPercent = equipment_size_land; break;
        }
        return (1 + ((equipment_size + elementPercent) * 0.01f)) * (1 + (final_size * 0.01f));
    }
    public float ElementSizePercent(ElementType e)
    {
        switch (e)
        {
            case ElementType.fire: return characteristic_size_fire;
            case ElementType.water: return characteristic_size_water;
            case ElementType.electric: return characteristic_size_electric;
            case ElementType.land: return characteristic_size_land;
            default: return 0f;
        }
    }
    public float time, equipment_time, characteristic_time;
    // 특정 속성 룬의 스킬 지속시간(%). equipment_time은 모든 속성 룬의 값입니다.
    public float equipment_time_fire, equipment_time_water, equipment_time_electric, equipment_time_land;
    public float final_time; // 12강 룬의 스킬 지속시간(%)
    public float coast, equipment_coast, characteristic_coast;

    public float TotalAtk
    {
        get
        {
            return atk + (atk * (equipment_atk * 0.01f)) + (atk * (characteristic_atk * 0.01f));
        }
    }
    public float TotalDef
    {
        get
        {
            return def + (def * (equipment_def * 0.01f)) + (def * (characteristic_def * 0.01f));
        }
    }
    public float TotalSpeed
    {
        get
        {
            // 기존: equipment_def / characteristic_def 로 계산되어 방어력 장비가 이동속도를 올리고, 이동속도 장비는 효과가 없었음
            return speed + (speed * (equipment_speed * 0.01f)) + (speed * (characteristic_speed * 0.01f));
        }
    }
    public float TotalHp
    {
        get
        {
            return hp + (hp * (equipment_hp * 0.01f)) + (hp * (characteristic_hp * 0.01f));
        }
    }
    public float TotalCurere
    {
        get
        {
            return curere + equipment_curere + characteristic_curere;
        }
    }
    public float TotalMana
    {
        get
        {
            return mana + equipment_mana + characteristic_mana;
        }
    }
    public float TotalManare
    {
        get
        {
            return manare + equipment_manare + characteristic_manare;
        }
    }
    public float TotalDamageFire
    {
        get
        {
            return damage_fire + (damage_fire * (characteristic_damage_fire * 0.01f));
        }
    }
    public float TotalDamageWater
    {
        get
        {
            return damage_water + (damage_water * (characteristic_damage_water * 0.01f));
        }
    }
    public float TotalDamageElectric
    {
        get
        {
            return damage_electric + (damage_electric * (characteristic_damage_electric * 0.01f));
        }
    }
    public float TotalDamageLand
    {
        get
        {
            return damage_land + (damage_land * (characteristic_damage_land * 0.01f));
        }
    }
    public float TotalResistFire
    {
        get
        {
            return resist_fire + (resist_fire * (equipment_resist_fire * 0.01f)) + (resist_fire * (characteristic_resist_fire * 0.01f));
        }
    }
    public float TotalResistWater
    {
        get
        {
            return resist_water + (resist_water * (equipment_resist_water * 0.01f)) + (resist_water * (characteristic_resist_water * 0.01f));
        }
    }
    public float TotalResistElectric
    {
        get
        {
            return resist_electric + (resist_electric * (equipment_resist_electric * 0.01f)) + (resist_electric * (characteristic_resist_electric * 0.01f));
        }
    }
    public float TotalResistLand
    {
        get
        {
            return resist_land + (resist_land * (equipment_resist_land * 0.01f)) + (resist_land * (characteristic_resist_land * 0.01f));
        }
    }
    /// <summary>
    /// 쿨타임 속성별로
    /// </summary>
    public float TotalCool
    {
        get
        {
            return (1 - (equipment_cool * 0.01f)) * (1 - (characteristic_cool * 0.01f));
        }
    }
    public float TotalFireCool
    {
        get
        {
            return (1 - (characteristic_cool_fire * 0.01f)) * (1 - (equipment_cool * 0.01f));
        }
    }
    public float TotalLandCool
    {
        get
        {
            return (1 - (characteristic_cool_land * 0.01f)) * (1 - (equipment_cool * 0.01f));
        }
    }
    public float TotalElectricCool
    {
        get
        {
            return (1 - (characteristic_cool_electric * 0.01f)) * (1 - (equipment_cool * 0.01f));
        }
    }
    public float TotalWaterCool
    {
        get
        {
            return (1 - (characteristic_cool_water * 0.01f)) * (1 - (equipment_cool * 0.01f));
        }
    }
    public float TotalSize
    {
        get
        {
            return size + characteristic_size;
        }
    }
    public float TotalTime
    {
        get
        {
            return time + characteristic_time;
        }
    }
    public float TotalCoast
    {
        get
        {
            return equipment_coast + characteristic_coast;
        }
    }
    public int TotalProjectileCount
    {
        get
        {
            return projectileCount;
        }
    }
}
[System.Serializable]
public struct TraitData // 특성 
{
    public string trait;
}
[System.Serializable]
public class TraitDataSave // 특성 저장
{
    public string trait;
    public int value;
}
[System.Serializable]
public class InventorySellDataSave // 장비 판매 요청
{
    public List<int> inventoryIds;
    public int gold;
}
[System.Serializable]
public class GachaUseCountData
{
    public int gachaId;
    public int count;
}
[System.Serializable]
public class GachaRewardDataSave
{
    public int rewardType;
    public int rewardId;
    public int count;
}
[System.Serializable]
public class GachaRewardListDataSave
{
    public int gachaId;
    public int gachaRewardType;
    public List<GachaRewardDataSave> rewards;
}
[System.Serializable]
public class ElementalList // 속성 관련 (조각, 코인) 리스트
{
    public int totalQty;
    public string elementalType;
    public ElementalList(int qty, string type)
    {
        totalQty = qty;
        elementalType = type;
    }
}
[System.Serializable]
public struct SkillPiece // 스킬 조각 갯수
{
    public int firepiece;
    public int waterpiece;
    public int electriciece;
    public int randpiece;
}
[System.Serializable]
public struct ElementCoin // 코인 갯수
{
    public int firecoin;
    public int watercoin;
    public int electriccoin;
    public int randcoin;
}
[System.Serializable]
public class StageClearData
{
    public int stage;
    public int score;
    public string time;
}
#endregion
