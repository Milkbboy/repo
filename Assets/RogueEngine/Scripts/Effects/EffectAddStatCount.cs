using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Effect that sets stats equal to a dynamic calculated value from a pile (number of cards on board/hand/deck)
    /// </summary>
    
    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SetStatCount", order = 10)]
    public class EffectAddStatCount : EffectData
    {
        public EffectStatType type;
        public PileType pile;

        [Header("Count Traits")]
        public ItemType has_type;
        public TeamData has_team;
        public TraitData has_trait;

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, BattleCharacter target)
        {
            int val = CountPile(caster, pile) * ability.GetValue(caster, card);
            if (type == EffectStatType.HP)
            {
                target.hp += val;
            }

            if (type == EffectStatType.Mana)
            {
                target.mana += val;
                target.mana = Mathf.Max(target.mana, 0);
            }
        }

        public override void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            int val = CountPile(caster, pile) * ability.GetValue(caster, card);
            if (type == EffectStatType.Mana)
                target.mana += val;
        }

        public override void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter caster, Card card, Card target)
        {
            int val = CountPile(caster, pile) * ability.GetValue(caster, card);
            if (type == EffectStatType.Mana)
                target.mana_ongoing += val;
        }

        private int CountPile(BattleCharacter owner, PileType pile)
        {
            List<Card> card_pile = null;

            if (pile == PileType.Hand)
                card_pile = owner.cards_hand;

            if (pile == PileType.Power)
                card_pile = owner.cards_power;

            if (pile == PileType.Deck)
                card_pile = owner.cards_deck;

            if (pile == PileType.Discard)
                card_pile = owner.cards_discard;

            if (pile == PileType.Void)
                card_pile = owner.cards_void;

            if (pile == PileType.Temp)
                card_pile = owner.cards_temp;

            if (card_pile != null)
            {
                int count = 0;
                foreach (Card card in card_pile)
                {
                    if (IsTrait(card))
                        count++;
                }
                return count;
            }
            return 0;
        }

        private bool IsTrait(Card card)
        {
            bool is_type = card.CardData.item_type == has_type || has_type == ItemType.None;
            bool is_team = card.CardData.team == has_team || has_team == null;
            bool is_trait = card.HasTrait(has_trait) || has_trait == null;
            return (is_type && is_team && is_trait);
        }
    }
}