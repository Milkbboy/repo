using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace RogueEngine.UI
{
    /// <summary>
    /// Main UI script for all the battle scene UI
    /// </summary>

    public class BattleUI : MonoBehaviour
    {
        public Canvas game_canvas;
        public Canvas panel_canvas;
        public Canvas top_canvas;
        public UIPanel menu_panel;
        public Text quit_btn;

        public Text mana_txt;
        public Button end_turn_button;

        public GameObject deck_group;
        public GameObject discard_group;
        public Text deck_value;
        public Text discard_value;

        private float end_turn_timer = 0f;
        private float selector_timer = 0f;

        private static BattleUI _instance;

        void Awake()
        {
            _instance = this;
            mana_txt.text = "";
        }

        private void Start()
        {
            GameClient.Get().onNewTurn += OnNewTurn;
            BlackPanel.Get().Show(true);
            BlackPanel.Get().Hide();
        }

        private void OnDestroy()
        {
            if (GameClient.Get() != null)
            {
                GameClient.Get().onNewTurn -= OnNewTurn;
            }
        }

        void Update()
        {
            Battle data = GameClient.Get().GetBattle();
			bool is_connecting = data == null || data.phase == BattlePhase.None;
            bool connection_lost = !is_connecting && !GameClient.Get().IsReady();
            LoadingPanel.Get().SetVisible(connection_lost);

            if (!GameClient.Get().IsBattleReady())
                return;

            //Menu
            if (Input.GetKeyDown(KeyCode.Escape))
                menu_panel.Toggle();

            if (!GameClient.Get().IsReady())
                return;

            bool yourturn = GameClient.Get().IsYourTurn();
            BattleCharacter champion = data.GetActiveCharacter();
            mana_txt.text = champion != null ? champion.mana.ToString() : "";
            end_turn_button.interactable = yourturn && end_turn_timer > 1f;
            end_turn_timer += Time.deltaTime;
            selector_timer += Time.deltaTime;

            bool show_deck = champion != null && !champion.IsEnemy();
            bool show_discard = champion != null && !champion.IsEnemy();
            if (show_deck != deck_group.activeSelf)
                deck_group.SetActive(show_deck);
            if (show_discard != discard_group.activeSelf)
                discard_group.SetActive(show_discard);

            if (champion != null)
            {
                deck_value.text = champion.cards_deck.Count.ToString();
                discard_value.text = champion.cards_discard.Count.ToString();
            }

            //Simulate timer
            if (data.phase == BattlePhase.Main && data.turn_timer > 0f)
                data.turn_timer -= Time.deltaTime;

            //Show selector panels
            foreach (SelectorPanel panel in SelectorPanel.GetAll())
            {
                bool should_show = panel.ShouldShow();
                if (should_show != panel.IsVisible() && selector_timer > 1f)
                {
                    selector_timer = 0f;
                    panel.SetVisible(should_show);

                    if (should_show)
                    {
                        AbilityData ability = AbilityData.Get(data.selector_ability_id);
                        BattleCharacter caster = data.GetCharacter(data.selector_caster_uid);
                        Card card = data.GetCard(data.selector_card_uid);
                        panel.Show(ability, caster, card);
                    }
                }
            }

            //Hide
            if (!yourturn)
            {
                SelectorPanel.HideAll();
            }

        }

        private void PulseFX()
        {
            //timeout_animator?.SetTrigger("pulse");
            //AudioTool.Get().PlaySFX("time", timeout_audio, 1f);
        }

        private void OnNewTurn()
        {
            CardSelector.Get().Hide();
            SelectTargetUI.Get().Hide();
        }

        public void OnClickNextTurn()
        {
            GameClient.Get().EndTurn();
            end_turn_timer = 0f; //Disable button immediately (dont wait for refresh)
        }

        public void OnClickDeck()
        {
            Battle data = GameClient.Get().GetBattle();
            BattleCharacter champion = data.GetActiveCharacter();
            if (champion != null)
            {
                CardSelector.Get().Show(champion.cards_deck, "Deck", true);
            }
        }

        public void OnClickDiscard()
        {
            Battle data = GameClient.Get().GetBattle();
            BattleCharacter champion = data.GetActiveCharacter();
            if (champion != null)
            {
                CardSelector.Get().Show(champion.cards_discard, "Discard");
            }
        }

        public void OnClickRestart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnClickMenu()
        {
            menu_panel.Show();
        }

        public void OnClickBack()
        {
            menu_panel.Hide();
        }

        public void OnClickEscape()
        {
            World world = GameClient.Get().GetWorld();
            EventBattle battle = EventBattle.Get(world.battle.battle_id);
            if (battle != null)
            {
                GameClient.Get().Resign();
                menu_panel.Hide();
            }
        }

        public void OnClickQuit()
        {
            StartCoroutine(QuitRoutine("Menu"));
            menu_panel.Hide();
        }

        private IEnumerator QuitRoutine(string scene)
        {
            BlackPanel.Get().Show();
            AudioTool.Get().FadeOutMusic("music");
            AudioTool.Get().FadeOutSFX("ambience");
            AudioTool.Get().FadeOutSFX("ending_sfx");

            yield return new WaitForSeconds(1f);

            GameClient.Get().Disconnect();
            SceneNav.GoTo(scene);
        }

        public static bool IsUIOpened()
        {
            return CardSelector.Get().IsVisible() || EndGamePanel.Get().IsVisible();
        }

        public static bool IsOverUI()
        {
            //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        public static bool IsOverUILayer(string sorting_layer)
        {
            return IsOverUILayer(SortingLayer.NameToID(sorting_layer));
        }

        public static bool IsOverUILayer(int sorting_layer)
        {
            //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            int count = 0;
            foreach (RaycastResult result in results)
            {
                if (result.sortingLayer == sorting_layer)
                    count++;
            }
            return count > 0;
        }

        public static bool IsOverRectTransform(Canvas canvas, RectTransform rect)
        {
            PointerEventData pevent = new PointerEventData(EventSystem.current);
            pevent.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            raycaster.Raycast(pevent, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.transform == rect || result.gameObject.transform.IsChildOf(rect))
                    return true;
            }
            return false;
        }

        public static Vector2 ScreenToRectPos(Canvas canvas, RectTransform rect, Vector2 screen_pos)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
            {
                Vector2 anchor_pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screen_pos, canvas.worldCamera, out anchor_pos);
                return anchor_pos;
            }
            else
            {
                Vector2 anchor_pos = screen_pos - new Vector2(rect.position.x, rect.position.y);
                anchor_pos = new Vector2(anchor_pos.x / rect.lossyScale.x, anchor_pos.y / rect.lossyScale.y);
                return anchor_pos;
            }
        }

        public static Vector3 MouseToWorld(Vector2 mouse_pos, float distance = 10f)
        {
            Camera cam = GameCamera.Get() != null ? GameCamera.GetCamera() : Camera.main;
            Vector3 wpos = cam.ScreenToWorldPoint(new Vector3(mouse_pos.x, mouse_pos.y, distance));
            return wpos;
        }

        public static string FormatNumber(int value)
        {
            return string.Format("{0:#,0}", value);
        }

        public static BattleUI Get()
        {
            return _instance;
        }
    }
}
