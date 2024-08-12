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

        public List<List<int>> ai_Groups = new List<List<int>>();

        public void Initialize(AiGroupDataEntity entity)
        {
            aiGroup_Id = entity.AiGroup_Id;
            switch (entity.Type)
            {
                case "Repeat":
                    aiGroupType = AiGroupType.Repeat;
                    break;
                case "Random":
                    aiGroupType = AiGroupType.Random;
                    break;
            }
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

            ai_Groups.Add(Utils.ParseIntArray(ai_Group_1).Where(x => x != 0).ToList());
            ai_Groups.Add(Utils.ParseIntArray(ai_Group_2).Where(x => x != 0).ToList());
            ai_Groups.Add(Utils.ParseIntArray(ai_Group_3).Where(x => x != 0).ToList());
            ai_Groups.Add(Utils.ParseIntArray(ai_Group_4).Where(x => x != 0).ToList());
            ai_Groups.Add(Utils.ParseIntArray(ai_Group_5).Where(x => x != 0).ToList());
            ai_Groups.Add(Utils.ParseIntArray(ai_Group_6).Where(x => x != 0).ToList());
        }

        public static List<AiGroupData> aiGroups_list = new List<AiGroupData>();
        public static Dictionary<int, AiGroupData> aiGroups_dict = new Dictionary<int, AiGroupData>();

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
            if (aiGroups_dict.ContainsKey(aiGroupId))
            {
                return aiGroups_dict[aiGroupId];
            }

            return null;
        }
    }
}