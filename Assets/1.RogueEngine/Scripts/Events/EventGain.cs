using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    //Event to gain items or cards

    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Gain", order = 10)]
    public class EventGain : EventData
    {
        [Header("Target")]
        public EventTarget target;

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
            if (gain_ally.Length > 0)
            {
                int slot_x = world.GetEmptySlotPos();
                return slot_x > 0;
            }
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion triggerer)
        {
            if (target == EventTarget.ChampionActive)
            {
                Gain(logic, triggerer);
            }

            if (target == EventTarget.ChampionAll)
            {
                foreach (Champion champion in logic.WorldData.champions)
                {
                    Gain(logic, champion);
                }
            }

            if (!string.IsNullOrEmpty(text))
                logic.TriggerTextEvent(triggerer, text);

            if (chain_event != null)
                logic.TriggerEventNext(triggerer, chain_event);
        }

        public void Gain(WorldLogic logic, Champion champion)
        {
            foreach (CardData card in gain_cards)
                champion.AddCard(card);
            foreach (CardData item in gain_items)
                champion.AddItem(item);

            foreach (CharacterData character in gain_ally)
                logic.GainAlly(character, champion.player_id);

            Player player = logic.WorldData.GetPlayer(champion.player_id);
            player.gold += gain_gold;
            champion.xp += gain_xp;
            champion.damage -= hp_heal;
            champion.damage = Mathf.Max(champion.damage, 0);
        }

    }
}
