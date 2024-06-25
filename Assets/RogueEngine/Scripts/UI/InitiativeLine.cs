using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using UnityEngine.EventSystems;

namespace RogueEngine.UI
{
    /// <summary>
    /// One of the squares in the history bar
    /// </summary>

    public class InitiativeLine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image bg_img;
        public Image card_img;
        public Text speed;

        public Sprite bg_ally;
        public Sprite bg_enemy;

        private BattleCharacter character;
        private RectTransform rect;
        private float timer = 0f;
        private bool is_hover = false;

        private static List<InitiativeLine> line_list = new List<InitiativeLine>();

        void Awake()
        {
            line_list.Add(this);
            rect = GetComponent<RectTransform>();
        }

        void OnDestroy()
        {
            line_list.Add(this);
        }

        void Start()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            timer += Time.deltaTime;
        }

        public void SetLine(BattleCharacter character)
        {
            this.character = character;

            if (character.is_champion)
                card_img.sprite = character.ChampionData.GetPortraitArt();
            else
                card_img.sprite = character.CharacterData.GetPortraitArt();

            if (speed != null)
                speed.text = character.GetSpeed().ToString();

            bg_img.sprite = character.IsEnemy() ? bg_enemy : bg_ally;
            gameObject.SetActive(true);
            timer = 0f;
        }

        public void Hide()
        {
            character = null;
            if (timer > 0.05f)
                gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            timer = 0f;
            is_hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            timer = 0f;
            is_hover = false;
        }

        void OnDisable()
        {
            is_hover = false;
        }

        public RectTransform RectTransform
        {
            get { return rect; }
        }

        public static BattleCharacter GetHoverCharacter()
        {
            foreach (InitiativeLine line in line_list)
            {
                if (line.character != null && line.is_hover)
                    return line.character;
            }
            return null;
        }
    }
}
