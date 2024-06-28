using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    [CreateAssetMenu(fileName = "Battle", menuName = "TcgEngine/MapEvent/Battle", order = 10)]
    public class EventBattle : EventData
    {
        [Header("Battle")]
        public string scene;
        public int enemies_level = 1;
        public CharacterData[] enemies;

        [Header("Extra Enemies")]
        public ExtraBattleEnemy[] extra_enemies; //Extra enemies at higher champion count

        [Header("Rewards")]
        public float reward_gold;                  //Added to gold earn for each enemy
        public float reward_xp;                     //Added to xp earn for each enemy
        public bool reward_cards = true;     
        public RarityData card_rarity;      //Keep null for ANY rarity, for cards, the team is set on the ChampionData instead of here
        public bool reward_items = false;
        public RarityData item_rarity;     //Keep null for ANY rarity
        public TeamData item_team;          //Team of possible items,   

        [Header("After")]
        public EventData win_event;

        public override bool AreEventsConditionMet(World world, Champion champion)
        {
            return true;
        }

        public override void DoEvent(WorldLogic logic, Champion champion)
        {
            logic.StartBattle(this);
        }

        public static new EventBattle Get(string id)
        {
            foreach (EventData battle in GetAll())
            {
                if (battle.id == id && battle is EventBattle)
                    return battle as EventBattle;
            }
            return null;
        }
    }

    [System.Serializable]
    public struct ExtraBattleEnemy
    {
        public int champions_min;
        public CharacterData[] enemies;
    }
}
