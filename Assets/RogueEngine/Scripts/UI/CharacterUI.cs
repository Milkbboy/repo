using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class CharacterUI : MonoBehaviour
    {
        [Header("UI")]
        public Text username;
        public Image turn_glow;
        public Image select_glow;
        public ProgressBar hp_bar;
        public GameObject shield_group;
        public Text shield;

        public IntentIcon intent_icon;
        public StatusIcon[] status_icons;

        private BoardCharacter bcharacter;
        private BattleCharacter character;
        private Canvas canvas;

        void Awake()
        {
            bcharacter = GetComponentInParent<BoardCharacter>();
            canvas = GetComponent<Canvas>();
            username.enabled = false;
            turn_glow.enabled = false;
            select_glow.enabled = false;
            shield.text = "";
            shield_group.SetActive(false);
            hp_bar.value = 100;
            hp_bar.value_max = 100;
            intent_icon.Hide();

            foreach (StatusIcon icon in status_icons)
                icon.Hide();
        }

        private void Update()
        {
            if (character != null && !character.IsDead())
            {
                canvas.sortingOrder = bcharacter.IsFocus() ? 10 : 5;
                select_glow.enabled = InitiativeLine.GetHoverCharacter() == character;
            }
        }

        public void Set(Champion champion)
        {
            hp_bar.value = champion.GetHP();
            hp_bar.value_max = champion.GetHPMax();
            intent_icon.Hide();

            World world = GameClient.Get().GetWorld();
            Player player = world?.GetPlayer(champion.player_id);
            username.enabled = player != null && world.IsMultiplayer();
            if (player != null)
                username.text = player.username;

            UpdateStatus();
        }

        public void Set(BattleCharacter character)
        {
            this.character = character;

            Battle battle = GameClient.Get().GetBattle();
            hp_bar.value = character.GetHP();
            hp_bar.value_max = character.GetHPMax();
            shield.text = character.GetShield().ToString();
            shield.enabled = character.GetShield() > 0;
            turn_glow.enabled = (character.uid == battle.active_character);
            select_glow.enabled = false;
            shield_group.SetActive(character.GetShield() > 0);
            intent_icon.Hide();

            World world = GameClient.Get().GetWorld();
            Player player = world?.GetPlayer(character.player_id);
            username.enabled = player != null && !character.IsEnemy() && world.IsMultiplayer();
            if (player != null)
                username.text = player.username;

            SetIntent(character);
            UpdateStatus();
        }

        public void SetIntent(BattleCharacter character)
        {
            Battle battle = GameClient.Get().GetBattle();
            if (battle.IsCharacterTurn(character))
                return; //Currently playing 
            if (!character.CanPlayTurn())
                return;  //Has status preventing from playing

            if (character.CharacterData != null && character.CharacterData.behavior != null)
            {
                int priority = -999;
                int actions = character.CharacterData.behavior.GetActionsPerTurn(battle, character, character.turn);
                for (int i = 0; i < actions; i++)
                {
                    Card card = battle.GetIntentCard(character, character.turn + i);
                    if (card != null && card.CardData.intent != null && card.CardData.intent.priority > priority)
                    {
                        priority = card.CardData.intent.priority;
                        intent_icon.SetIntent(card.CardData.intent, card.GetAbilityValue(character));
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            if (character != null)
            {
                int index = 0;

                foreach (CardTrait trait in character.GetAllTraits())
                {
                    TraitData itrait = TraitData.Get(trait.id);
                    if (itrait != null && itrait.icon != null && index < status_icons.Length)
                    {
                        StatusIcon icon = status_icons[index];
                        icon.SetTrait(itrait, trait.value);
                        index++;
                    }
                }

                foreach (CardStatus status in character.GetAllStatus())
                {
                    StatusData istatus = StatusData.Get(status.id);
                    if (istatus != null && istatus.icon != null && index < status_icons.Length)
                    {
                        StatusIcon icon = status_icons[index];
                        icon.SetStatus(istatus, status.value);
                        index++;
                    }
                }

                for (int i = index; i < status_icons.Length; i++)
                    status_icons[i].Hide();
            }
        }
    }
}
