using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    public class ConditionData : ScriptableObject
    {
        public int conditionData_Id; // Condition Id를 지정한다.
        public string setTarget; // 체크할 대상을 선정함
        public string conditionType; // 조건 타입 입력
        public string checkPoint; // 조건이 발동되는 시점
        public string method; // 해당 조건의 지속 방식
        public int value;
        public string compare;

        public void Initialize(ConditionDataEntity entity)
        {
            conditionData_Id = entity.ConditionData_Id;
            setTarget = entity.SetTarget;
            conditionType = entity.ConditionType;
            checkPoint = entity.CheckPoint;
            method = entity.Method;
            value = entity.Value;
            compare = entity.Compare;
        }

        public static List<ConditionData> conditionData_list = new List<ConditionData>();
        public static Dictionary<int, ConditionData> conditionData_dict = new Dictionary<int, ConditionData>();

        public static void Load(string path = "")
        {
            ConditionDataTable conditionDataTable = Resources.Load<ConditionDataTable>(path);

            if (conditionDataTable == null)
            {
                Debug.LogError("ConditionDataTable is not found");
                return;
            }

            foreach (var conditionEntity in conditionDataTable.items)
            {
                string assetPath = $"Assets/ERang/Resources/Conditions/{conditionEntity.ConditionData_Id}.asset";
                ConditionData conditionData = AssetDatabase.LoadAssetAtPath<ConditionData>(assetPath);

                if (conditionData == null)
                {
                    conditionData = CreateInstance<ConditionData>();
                    AssetDatabase.CreateAsset(conditionData, assetPath);
                }

                conditionData.Initialize(conditionEntity);

                conditionData_list.Add(conditionData);
                conditionData_dict.Add(conditionData.conditionData_Id, conditionData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}