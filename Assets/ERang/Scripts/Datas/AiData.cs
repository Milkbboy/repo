using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    public class AiData : ScriptableObject
    {
        public int ai_Id; // Ai의 Id 값
        public string type; // 행동의 타입을 정의
        public string target; // 대상 혹은 복수 대상을 설정한다.
        public string atk_Type; // 행동이 이루어지는 절차를 설정한다.
        public string atk_Range; // 공격 범위를 설정한다.
        public int atk_Cnt; // 공격 횟수
        public float atk_Interval; // 공격 횟수가 1이 아닐 경우 공격이 진행되는 텀을 지정
        public int value; // 해당 행동의 무게 값으로 Ai Group에서 참조된다.
        public int explosion_Shock; // Type이 Explosion일 경우에만 입력
        public string ability_id; // 실질적인 효과를 주는 Ability의 Id를 입력
        public List<int> atk_Ranges = new List<int>();

        public void Initialize(AiDataEntity entity)
        {
            ai_Id = entity.Ai_Id;
            type = entity.Type;
            target = entity.Target;
            atk_Type = entity.Atk_Type;
            atk_Range = entity.Atk_Range;
            atk_Cnt = entity.Atk_Cnt;
            atk_Interval = entity.Atk_Interval;
            value = entity.Value;
            explosion_Shock = entity.Explosion_Shock;
            ability_id = entity.Ability_id;

            atk_Ranges.AddRange(Utils.ParseIntArray(entity.Atk_Range).Where(x => x != 0));
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
                    aiData.atk_Ranges.Clear();
                }

                aiData.Initialize(aiEntity);

                ai_list.Add(aiData);
                ai_dict.Add(aiData.ai_Id, aiData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}