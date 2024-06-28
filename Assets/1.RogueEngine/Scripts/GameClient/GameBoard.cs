using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Client;
using UnityEngine.Events;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    /// <summary>
    /// GameBoard takes care of spawning and despawning BoardCharacters, based on the refreshed data received from the server
    /// It also ends the game when the server sends a endgame
    /// </summary>

    public class GameBoard : MonoBehaviour
    {
        public UnityAction<BoardCharacter> onCharacterSpawned;
        public UnityAction<BoardCharacter> onCharacterKilled;

        private static GameBoard _instance;

        void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            
        }

        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            int player_id = GameClient.Get().GetPlayerID();
            Battle data = GameClient.Get().GetBattle();

            //--- Battle characters --------

            List<BoardCharacter> characters = BoardCharacter.GetAll();

            //Add missing cards
            foreach (BattleCharacter character in data.characters)
            {
                BoardCharacter bcard = BoardCharacter.Get(character.uid);
                if (character != null && !character.IsDead() && bcard == null)
                    SpawnNewCharacter(character);
            }

            //Vanish removed cards
            for (int i = characters.Count - 1; i >= 0; i--)
            {
                BoardCharacter character = characters[i];
                if (character  && !character.IsDead())
                {
                    BattleCharacter bcharacter = data.GetCharacter(character.GetUID());
                    if (bcharacter == null || bcharacter.IsDead() || character.GetCharacterID() != bcharacter.character_id)
                    {
                        character.Kill();
                        onCharacterKilled?.Invoke(character);
                    }
                }
            }

        }

        private void SpawnNewCharacter(BattleCharacter character)
        {
            GameObject obj = Instantiate(character.GetPrefab(), Vector3.zero, Quaternion.identity);
            obj.SetActive(true);
            obj.GetComponent<BoardCharacter>().SetCharacter(character);
            onCharacterSpawned?.Invoke(obj.GetComponent<BoardCharacter>());
        }

        //Raycast mouse position to board position
        public Vector3 RaycastMouseBoard()
        {
            Ray ray = GameCamera.GetCamera().ScreenPointToRay(Input.mousePosition);
            bool success = Physics.Raycast(ray, out RaycastHit hit, 100f);
            if (success)
                return ray.GetPoint(hit.distance);

            Plane plane = new Plane(transform.forward, -10f);
            success = plane.Raycast(ray, out float dist);
            if (success)
                return ray.GetPoint(dist);

            return Vector3.zero;
        }

        public Vector3 GetAngles()
        {
            return transform.rotation.eulerAngles;
        }

        public static GameBoard Get()
        {
            return _instance;
        }
    }
}