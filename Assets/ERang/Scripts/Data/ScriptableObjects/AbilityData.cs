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
        public string nameDesc;
        public AbilityType abilityType;
        public int value;
        public int value2;
        public float ratio;
        public AbilityWorkType workType;
        public int duration;
        public string skillAnim;
        public string skillFx;
        public string hitFx;
        public string ptojectileFx;
        public string skillIcon;
        public string skillIconDesc;
        public string skillViewIcon;
        public string fxSound;
        public int summonGroupId;
        public int chainAbilityId;

        [Header("Display")]
        public Texture2D iconTexture;

        public void Initialize(AbilityDataEntity entity)
        {
            abilityId = entity.AbilityData_Id;
            nameDesc = entity.NameDesc;
            abilityType = ConvertAbilityType(entity.AbilityType);
            value = entity.Value;
            value2 = entity.Value2;
            ratio = entity.Ratio;
            workType = ConvertAbilityWorkType(entity.Type);
            duration = entity.Duration;
            skillAnim = entity.SkillAnim;
            skillFx = entity.SkillFx;
            hitFx = entity.HitFx;
            ptojectileFx = entity.PtojectileFx;
            skillIcon = entity.SkillIcon;
            skillIconDesc = entity.SkillIconDesc;
            skillViewIcon = entity.SkillViewIcon;
            fxSound = entity.FxSound;
            summonGroupId = entity.Summon_GroupId;
            chainAbilityId = entity.ChainAbilityId;

            // 아이콘 이미지 로드 및 iconTexture 에 할당
            if (!string.IsNullOrEmpty(skillIcon))
            {
                string texturePath = $"Textures/{skillIcon}";
                iconTexture = Resources.Load<Texture2D>(texturePath);

                if (iconTexture == null)
                    Debug.LogError("Card Texture not found: " + texturePath);
            }
        }

        public static List<AbilityData> abilityData_list = new();
        public static Dictionary<int, AbilityData> abilityData_dict = new();

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
            abilityType = abilityType.Replace(" ", "");

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
                "Summon_Hand" => AbilityType.SummonHand,
                "Summon_DrawDeck" => AbilityType.SummonDrawDeck,
                "Summon_GraveDeck" => AbilityType.SummonGraveDeck,
                "Weaken" => AbilityType.Weaken,
                "ArmorBreak" => AbilityType.ArmorBreak,
                "Doom" => AbilityType.Doom,
                "Burn" => AbilityType.Burn,
                "Poison" => AbilityType.Poison,
                "Swallow" => AbilityType.Swallow,
                "ReducedMana" => AbilityType.ReducedMana,
                "Summon_Hand_Sel" => AbilityType.SummonHandSelect,
                "Summon_DrawDeck_Sel" => AbilityType.SummonDrawDeckSelect,
                "Summon_GraveDeck_Sel" => AbilityType.SummonGraveDeckSelect,
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
                "OnStage" or "Onstage" => AbilityWorkType.OnStage,
                _ => AbilityWorkType.None,
            };
        }
    }
}