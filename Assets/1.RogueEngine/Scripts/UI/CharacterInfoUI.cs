using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;

namespace RogueEngine.UI
{

    public class CharacterInfoUI : MonoBehaviour
    {
        public UIPanel panel;
        public UIPanel champion_panel;
        public UIPanel enemy_panel;

        public Text title;
        public Text level_txt;
        public Text status_txt;

        public Text skill_label_txt;
        public Text behavior_label_txt;
        public Text skill_txt;
        public Text behavior_txt;

        public Text hp_txt;
        public Text speed_txt; 
        public Text cards_txt;
        public Text energy_txt;

        public CardUIHover[] hand_cards;

        private static CharacterInfoUI instance;

        void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            canvas.worldCamera = GameCamera.GetUICamera();
            panel.Hide(true);
            enemy_panel.Hide(true);
        }

        void Update()
        {

        }

        public void ShowCharacter(BattleCharacter character)
        {
            Battle battle = GameClient.Get().GetBattle();
            BoardCharacter bcharacter = BoardCharacter.Get(character.uid);
            //if (bcharacter != null)
            //    transform.position = bcharacter.transform.position;

            title.text = character.is_champion ? character.ChampionData.GetTitle() : character.CharacterData.GetTitle();
            level_txt.text = "Level " + character.level;
            hp_txt.text = character.GetHP().ToString() + " / " + character.GetHPMax().ToString();
            speed_txt.text = character.GetSpeed().ToString();
            cards_txt.text = character.GetHand().ToString();
            energy_txt.text = character.GetEnergy().ToString();
            status_txt.text = GetStatusText(character);

            panel.Show();

            //Ability
            skill_txt.text = character.CharacterData != null ? character.CharacterData.GetAbilitiesDesc() : "";
            behavior_txt.text = character.CharacterData != null ? character.CharacterData.behavior.GetBehaviorText(): "";
            skill_label_txt.enabled = !string.IsNullOrEmpty(skill_txt.text);
            behavior_label_txt.enabled = !string.IsNullOrEmpty(behavior_txt.text);

            //Enemy Hand
            enemy_panel.SetVisible(character.IsEnemy(), true);

            foreach (CardUIHover ui in hand_cards)
                ui.Hide();

            int total_mana = character.GetEnergy();
            int index = 0;
            foreach (Card card in character.cards_hand)
            {
                if (index < hand_cards.Length)
                {
                    int cost = card.GetMana();
                    bool can_play = true; // total_mana >= cost && battle.CanPlayCardAnyTarget(card, true);
                    hand_cards[index].SetCard(bcharacter.GetCharacter(), card, can_play);
                    total_mana -= cost;
                    index++;
                }
            }
        }

        public string GetStatusText(BattleCharacter bcharacter)
        {
            string txt = "";
            foreach (CardStatus astatus in bcharacter.GetAllStatus())
            {
                StatusData istats = StatusData.Get(astatus.id);
                if (istats != null && !string.IsNullOrEmpty(istats.title))
                {
                    string sval = astatus.value > 1 ? " " + astatus.value : "";
                    txt += istats.GetTitle() + sval + ", ";
                }
            }
            if (txt.Length > 2)
                txt = txt.Substring(0, txt.Length - 2);
            return txt;
        }

        public void Hide()
        {
            panel.Hide();
        }

        public static CharacterInfoUI Get()
        {
            return instance;
        }
    }
}
