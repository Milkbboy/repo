using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    /// <summary>
    /// 스킬의 타겟과 행동 타입을 지정하는 데이터 시트
    /// </summary>
    [System.Serializable]
    public class AiData : ScriptableObject
    {
        public int ai_Id; // Ai의 Id 값
        public AiDataType type; // 행동의 타입을 정의
        public AiDataTarget target; // 대상 혹은 복수 대상을 설정한다.
        public AiDataAttackType attackType; // 행동이 이루어지는 절차를 설정한다.
        public int atk_Cnt; // 공격 횟수
        public float atk_Interval; // 공격 횟수가 1이 아닐 경우 공격이 진행되는 텀을 지정
        public int value; // 해당 행동의 무게 값으로 Ai Group에서 참조된다.
        public int explosion_Shock; // Type이 Explosion일 경우에만 입력
        public List<int> ability_Ids = new List<int>(); // 실질적인 효과를 주는 Ability의 Id를 입력
        // 공격 범위를 설정한다. 
        // - 근거리의 경우 Type를 통해 가장 근접한 적으로 이동한 이후를 기준으로 1은 바로 앞의 적을 의미하고, 2는 2칸 앞의 적을 의미한다.
        //   (Ex 1만 입력된 경우 바로 앞의 적을 공격, 1과 2가 입력된 경우 자신의 앞과 그 뒤의 적까지 공격) 
        // - Ranged의 경우 자신의 위치를 기준으로 지정된 값 만큼의 거리를 의미한다. 
        //   (Ex 4의 경우 자신의 4칸 앞을 향해 공격한다는 것을 의미, 4와 5가 입력된 경우 자신의 앞 4번째 그리고 5번째 적까지 공격한다는 의미)
        public List<int> attackRanges = new List<int>();

        public void Initialize(AiDataEntity entity)
        {
            ai_Id = entity.Ai_Id;
            type = ConvertAiDataType(entity.Type);
            target = ConvertAiDataTarget(entity.Target);
            attackType = ConvertAiDataAtackType(entity.Atk_Type);
            atk_Cnt = entity.Atk_Cnt;
            atk_Interval = entity.Atk_Interval;
            value = entity.Value;
            explosion_Shock = entity.Explosion_Shock;
            ability_Ids.AddRange(Utils.ParseIntArray(entity.Ability_id).Where(x => x != 0));
            attackRanges.AddRange(Utils.ParseIntArray(entity.Atk_Range).Where(x => x != 0));
        }

        public static List<AiData> ai_list = new List<AiData>();
        public static Dictionary<int, AiData> ai_dict = new Dictionary<int, AiData>();

        public static void Load(string path = "")
        {
            AiDataTable aiDataTable = Resources.Load<AiDataTable>(path);

            if (aiDataTable == null)
            {
                Debug.LogError("AiDataTable is not found");
                return;
            }

            foreach (var aiEntity in aiDataTable.items)
            {
                string assetPath = $"Assets/ERang/Resources/Ais/{aiEntity.Ai_Id}.asset";
                AiData aiData = AssetDatabase.LoadAssetAtPath<AiData>(assetPath);

                if (aiData == null)
                {
                    aiData = CreateInstance<AiData>();
                    AssetDatabase.CreateAsset(aiData, assetPath);
                }
                else
                {
                    aiData.ability_Ids.Clear();
                    aiData.attackRanges.Clear();
                }

                aiData.Initialize(aiEntity);

                ai_list.Add(aiData);
                ai_dict.Add(aiData.ai_Id, aiData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static AiData GetAiData(int aiId)
        {
            return ai_dict.ContainsKey(aiId) ? ai_dict[aiId] : null;
        }

        AiDataType ConvertAiDataType(string type)
        {
            return type switch
            {
                "Melee" => AiDataType.Melee,
                "Ranged" => AiDataType.Ranged,
                "Explosion" => AiDataType.Explosion,
                "Buff" => AiDataType.Buff,
                "DeBuff" => AiDataType.Debuff,
                _ => AiDataType.None,
            };
        }

        AiDataTarget ConvertAiDataTarget(string target)
        {
            // Remove spaces from the target string
            target = target.Replace(" ", "");

            return target switch
            {
                "NearEnemy" => AiDataTarget.NearEnemy,
                "Enemy" => AiDataTarget.Enemy,
                "RandomEnemy" => AiDataTarget.RandomEnemy,
                "RandomEnemyCreature" => AiDataTarget.RandomEnemyCreature,
                "AllEnemy" => AiDataTarget.AllEnemy,
                "AllEnemyCreature" => AiDataTarget.AllEnemyCreature,
                "Friendly" => AiDataTarget.Friendly,
                "AllFriendly" => AiDataTarget.AllFriendly,
                "AllFriendlyCreature" => AiDataTarget.AllFriendlyCreature,
                "Self" => AiDataTarget.Self,
                _ => AiDataTarget.None,
            };
        }

        AiDataAttackType ConvertAiDataAtackType(string atkType)
        {
            // Remove spaces from the atkType string
            atkType = atkType.Replace(" ", "");

            return atkType switch
            {
                "Automatic" => AiDataAttackType.Automatic,
                "SelectEnemy" => AiDataAttackType.SelectEnemy,
                "SelectFriendly" => AiDataAttackType.SelectFriendly,
                "SelectEnemyCreature" => AiDataAttackType.SelectEnemyCreature,
                "SelectFriendlyCreature" => AiDataAttackType.SelectFriendlyCreature,
                _ => AiDataAttackType.None,
            };
        }
    }
}