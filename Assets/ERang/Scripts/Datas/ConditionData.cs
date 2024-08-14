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
        public ConditionTarget target; // 체크할 대상을 선정함
        public ConditionType type; // 조건 타입 입력
        public ConditionCheckPoint checkPoint; // 조건이 발동되는 시점
        public ConditionMethod method; // 해당 조건의 지속 방식
        public int value;
        public string compare;

        public void Initialize(ConditionDataEntity entity)
        {
            conditionData_Id = entity.ConditionData_Id;
            target = ConvertSetTarget(entity.SetTarget);
            type = ConvertConditionType(entity.ConditionType);
            checkPoint = ConvertCheckPoint(entity.CheckPoint);
            method = ConvertConditionMethod(entity.Method);
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

        private ConditionTarget ConvertSetTarget(string setTarget)
        {
            switch (setTarget)
            {
                case "NearEnemy": return ConditionTarget.NearEnemy;
                case "Self": return ConditionTarget.Self;
                case "Enemy1": return ConditionTarget.Enemy1;
                case "Enemy2": return ConditionTarget.Enemy2;
                case "Enemy3": return ConditionTarget.Enemy3;
                case "Enemy4": return ConditionTarget.Enemy4;
                case "FriendlyCreature": return ConditionTarget.FriendlyCreature;
                case "EnemyCreature": return ConditionTarget.EnemyCreature;
                case "Card": return ConditionTarget.Card;
                default: return ConditionTarget.None;
            }
        }

        private ConditionType ConvertConditionType(string type)
        {
            switch (type)
            {
                case "Buff": return ConditionType.Buff;
                case "Debuff": return ConditionType.Debuff;
                case "HP": return ConditionType.HP;
                case "EveryTurn": return ConditionType.EveryTurn;
                case "Extinction": return ConditionType.Extinction;
                case "Acquisition": return ConditionType.Acquisition;
                default: return ConditionType.None;
            }
        }

        private ConditionCheckPoint ConvertCheckPoint(string checkPoint)
        {
            switch (checkPoint)
            {
                case "TurnStart": return ConditionCheckPoint.TurnStart;
                case "TurnEnd": return ConditionCheckPoint.TurnEnd;
                case "Immediately": return ConditionCheckPoint.Immediately;
                default: return ConditionCheckPoint.None;
            }
        }

        private ConditionMethod ConvertConditionMethod(string method)
        {
            switch (method)
            {
                case "Always": return ConditionMethod.Always;
                case "StageOneTime": return ConditionMethod.StageOneTime;
                case "GameOneTime": return ConditionMethod.GameOneTime;
                default: return ConditionMethod.None;
            }
        }
    }
}