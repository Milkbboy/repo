using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ERang.Data;

namespace ERang
{
    public class BoardCardEditorWindow : EditorWindow
    {
        private BoardSystem boardSystem;
        private Vector2 scrollPosition;
        private int[] cardDataIds;
        private string[] cardDataNames;
        private int[] masterCardDataIds;
        private string[] masterCardDataNames;
        private const int elementWidth = 200;

        [MenuItem("ERang/Board Card Editor")]
        public static void ShowWindow()
        {
            GetWindow<BoardCardEditorWindow>("Base Editor");
        }

        private void OnEnable()
        {
            InitializeEditor();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                InitializeEditor();
                Repaint();
            }
        }

        private void InitializeEditor()
        {
            boardSystem = FindObjectOfType<BoardSystem>();

            List<(int, string)> cardIdNames = CardData.GetCardIdNames();
            List<(int, string)> masterCardIdNames = MasterData.GetCardIdNames();

            cardDataIds = new int[cardIdNames.Count];
            cardDataNames = new string[cardIdNames.Count];
            masterCardDataIds = new int[masterCardIdNames.Count];
            masterCardDataNames = new string[masterCardIdNames.Count];

            for (int i = 0; i < cardIdNames.Count; i++)
            {
                cardDataIds[i] = cardIdNames[i].Item1;
                cardDataNames[i] = cardIdNames[i].Item2;
            }

            for (int i = 0; i < masterCardIdNames.Count; i++)
            {
                masterCardDataIds[i] = masterCardIdNames[i].Item1;
                masterCardDataNames[i] = masterCardIdNames[i].Item2;
            }
        }

        private void OnGUI()
        {
            if (boardSystem == null)
            {
                EditorGUILayout.HelpBox("BoardSystem을 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("보드 카드 에디터", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginHorizontal();
            DrawBoardCards(boardSystem.AllSlots);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            // 변경 사항을 적용합니다
            if (GUILayout.Button("Apply Changes"))
            {
                EditorUtility.SetDirty(boardSystem);
                boardSystem.RefreshBoardSlot();

                foreach (BSlot bSlot in boardSystem.AllSlots)
                {
                    if (bSlot.Card == null)
                        continue;

                    Debug.Log($"SlotNum: {bSlot.SlotNum}, {string.Join(", ", bSlot.Card.AbilitySystem.CardAbilities.Select(ability => ability.abilityId))}");
                }
            }
        }

        private void DrawBoardCards(List<BSlot> bSlots)
        {
            GUIStyle redTextStyle = new();
            redTextStyle.normal.textColor = new Color(244 / 255f, 100 / 255f, 81 / 255f);

            GUIStyle statStyle = new GUIStyle(GUI.skin.label);
            statStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f)); // 반투명 검정 배경
            statStyle.normal.textColor = Color.white;

            foreach (BSlot bSlot in bSlots)
            {
                EditorGUILayout.BeginVertical("box", GUILayout.Width(elementWidth));
                EditorGUILayout.LabelField($"SlotNum: {bSlot.SlotNum}");
                EditorGUILayout.LabelField($"Index: {bSlot.Index}");
                EditorGUILayout.LabelField($"IsOverlapCard: {bSlot.IsOverlapCard}");
                EditorGUILayout.LabelField("SlotCardType");
                bSlot.SlotCardType = (CardType)EditorGUILayout.EnumPopup(bSlot.SlotCardType);

                BaseCard card = bSlot.Card;

                // CardData 선택 드롭다운 메뉴 추가
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Card Data");
                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    bSlot.RemoveCard();
                }
                EditorGUILayout.EndHorizontal();

                int[] ids = bSlot.SlotCardType switch
                {
                    CardType.Master => masterCardDataIds,
                    CardType.Creature => cardDataIds,
                    CardType.Monster => cardDataIds,
                    _ => cardDataIds,
                };

                string[] names = bSlot.SlotCardType switch
                {
                    CardType.Master => masterCardDataNames,
                    CardType.Creature => cardDataNames,
                    CardType.Monster => cardDataNames,
                    _ => cardDataNames,
                };

                int selectedIndex = System.Array.IndexOf(ids, card?.Id ?? -1);
                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, names);

                if (newSelectedIndex != selectedIndex)
                {
                    int cardId = bSlot.SlotCardType switch
                    {
                        CardType.Master => masterCardDataIds[newSelectedIndex],
                        CardType.Creature => cardDataIds[newSelectedIndex],
                        CardType.Monster => cardDataIds[newSelectedIndex],
                        _ => cardDataIds[newSelectedIndex],
                    };

                    bool inUse = false;
                    Texture2D cardTexture = null;

                    switch (bSlot.SlotCardType)
                    {
                        case CardType.Master:
                            MasterData selectedMasterCardData = MasterData.GetMasterData(cardId);
                            inUse = false;
                            cardTexture = selectedMasterCardData.GetMasterTexture();
                            card ??= new MasterCard();
                            break;
                        case CardType.Monster:
                            CardData selectedMonsterCardData = CardData.GetCardData(cardId);
                            inUse = selectedMonsterCardData.inUse;
                            cardTexture = selectedMonsterCardData.GetCardTexture();
                            card ??= new CreatureCard(selectedMonsterCardData);
                            break;
                        case CardType.Creature:
                            CardData selectedCardData = CardData.GetCardData(cardId);
                            inUse = selectedCardData.inUse;
                            cardTexture = selectedCardData.GetCardTexture();
                            card ??= new CreatureCard(selectedCardData);
                            break;
                    }

                    card.UpdateCardData(cardId, bSlot.SlotCardType, inUse, 0, cardTexture);
                    bSlot.EquipCard(card);
                }

                if (card != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    // 카드 이미지 표시
                    if (bSlot.Card.CardImage != null)
                        GUILayout.Label(bSlot.Card.CardImage, GUILayout.Width(elementWidth), GUILayout.Height(150));
                    else
                        GUILayout.Label("No Image", GUILayout.Width(elementWidth), GUILayout.Height(150));

                    // 카드 이미지 위에 스탯 표시
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    GUI.Label(new Rect(lastRect.x, lastRect.y, 50, 20), $"Mana: {card.Mana}", statStyle);
                    GUI.Label(new Rect(lastRect.xMax - 100, lastRect.y, 50, 20), $"Atk: {card.Atk}", statStyle);
                    GUI.Label(new Rect(lastRect.xMax - 100, lastRect.y + 20, 50, 20), $"Def: {card.Def}", statStyle);
                    GUI.Label(new Rect(lastRect.xMax - 100, lastRect.y + 40, 50, 20), $"Hp: {card.Hp}", statStyle);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    // 카드 ability 표시
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Card Abilities");

                    // 카드 ability 추가
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        AbilitySelectorWindow.ShowWindow(bSlot, (selectedAbilityId) =>
                        {
                            Debug.Log($"Add Ability: {selectedAbilityId}");

                            // AbilityLogic.Instance.AbilityAction(selectedAbilityId, bSlot);
                            Repaint();
                        });
                    }
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < card.AbilitySystem.CardAbilities.Count; ++i)
                    {
                        CardAbility cardAbility = card.AbilitySystem.CardAbilities[i];
                        AbilityData abilityData = AbilityData.GetAbilityData(cardAbility.abilityId);

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            // AbilityLogic.Instance.ReleaseAction(bSlot, cardAbility);
                            i--;
                        }
                        EditorGUILayout.LabelField($"[{cardAbility.abilityId}] {abilityData.workType}: {AbilityData.GetAbilityData(abilityData.abilityId).abilityType}", GUILayout.Width(elementWidth));
                        // duration을 빨간색으로 표시
                        GUILayout.Label($"{cardAbility.duration}", redTextStyle);
                        EditorGUILayout.EndHorizontal();

                        // 마우스 오버 시 툴팁 표시
                        Rect lastRectAbility = GUILayoutUtility.GetLastRect();
                        if (lastRectAbility.Contains(Event.current.mousePosition))
                        {
                            Vector2 mousePosition = Event.current.mousePosition;
                            GUIStyle tooltipStyle = new(GUI.skin.box);
                            tooltipStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f)); // 검정 배경
                            tooltipStyle.normal.textColor = Color.white;
                            tooltipStyle.padding = new RectOffset(10, 10, 10, 10);

                            string tooltipText = $"{abilityData.nameDesc} {cardAbility.duration} 턴 남음";

                            Vector2 size = tooltipStyle.CalcSize(new GUIContent(tooltipText));
                            Rect tooltipRect = new Rect(mousePosition.x, mousePosition.y - 10, size.x, size.y);

                            GUI.Box(tooltipRect, tooltipText, tooltipStyle);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}