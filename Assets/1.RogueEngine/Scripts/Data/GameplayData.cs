using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.AI;

namespace RogueEngine
{
    /// <summary>
    /// Generic gameplay settings, such as starting stats, decks limit, scenes, and ai level
    /// </summary>

    [CreateAssetMenu(fileName = "GameplayData", menuName = "TcgEngine/GameplayData", order = 0)]
    public class GameplayData : ScriptableObject
    {
        [Header("Gameplay Map")]
        public int xp_per_level = 100;

        [Header("Gameplay Battle")]
        public int cards_max = 10;
        [HideInInspector] public float turn_duration = 60f; //Not in use

        [Header("AI")]
        public AIType ai_type;              //AI algorythm

        [Header("Scenarios")]
        public ScenarioData[] scenarios;

        [Header("Progression")]
        public int progress_unlock_cards = 2;
        public int progress_unlock_items = 1;
        public TeamData neutral_team;

        [Header("Test")]
        public ScenarioData test_scenario;
        public ChampionData test_champion;
        public EventBattle test_battle;

        public int GetXpForNextLevel(int level)
        {
            return xp_per_level;
        }

        public ScenarioData GetDefaultScenario(bool multiplayer)
        {
            foreach (ScenarioData scenario in scenarios)
            {
                if (!multiplayer || scenario.champions >= 2)
                    return scenario;
            }
            return null;
        }

        public static GameplayData Get()
        {
            return DataLoader.Get().data;
        }
    }
}