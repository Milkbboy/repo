using RogueEngine.Gameplay;
using UnityEngine;

namespace RogueEngine
{
    //Event to lose items or cards

    [CreateAssetMenu(fileName = "Event", menuName = "TcgEngine/MapEvent/Lose", order = 10)]
    public class EventLose : EventData
    {
        [Header("Target")]
        public EventTarget target;

        [Header("Spend")]
        public CardData[] lose_cards;
        public CardData[] lose_items;
        public CharacterData[] lose_ally;
        public int lose_gold;
        public int hp_damage;

        [Header("Text")]
        [TextArea(5, 8)]
        public string text;

        [Header("Chain Event")]
        public EventData chain_event;

        public override bool AreEventsConditionMet(World world, Champion triggerer)
        {
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion triggerer)
        {
            if (target == EventTarget.ChampionActive)
            {
                Spend(logic, triggerer);
            }

            if (target == EventTarget.ChampionAll)
            {
                foreach (Champion champion in logic.WorldData.champions)
                {
                    Spend(logic, champion);
                }
            }

            if (!string.IsNullOrEmpty(text))
                logic.TriggerTextEvent(triggerer, text);

            if (chain_event != null)
                logic.TriggerEventNext(triggerer, chain_event);
        }

        public void Spend(WorldLogic logic, Champion champion)
        {
            foreach (CardData card in lose_cards)
                champion.RemoveCard(card);
            foreach (CardData item in lose_items)
                champion.RemoveItem(item);

            foreach (CharacterData character in lose_ally)
            {
                CharacterAlly ally = logic.WorldData.GetAlly(character);
                logic.RemoveAlly(ally);
            }

            Player player = logic.WorldData.GetPlayer(champion.player_id);
            player.gold -= lose_gold;
            player.gold = Mathf.Max(player.gold, 0);

            champion.damage += hp_damage;
            champion.damage = Mathf.Max(champion.damage, 0);
        }

    }
}
