using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    public class AiGroupData : ScriptableObject
    {
        public int aiGroup_Id;
        public string ai_Group_1;
        public string ai_Group_2;
        public string ai_Group_3;
        public string ai_Group_4;
        public string ai_Group_5;
        public string ai_Group_6;
        public int reaction_Condition_1;
        public float reaction_Condition_1_Ratio;
        public int reaction_Ai_1;
        public int reaction_Condition_2;
        public float reaction_Condition_2_Ratio;
        public int reaction_Ai_2;
        public int reaction_Condition_3;
        public float reaction_Condition_3_Ratio;
        public int reaction_Ai_3;
        public AiGroupType aiGroupType;

        // 이중 배열
        public List<List<int>> ai_Groups = new();

        public static List<AiGroupData> aiGroups_list = new();
        public static Dictionary<int, AiGroupData> aiGroups_dict = new();

        // 중첩 클래스 Reaction 선언
        public class Reaction
        {
            public int conditionId;
            public float ratio;
            public int aiDataId;

            public Reaction(int condition, float ratio, int ai)
            {
                conditionId = condition;
                this.ratio = ratio;
                aiDataId = ai;
            }
        }

        public List<Reaction> reactions = new();

        public void Initialize(AiGroupDataEntity entity)
        {
            aiGroup_Id = entity.AiGroup_Id;

            switch (entity.Type)
            {
                case "Repeat": aiGroupType = AiGroupType.Repeat; break;
                case "Random": aiGroupType = AiGroupType.Random; break;
            }

            string[] aiGroups = new string[] { entity.Ai_Group_1, entity.Ai_Group_2, entity.Ai_Group_3, entity.Ai_Group_4, entity.Ai_Group_5, entity.Ai_Group_6 };

            // 이중 배열에 AiData 를 그룹으로(배열) 만들어서 추가
            // Ai_Group_1: [ 1001, 2001], Ai_Group_2: [ 1002 ], Ai_Group_3: [ 1003, 2003, 1004 ]
            foreach (var aiGroup in aiGroups)
            {
                AddAiGroup(aiGroup);
            }

            AddReaction(entity.Reaction_Condition_1, entity.Reaction_Condition_1_Ratio, entity.Reaction_Ai_1);
            AddReaction(entity.Reaction_Condition_2, entity.Reaction_Condition_2_Ratio, entity.Reaction_Ai_2);
            AddReaction(entity.Reaction_Condition_3, entity.Reaction_Condition_3_Ratio, entity.Reaction_Ai_3);

            ai_Group_1 = entity.Ai_Group_1;
            ai_Group_2 = entity.Ai_Group_2;
            ai_Group_3 = entity.Ai_Group_3;
            ai_Group_4 = entity.Ai_Group_4;
            ai_Group_5 = entity.Ai_Group_5;
            ai_Group_6 = entity.Ai_Group_6;
            reaction_Condition_1 = entity.Reaction_Condition_1;
            reaction_Condition_1_Ratio = entity.Reaction_Condition_1_Ratio;
            reaction_Ai_1 = entity.Reaction_Ai_1;
            reaction_Condition_2 = entity.Reaction_Condition_2;
            reaction_Condition_2_Ratio = entity.Reaction_Condition_2_Ratio;
            reaction_Ai_2 = entity.Reaction_Ai_2;
            reaction_Condition_3 = entity.Reaction_Condition_3;
            reaction_Condition_3_Ratio = entity.Reaction_Condition_3_Ratio;
            reaction_Ai_3 = entity.Reaction_Ai_3;
        }

        public static void Load(string path = "")
        {
            AiGroupDataTable aiGroupDataTable = Resources.Load<AiGroupDataTable>(path);

            if (aiGroupDataTable == null)
            {
                Debug.LogError("AiGroupDataTable is not found");
                return;
            }

            foreach (var aiGroupEntity in aiGroupDataTable.items)
            {
                string assetPath = $"Assets/ERang/Resources/AiGroups/{aiGroupEntity.AiGroup_Id}.asset";
                AiGroupData aiGroupData = AssetDatabase.LoadAssetAtPath<AiGroupData>(assetPath);

                if (aiGroupData == null)
                {
                    aiGroupData = CreateInstance<AiGroupData>();
                    AssetDatabase.CreateAsset(aiGroupData, assetPath);
                }
                else
                {
                    foreach (var aiGroup in aiGroupData.ai_Groups)
                    {
                        aiGroup.Clear();
                    }
                }
                aiGroupData.Initialize(aiGroupEntity);

                aiGroups_list.Add(aiGroupData);
                aiGroups_dict.Add(aiGroupData.aiGroup_Id, aiGroupData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static AiGroupData GetAiGroupData(int aiGroupId)
        {
            return aiGroups_dict.TryGetValue(aiGroupId, out AiGroupData aiGroupData) ? aiGroupData : null;
        }

        /// <summary>
        /// Ai_Group 값이 있으면 aiGroup을 추가한다.
        /// </summary>
        /// <param name="aiGroup"></param>
        private void AddAiGroup(string aiGroup)
        {
            var group = Utils.ParseIntArray(aiGroup).Where(x => x != 0).ToList();

            if (group.Any() == false)
                return;

            ai_Groups.Add(group);
        }

        private void AddReaction(int condition, float ratio, int ai)
        {
            if (condition == 0)
                return;

            Reaction reaction = new(condition, ratio, ai);
            reactions.Add(reaction);
        }
    }
}