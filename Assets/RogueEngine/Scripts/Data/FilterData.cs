using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Base class for target filters
    /// Let you filter targets after they have already been picked by conditions but before effects are applied
    /// </summary>

    public class FilterData : ScriptableObject
    {
        public virtual List<Card> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Card> source, List<Card> dest)
        {
            return source; //Override this, filter targeting card
        }

        public virtual List<BattleCharacter> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<BattleCharacter> source, List<BattleCharacter> dest)
        {
            return source; //Override this, filter targeting character
        }

        public virtual List<Slot> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Slot> source, List<Slot> dest)
        {
            return source; //Override this, filter targeting slot
        }

        public virtual List<CardData> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<CardData> source, List<CardData> dest)
        {
            return source; //Override this, for filters that create new cards
        }
    }
}
