using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    /// <summary>
    /// Ai와 AiGroup에서 스킬이 선택되고 대상이 선택된 후, 실질적인 효과를 지정하는 데이터
    /// </summary>
    public class AbilityData
    {
        public int abilityId;
        public AbilityType abilityType;
        public int value;
        public float ratio;
        public AbilityWorkType workType;
        public int duration;
        public string skillAnim;
        public string skillFx;
        public string hitFx;
        public string ptojectileFx;
        public string skillIcon;
        public string skillViewIcon;
        public string fxSound;

        public string LogText => Utils.AbilityLog(this);

        public void Initialize(AbilityDataEntity entity)
        {
            abilityId = entity.AbilityData_Id;
            abilityType = ConvertAbilityType(entity.AbilityType);
            value = entity.Value;
            ratio = entity.Ratio;
            workType = ConvertAbilityWorkType(entity.Type);
            duration = entity.Duration;
            skillAnim = entity.SkillAnim;
            skillFx = entity.SkillFx;
            hitFx = entity.HitFx;
            ptojectileFx = entity.PtojectileFx;
            skillIcon = entity.SkillIcon;
            skillViewIcon = entity.SkillViewIcon;
            fxSound = entity.FxSound;
        }

        public static List<AbilityData> abilityData_list = new List<AbilityData>();
        public static Dictionary<int, AbilityData> abilityData_dict = new Dictionary<int, AbilityData>();

        public static void Load(string path = "")
        {
            AbilityDataTable abilityDataTable = Resources.Load<AbilityDataTable>(path);

            if (abilityDataTable == null)
            {
                Debug.LogError("AbilityDataTable is not found");
                return;
            }

            foreach (var abilityEntity in abilityDataTable.items)
            {
                if (abilityData_dict.ContainsKey(abilityEntity.AbilityData_Id))
                    continue;

                AbilityData abilityData = new();

                abilityData.Initialize(abilityEntity);

                abilityData_list.Add(abilityData);
                abilityData_dict.Add(abilityData.abilityId, abilityData);
            }
        }

        public static AbilityData GetAbilityData(int abilityData_Id)
        {
            return abilityData_dict.TryGetValue(abilityData_Id, out AbilityData abilityData) ? abilityData : null;
        }

        AbilityType ConvertAbilityType(string abilityType)
        {
            return abilityType switch
            {
                "Damage" => AbilityType.Damage,
                "Heal" => AbilityType.Heal,
                "AtkUp" => AbilityType.AtkUp,
                "DefUp" => AbilityType.DefUp,
                "BrokenDef" => AbilityType.BrokenDef,
                "ChargeDamage" => AbilityType.ChargeDamage,
                "AddGoldPer" => AbilityType.AddGoldPer,
                "AddMana" => AbilityType.AddMana,
                "SubMana" => AbilityType.SubMana,
                "AddGold" => AbilityType.AddGold,
                "AddSatiety" => AbilityType.AddSatiety,
                "SubSatiety" => AbilityType.SubSatiety,
                _ => AbilityType.None,
            };
        }

        AbilityWorkType ConvertAbilityWorkType(string type)
        {
            return type switch
            {
                "Active" => AbilityWorkType.Active,
                "Passive" => AbilityWorkType.Passive,
                "OnHand" => AbilityWorkType.OnHand,
                _ => AbilityWorkType.None,
            };
        }
    }
}