using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ERang.Table;

namespace ERang.Data
{
    public class ConditionData
    {
        public int id; // Condition Id를 지정한다.
        public ConditionTarget target; // 체크할 대상을 선정함
        public ConditionType type; // 조건 타입 입력
        public ConditionCheckPoint checkPoint; // 조건이 발동되는 시점
        public ConditionMethod method; // 해당 조건의 지속 방식
        public int value;
        public string compare;

        public void Initialize(ConditionDataEntity entity)
        {
            id = entity.ConditionData_Id;
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
                if (conditionData_dict.ContainsKey(conditionEntity.ConditionData_Id))
                    continue;

                ConditionData conditionData = new();

                conditionData.Initialize(conditionEntity);

                conditionData_list.Add(conditionData);
                conditionData_dict.Add(conditionData.id, conditionData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ConditionData GetConditionData(int id)
        {
            if (conditionData_dict.ContainsKey(id))
            {
                return conditionData_dict[id];
            }

            return null;
        }

        private ConditionTarget ConvertSetTarget(string setTarget)
        {
            return setTarget switch
            {
                "NearEnemy" => ConditionTarget.NearEnemy,
                "Self" => ConditionTarget.Self,
                "Enemy1" => ConditionTarget.Enemy1,
                "Enemy2" => ConditionTarget.Enemy2,
                "Enemy3" => ConditionTarget.Enemy3,
                "Enemy4" => ConditionTarget.Enemy4,
                "FriendlyCreature" => ConditionTarget.FriendlyCreature,
                "EnemyCreature" => ConditionTarget.EnemyCreature,
                "Card" => ConditionTarget.Card,
                _ => ConditionTarget.None,
            };
        }

        private ConditionType ConvertConditionType(string type)
        {
            return type switch
            {
                "Buff" => ConditionType.Buff,
                "Debuff" => ConditionType.Debuff,
                "Hp" => ConditionType.Hp,
                "EveryTurn" => ConditionType.EveryTurn,
                "Extinction" => ConditionType.Extinction,
                "Acquisition" => ConditionType.Acquisition,
                _ => ConditionType.None,
            };
        }

        private ConditionCheckPoint ConvertCheckPoint(string checkPoint)
        {
            return checkPoint switch
            {
                "TurnStart" => ConditionCheckPoint.TurnStart,
                "TurnEnd" => ConditionCheckPoint.TurnEnd,
                "Immediately" => ConditionCheckPoint.Immediately,
                _ => ConditionCheckPoint.None,
            };
        }

        private ConditionMethod ConvertConditionMethod(string method)
        {
            return method switch
            {
                "Always" => ConditionMethod.Always,
                "StageOneTime" => ConditionMethod.StageOneTime,
                "GameOneTime" => ConditionMethod.GameOneTime,
                _ => ConditionMethod.None,
            };
        }
    }
}