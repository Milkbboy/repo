using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Base class for all ability conditions, override the IsConditionMet function
    /// </summary>

    public class ConditionData : ScriptableObject
    {
        public virtual bool IsMapEventConditionMet(World data, EventEffect evt, Champion champion)
        {
            return true; //Override this, condition for map events
        }

        public virtual bool IsMapTriggerConditionMet(World data, AbilityData evt, Champion champion, ChampionItem item)
        {
            return true; //Override this, Ongoing effect for map (like passive items with out-of-battle effects)
        }

        public virtual bool IsMapTargetConditionMet(World data, AbilityData evt, Champion champion, ChampionItem item, Champion target)
        {
            return true; //Override this, Ongoing effect for map (like passive items with out-of-battle effects)
        }

        public virtual bool IsTriggerConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card)
        {
            return true; //Override this, battle condition, applies to any target, always checked
        }

        public virtual bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, Card target)
        {
            return true; //Override this, battle condition targeting card
        }

        public virtual bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, BattleCharacter target)
        {
            return true; //Override this, battle condition targeting a character
        }

        public virtual bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, Slot target)
        {
            return true; //Override this, battle condition targeting slot
        }

        public virtual bool IsTargetConditionMet(Battle data, AbilityData ability, BattleCharacter character, Card card, CardData target)
        {
            return true; //Override this, battle condition for effects that create new cards
        }

        public bool CompareBool(bool condition, ConditionOperatorBool oper)
        {
            if (oper == ConditionOperatorBool.IsFalse)
                return !condition;
            return condition;
        }

        public bool CompareInt(int ival1, ConditionOperatorInt oper, int ival2)
        {
            if (oper == ConditionOperatorInt.Equal)
            {
                return ival1 == ival2;
            }
            if (oper == ConditionOperatorInt.NotEqual)
            {
                return ival1 != ival2;
            }
            if (oper == ConditionOperatorInt.GreaterEqual)
            {
                return ival1 >= ival2;
            }
            if (oper == ConditionOperatorInt.LessEqual)
            {
                return ival1 <= ival2;
            }
            if (oper == ConditionOperatorInt.Greater)
            {
                return ival1 > ival2;
            }
            if (oper == ConditionOperatorInt.Less)
            {
                return ival1 < ival2;
            }
            return false;
        }
    }

    public enum ConditionOperatorInt
    {
        Equal,
        NotEqual,
        GreaterEqual,
        LessEqual,
        Greater,
        Less,
    }

    public enum ConditionOperatorBool
    {
        IsTrue,
        IsFalse,
    }
}