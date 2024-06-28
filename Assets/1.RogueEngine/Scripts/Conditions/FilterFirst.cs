using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Pick X first cards from the source array

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/First", order = 10)]
    public class FilterFirst : FilterData
    {
        public int amount = 1; //Number of first targets selected

        public override List<Card> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Card> source, List<Card> dest)
        {
            int max = Mathf.Min(source.Count, amount);
            for (int i = 0; i < max; i++)
                dest.Add(source[i]);
            return dest;
        }

        public override List<BattleCharacter> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<BattleCharacter> source, List<BattleCharacter> dest)
        {
            int max = Mathf.Min(source.Count, amount);
            for (int i = 0; i < max; i++)
                dest.Add(source[i]);
            return dest;
        }

        public override List<Slot> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Slot> source, List<Slot> dest)
        {
            int max = Mathf.Min(source.Count, amount);
            for (int i = 0; i < max; i++)
                dest.Add(source[i]);
            return dest;
        }
    }
}
