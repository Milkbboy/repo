using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    /// <summary>
    /// Ai와 AiGroup에서 스킬이 선택되고 대상이 선택된 후, 실질적인 효과를 지정하는 데이터
    /// </summary>
    public class AbilityData : ScriptableObject
    {
        public int abilityData_Id;
        public AbilityType abilityType;
        public int value;
        public float ratio;
        public AbilityWorkType type;
        public int duration;
        public string skillAnim;
        public string skillFx;
        public string hitFx;
        public string ptojectileFx;
        public string skillIcon;
        public string skillViewIcon;
        public string fxSound;

        public void Initialize(AbilityDataEntity entity)
        {
            abilityData_Id = entity.AbilityData_Id;
            abilityType = ConvertAbilityType(entity.AbilityType);
            value = entity.Value;
            ratio = entity.Ratio;
            type = ConvertAbilityWorkType(entity.Type);
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
                string assetPath = $"Assets/ERang/Resources/Abilities/{abilityEntity.AbilityData_Id}.asset";
                AbilityData abilityData = AssetDatabase.LoadAssetAtPath<AbilityData>(assetPath);

                if (abilityData == null)
                {
                    abilityData = CreateInstance<AbilityData>();
                    AssetDatabase.CreateAsset(abilityData, assetPath);
                }

                abilityData.Initialize(abilityEntity);

                abilityData_list.Add(abilityData);
                abilityData_dict.Add(abilityData.abilityData_Id, abilityData);
            }
        }

        public static AbilityData GetAbilityData(int abilityData_Id)
        {
            if (abilityData_dict.ContainsKey(abilityData_Id))
            {
                return abilityData_dict[abilityData_Id];
            }

            Debug.LogError($"AbilityData.GetAbilityData() - abilityData_Id {abilityData_Id} is not found");

            return null;
        }

        AbilityType ConvertAbilityType(string abilityType)
        {
            switch (abilityType)
            {
                case "Damage": return AbilityType.Damage;
                case "Heal": return AbilityType.Heal;
                case "AtkUp": return AbilityType.AtkUp;
                case "DefUp": return AbilityType.DefUp;
                case "BrokenDef": return AbilityType.BrokenDef;
                case "ChargeDamage": return AbilityType.ChargeDamage;
                case "AddGoldPer": return AbilityType.AddGoldPer;
                case "AddMana": return AbilityType.AddMana;
                case "SubMana": return AbilityType.SubMana;
                case "AddGold": return AbilityType.AddGold;
                default: return AbilityType.None;
            }
        }

        AbilityWorkType ConvertAbilityWorkType(string type)
        {
            switch (type)
            {
                case "Active": return AbilityWorkType.Active;
                case "Passive": return AbilityWorkType.Passive;
                case "OnHand": return AbilityWorkType.OnHand;
                default: return AbilityWorkType.None;
            }
        }
    }
}