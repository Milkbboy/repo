using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Pick X number of targets at random from the source array

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/Random", order = 10)]
    public class FilterRandom : FilterData
    {
        public int amount = 1; //Number of random targets selected

        public override List<Card> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Card> source, List<Card> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }

        public override List<BattleCharacter> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<BattleCharacter> source, List<BattleCharacter> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }

        public override List<Slot> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Slot> source, List<Slot> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }

        public override List<CardData> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<CardData> source, List<CardData> dest)
        {
            return GameTool.PickXRandom(source, dest, amount);
        }
    }
}
