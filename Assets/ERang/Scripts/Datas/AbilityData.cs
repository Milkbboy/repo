using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    public class AbilityData : ScriptableObject
    {
        public int abilityData_Id;
        public string abilityType;
        public int value;
        public float ratio;
        public string type;
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
            abilityType = entity.AbilityType;
            value = entity.Value;
            ratio = entity.Ratio;
            type = entity.Type;
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
    }
}