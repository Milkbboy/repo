using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    //Event to gain items or cards, at the cost of other things

    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Trade", order = 10)]
    public class EventTrade : EventData
    {
        [Header("Target")]
        public EventTarget target;

        [Header("Spend")]
        public CardData[] spend_cards;
        public CardData[] spend_items;
        public CharacterData[] spend_ally;
        public int spend_gold;
        public int hp_damage;

        [Header("Gain")]
        public CardData[] gain_cards;
        public CardData[] gain_items;
        public CharacterData[] gain_ally;
        public int gain_gold;
        public int gain_xp;
        public int hp_heal;

        [Header("Text")]
        [TextArea(5, 8)]
        public string text;

        [Header("Chain Event")]
        public EventData chain_event;

        public override bool AreEventsConditionMet(World world, Champion triggerer)
        {
            if (target == EventTarget.ChampionActive)
            {
                return CanSpend(world, triggerer);
            }

            if (target == EventTarget.ChampionAll)
            {
                foreach (Champion champion in world.champions)
                {
                    if (CanSpend(world, champion))
                        return true;
                }
            }
            return false;
        }

        public override void DoEvent(WorldLogic logic, Champion triggerer)
        {
            if (target == EventTarget.ChampionActive)
            {
                if (CanSpend(logic.WorldData, triggerer))
                {
                    Trade(logic, triggerer);
                }
            }

            if (target == EventTarget.ChampionAll)
            {
                foreach (Champion champion in logic.WorldData.champions)
                {
                    if (CanSpend(logic.WorldData, champion))
                    {
                        Trade(logic, champion);
                    }
                }
            }

            if (!string.IsNullOrEmpty(text))
                logic.TriggerTextEvent(triggerer, text);

            if (chain_event != null)
                logic.TriggerEventNext(triggerer, chain_event);
        }

        public bool CanSpend(World world, Champion champion)
        {
            foreach (CardData card in spend_cards)
            {
                if (!champion.HasCard(card))
                    return false;
            }
            foreach (CardData item in spend_items)
            {
                if (!champion.HasItem(item))
                    return false;
            }

            foreach (CharacterData ally in spend_ally)
            {
                if (world.GetAlly(ally) != null)
                    return false;
            }

            Player player = world.GetPlayer(champion.player_id);
            return player.gold >= spend_gold;
        }

        public void Trade(WorldLogic logic, Champion champion)
        {
            foreach (CardData card in spend_cards)
                champion.RemoveCard(card);
            foreach (CardData item in spend_items)
                champion.RemoveItem(item);

            foreach (CharacterData character in spend_ally)
            {
                CharacterAlly ally = logic.WorldData.GetAlly(character);
                logic.RemoveAlly(ally);
            }

            foreach (CardData card in gain_cards)
                champion.AddCard(card);
            foreach (CardData item in gain_items)
                champion.AddItem(item);

            foreach (CharacterData character in gain_ally)
                logic.GainAlly(character, champion.player_id);

            Player player = logic.WorldData.GetPlayer(champion.player_id);
            player.gold += gain_gold - spend_gold;
            champion.xp += gain_xp;
            champion.damage += hp_damage - hp_heal;
            champion.damage = Mathf.Max(champion.damage, 0);
        }

    }
}
