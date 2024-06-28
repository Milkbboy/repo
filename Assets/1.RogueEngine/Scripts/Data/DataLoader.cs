using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Client;

namespace RogueEngine
{

    /// <summary>
    /// This script initiates loading all the game data
    /// </summary>

    public class DataLoader : MonoBehaviour
    {
        public GameplayData data;
        public AssetData assets;

        private HashSet<string> card_ids = new HashSet<string>();
        private HashSet<string> ability_ids = new HashSet<string>();
        private HashSet<string> deck_ids = new HashSet<string>();

        private static DataLoader instance;

        void Awake()
        {
            instance = this;
            LoadData();
        }

        public void LoadData()
        {
            //To make loading faster, add a path inside each Load() function, relative to Resources folder
            //For example CardData.Load("Cards");  to only load data inside the Resources/Cards folder
            
            CardData.Load();
            TeamData.Load();
            RarityData.Load();
            TraitData.Load();
            AbilityData.Load();
            StatusData.Load();
            AvatarData.Load();
            CardbackData.Load();

            ChampionData.Load();
            CharacterData.Load();
            ScenarioData.Load();
            MapData.Load();
            EventData.Load();
            IntentData.Load();
            TipData.Load();

            CheckCharacterData();
            CheckCardData();
            CheckAbilityData();
        }

        //Make sure the data is valid
        private void CheckCharacterData()
        {
            card_ids.Clear();
            foreach (ChampionData character in ChampionData.GetAll())
            {
                if (string.IsNullOrEmpty(character.id))
                    Debug.LogError(character.name + " id is empty");
                if (card_ids.Contains(character.id))
                    Debug.LogError("Dupplicate Card ID: " + character.id);

                if (character.prefab == null)
                    Debug.LogError(character.id + " prefab is null");

                foreach (AbilityData ability in character.abilities)
                {
                    if (ability == null)
                        Debug.LogError(character.id + " has null ability");
                }

                foreach (CardData card in character.start_cards)
                {
                    if (card == null)
                        Debug.LogError(character.id + " has null card");
                }
                foreach (CardData item in character.start_items)
                {
                    if (item == null)
                        Debug.LogError(character.id + " has null item");
                }
                foreach (TeamData team in character.reward_cards)
                {
                    if (team == null)
                        Debug.LogError(character.id + " has null card");
                }

                card_ids.Add(character.id);
            }

            foreach (CharacterData character in CharacterData.GetAll())
            {
                if (string.IsNullOrEmpty(character.id))
                    Debug.LogError(character.name + " id is empty");
                if (card_ids.Contains(character.id))
                    Debug.LogError("Dupplicate Card ID: " + character.id);

                if (character.prefab == null)
                    Debug.LogError(character.id + " prefab is null");

                foreach (AbilityData ability in character.abilities)
                {
                    if (ability == null)
                        Debug.LogError(character.id + " has null ability");
                }

                foreach (CardData card in character.cards)
                {
                    if (card == null)
                        Debug.LogError(character.id + " has null card");
                }

                card_ids.Add(character.id);
            }
        }

        private void CheckCardData()
        {
            card_ids.Clear();
            foreach (CardData card in CardData.GetAll())
            {
                if (string.IsNullOrEmpty(card.id))
                    Debug.LogError(card.name + " id is empty");
                if (card_ids.Contains(card.id))
                    Debug.LogError("Dupplicate Card ID: " + card.id);

                if (card.team == null)
                    Debug.LogError(card.id + " team is null");
                if (card.rarity == null)
                    Debug.LogError(card.id + " rarity is null");

                foreach (TraitData trait in card.traits)
                {
                    if (trait == null)
                        Debug.LogError(card.id + " has null trait");
                }

                if (card.stats != null)
                {
                    foreach (TraitStat stat in card.stats)
                    {
                        if (stat.trait == null)
                            Debug.LogError(card.id + " has null stat trait");
                    }
                }

                foreach (AbilityData ability in card.abilities)
                {
                    if(ability == null)
                        Debug.LogError(card.id + " has null ability");
                }

                card_ids.Add(card.id);
            }
        }

        //Make sure the data is valid
        private void CheckAbilityData()
        {
            ability_ids.Clear();
            foreach (AbilityData ability in AbilityData.GetAll())
            {
                if (string.IsNullOrEmpty(ability.id))
                    Debug.LogError(ability.name + " id is empty");
                if (ability_ids.Contains(ability.id))
                    Debug.LogError("Dupplicate Ability ID: " + ability.id);

                foreach (AbilityData chain in ability.chain_abilities)
                {
                    if (chain == null)
                        Debug.LogError(ability.id + " has null chain ability");
                }

                ability_ids.Add(ability.id);
            }
        }

        public static DataLoader Get()
        {
            return instance;
        }
    }
}