using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Pick all targets with the highest stat

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/HighestStat", order = 10)]
    public class FilterHighestStat : FilterData
    {
        public EffectStatType stat;

        public override List<Card> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Card> source, List<Card> dest)
        {
            //Find highest
            int highest = -999;
            foreach (Card scard in source)
            {
                int stat = GetStat(scard);
                if (stat > highest)
                    highest = stat;
            }

            //Add all highest
            foreach (Card scard in source)
            {
                int stat = GetStat(scard);
                if (stat == highest)
                    dest.Add(scard);
            }

            return dest;
        }

        public override List<BattleCharacter> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<BattleCharacter> source, List<BattleCharacter> dest)
        {
            //Find highest
            int highest = -999;
            foreach (BattleCharacter character in source)
            {
                int stat = GetStat(character);
                if (stat > highest)
                    highest = stat;
            }

            //Add all lowest
            foreach (BattleCharacter character in source)
            {
                int stat = GetStat(character);
                if (stat == highest)
                    dest.Add(character);
            }

            return dest;
        }

        private int GetStat(Card card)
        {
            if (stat == EffectStatType.Mana)
            {
                return card.GetMana();
            }
            return 0;
        }

        private int GetStat(BattleCharacter character)
        {
            if (stat == EffectStatType.HP)
            {
                return character.GetHP();
            }
            if (stat == EffectStatType.Mana)
            {
                return character.GetMana();
            }
            return 0;
        }
    }
}
