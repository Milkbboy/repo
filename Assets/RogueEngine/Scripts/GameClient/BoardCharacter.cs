using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.UI;
using RogueEngine.Client;
using UnityEngine.Events;

namespace RogueEngine
{

    public class BoardCharacter : MonoBehaviour
    {
        public float move_speed = 5f;
        public float death_delay = 0.7f;

        public UnityAction onKill;

        private CharacterUI ui;

        private string uid;
        private string character_id;
        private bool killed = false;
        private bool focus = false;

        private Collider collide;
        private float update_timer = 0f;

        private static List<BoardCharacter> character_list = new List<BoardCharacter>();

        void Awake()
        {
            character_list.Add(this);
            ui = GetComponentInChildren<CharacterUI>();
            collide = GetComponent<Collider>();
        }

        void OnDestroy()
        {
            character_list.Remove(this);
        }

        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter character = battle.GetCharacter(uid);
            if (character == null)
                return;

            Vector3 tpos = transform.position;
            BoardSlot bslot = BoardSlot.Get(character.slot);
            if (bslot != null)
                tpos = bslot.transform.position;

            transform.position = Vector3.MoveTowards(transform.position, tpos, move_speed * Time.deltaTime);
            ui?.Set(character);

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            
        }

        public void SetChampion(Champion champion)
        {
            uid = champion.uid;
            character_id = champion.character_id;
            ui?.Set(champion);
        }

        public void SetCharacter(BattleCharacter character)
        {
            uid = character.uid;
            character_id = character.character_id;

            BoardSlot bslot = BoardSlot.Get(character.slot);
            if (bslot != null)
                transform.position = bslot.transform.position;
        }

        public void Kill()
        {
            killed = true;
            onKill?.Invoke();
            Destroy(gameObject, death_delay);
        }

        public bool IsDead()
        {
            return killed;
        }

        public BattleCharacter GetCharacter()
        {
            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter character = battle?.GetCharacter(uid);
            return character;
        }

        public string GetCharacterID()
        {
            return character_id;
        }

        public int GetPlayerID()
        {
            BattleCharacter character = GetCharacter();
            if(character != null)
                return character.player_id;
            return -1;
        }

        public string GetUID()
        {
            return uid;
        }

        public bool IsFocus()
        {
            return focus;
        }

        public bool IsActive()
        {
            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter active = battle.GetActiveCharacter();
            return active != null && GetUID() == battle.GetActiveCharacter().uid;
        }

        public void OnMouseDown()
        {
            if (BattleUI.IsOverUILayer("UI"))
                return;

            PlayerControls controls = PlayerControls.Get();
            if (controls != null)
            {
                controls.SelectCharacter(this);
            }
        }

        private void OnMouseEnter()
        {
            focus = true;
        }

        private void OnMouseExit()
        {
            focus = false;
        }

        private void OnDisable()
        {
            focus = false;
        }

        public static BoardCharacter Get(string uid)
        {
            foreach (BoardCharacter character in character_list)
            {
                if (character.uid == uid)
                    return character;
            }
            return null;
        }

        public static BoardCharacter GetFocus()
        {
            foreach (BoardCharacter character in character_list)
            {
                if (character.focus)
                    return character;
            }
            return null;
        }

        public static BoardCharacter GetMouseRaycast(float range = 999f)
        {
            Ray ray = GameCamera.GetCamera().ScreenPointToRay(Input.mousePosition);
            foreach (BoardCharacter bcharacter in GetAll())
            {
                if (bcharacter.collide != null && bcharacter.collide.Raycast(ray, out RaycastHit hit, range))
                    return bcharacter;
            }
            return null;
        }

        public static List<BoardCharacter> GetAll()
        {
            return character_list;
        }
    }
}