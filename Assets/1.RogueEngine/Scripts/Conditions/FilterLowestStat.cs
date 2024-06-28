using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Pick all targets with the lowest stat

    [CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/LowestStat", order = 10)]
    public class FilterLowestStat : FilterData
    {
        public EffectStatType stat;

        public override List<Card> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<Card> source, List<Card> dest)
        {
            //Find lowest
            int lowest = 99999;
            foreach (Card scard in source)
            {
                int stat = GetStat(scard);
                if (stat < lowest)
                    lowest = stat;
            }

            //Add all lowest
            foreach (Card scard in source)
            {
                int stat = GetStat(scard);
                if (stat == lowest)
                    dest.Add(scard);
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

        public override List<BattleCharacter> FilterTargets(Battle data, AbilityData ability, BattleCharacter caster, Card card, List<BattleCharacter> source, List<BattleCharacter> dest)
        {
            //Find lowest
            int lowest = 99999;
            foreach (BattleCharacter character in source)
            {
                int stat = GetStat(character);
                if (stat < lowest)
                    lowest = stat;
            }

            //Add all lowest
            foreach (BattleCharacter character in source)
            {
                int stat = GetStat(character);
                if (stat == lowest)
                    dest.Add(character);
            }

            return dest;
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
