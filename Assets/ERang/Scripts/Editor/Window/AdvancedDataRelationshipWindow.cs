using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;
using ERang.Table;

namespace ERang
{
    public class AdvancedDataRelationshipWindow : EditorWindow
    {
        [System.Serializable]
        public class CardRelationshipInfo
        {
            public CardData cardData;
            public List<AiGroupData> aiGroupDatas = new();
            public List<AiData> aiDatas = new();
            public List<AbilityData> abilityDatas = new();
        }

        // 변경 감지 시스템
        [System.Serializable]
        public class ChangeTracker
        {
            public Dictionary<string, object> originalValues = new();
            public Dictionary<string, object> currentValues = new();

            public bool HasChanges => !DictionariesEqual(originalValues, currentValues);

            public void RecordOriginal(string key, object value)
            {
                originalValues[key] = value;
                currentValues[key] = value;
            }

            public void RecordChange(string key, object value)
            {
                currentValues[key] = value;
            }

            public void Reset()
            {
                originalValues.Clear();
                currentValues.Clear();
            }

            private bool DictionariesEqual(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
            {
                if (dict1.Count != dict2.Count) return false;

                foreach (var kvp in dict1)
                {
                    if (!dict2.ContainsKey(kvp.Key) || !Equals(dict2[kvp.Key], kvp.Value))
                        return false;
                }
                return true;
            }
        }

        // UI 상태
        private Vector2 scrollPosition;
        private int selectedCardIndex = 0;
        private string[] cardNames;
        private List<CardData> allCards;
        private CardRelationshipInfo currentRelationship;
        private bool isDataLoaded = false;
        private string searchString = "";
        private List<CardData> filteredCards;

        // 편집 모드
        private bool isEditMode = false;
        private ChangeTracker changeTracker = new ChangeTracker();

        // 연결 필드 편집용 데이터
        private Dictionary<string, bool> connectionEditStates = new Dictionary<string, bool>();
        private Dictionary<string, int> selectedDropdownIndices = new Dictionary<string, int>();

        // GUI 스타일
        private GUIStyle headerStyle;
        private GUIStyle cardStyle;
        private GUIStyle groupStyle;
        private GUIStyle aiStyle;
        private GUIStyle abilityStyle;
        private GUIStyle valueStyle;
        private GUIStyle editModeStyle;
        private GUIStyle saveButtonStyle;

        [MenuItem("ERang/Tools/Advanced Data Relationship Analyzer")]
        public static void ShowWindow()
        {
            AdvancedDataRelationshipWindow window = GetWindow<AdvancedDataRelationshipWindow>("Advanced Data Analyzer");
            window.minSize = new Vector2(700, 500);
        }

        private void OnEnable()
        {
            LoadAllData();
        }

        private void InitializeStyles()
        {
            if (headerStyle != null) return; // 이미 초기화됨

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                normal = { textColor = Color.white }
            };

            cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = MakeTexture(1, 1, new Color(0.3f, 0.6f, 0.9f, 0.3f)) }
            };

            groupStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = MakeTexture(1, 1, new Color(0.6f, 0.9f, 0.3f, 0.3f)) }
            };

            aiStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = MakeTexture(1, 1, new Color(0.9f, 0.6f, 0.3f, 0.3f)) }
            };

            abilityStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal = { background = MakeTexture(1, 1, new Color(0.9f, 0.3f, 0.6f, 0.3f)) }
            };

            valueStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.red },
                fontSize = 12
            };

            editModeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.yellow },
                fontSize = 14
            };

            saveButtonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold
            };
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void LoadAllData()
        {
            try
            {
                CardData.Load("TableExports/CardDataTable");
                AiGroupData.Load("TableExports/AiGroupDataTable");
                AiData.Load("TableExports/AiDataTable");
                AbilityData.Load("TableExports/AbilityDataTable");

                allCards = CardData.card_list ?? new List<CardData>();
                filteredCards = new List<CardData>(allCards);

                if (allCards != null && allCards.Count > 0)
                {
                    UpdateCardNames();
                    isDataLoaded = true;
                    AnalyzeCardRelationship(allCards[0]);
                }
                else
                {
                    Debug.LogError("Card data could not be loaded. Please check if the data tables exist.");
                    isDataLoaded = false;
                    // 빈 배열로 초기화
                    allCards = new List<CardData>();
                    filteredCards = new List<CardData>();
                    cardNames = new string[0];
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load data: {e.Message}");
                isDataLoaded = false;
                // 안전한 초기화
                allCards = new List<CardData>();
                filteredCards = new List<CardData>();
                cardNames = new string[0];
            }
        }

        private void UpdateCardNames()
        {
            try
            {
                if (filteredCards != null && filteredCards.Count > 0)
                {
                    cardNames = filteredCards.Select(card => $"[{card?.card_id ?? 0}] {card?.nameDesc ?? "Unknown"}").ToArray();
                }
                else
                {
                    cardNames = new string[0];
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating card names: {e.Message}");
                cardNames = new string[0];
            }
        }

        private void OnGUI()
        {
            // 스타일 초기화 (OnGUI에서만 가능)
            InitializeStyles();

            if (!isDataLoaded)
            {
                EditorGUILayout.HelpBox("Data is not loaded. Please make sure all data tables exist in Resources/TableExports/", MessageType.Error);
                if (GUILayout.Button("Reload Data"))
                {
                    LoadAllData();
                }
                return;
            }

            DrawHeader();
            DrawCardSelectionArea();

            if (currentRelationship != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawRelationshipTree();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawHeader()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("고급 데이터 관계 분석기", headerStyle ?? EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                // Edit Mode 토글
                EditorGUI.BeginChangeCheck();
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = isEditMode ? Color.yellow : Color.white;
                isEditMode = GUILayout.Toggle(isEditMode, isEditMode ? "🔧 Edit Mode" : "👁️ View Mode",
                    EditorStyles.miniButton, GUILayout.Width(100));
                GUI.backgroundColor = originalColor;

                if (EditorGUI.EndChangeCheck())
                {
                    if (isEditMode)
                    {
                        InitializeEditMode();
                    }
                    else
                    {
                        ExitEditMode();
                    }
                }

                // 변경사항 표시
                if (isEditMode && changeTracker.HasChanges)
                {
                    EditorGUILayout.LabelField("🟡 Changes Pending", editModeStyle ?? EditorStyles.boldLabel, GUILayout.Width(120));
                }

                EditorGUILayout.EndHorizontal();
            }
            catch (System.Exception e)
            {
                EditorGUILayout.LabelField($"Header Error: {e.Message}");
            }

            EditorGUILayout.Space();
        }

        private void InitializeEditMode()
        {
            changeTracker.Reset();
            RecordOriginalValues();
            Debug.Log("Edit Mode activated - Original values recorded");
        }

        private void ExitEditMode()
        {
            if (changeTracker.HasChanges)
            {
                bool saveChanges = EditorUtility.DisplayDialog("Unsaved Changes",
                    "You have unsaved changes. Do you want to save them?",
                    "Save", "Discard");

                if (saveChanges)
                {
                    SaveAllChanges();
                }
                else
                {
                    DiscardDataChanges();
                }
            }

            changeTracker.Reset();
            Debug.Log("Edit Mode deactivated");
        }

        private void RecordOriginalValues()
        {
            try
            {
                if (currentRelationship?.cardData != null)
                {
                    var card = currentRelationship.cardData;
                    changeTracker.RecordOriginal($"card_{card.card_id}_hp", card.hp);
                    changeTracker.RecordOriginal($"card_{card.card_id}_atk", card.atk);
                    changeTracker.RecordOriginal($"card_{card.card_id}_def", card.def);
                    changeTracker.RecordOriginal($"card_{card.card_id}_costMana", card.costMana);
                    changeTracker.RecordOriginal($"card_{card.card_id}_costGold", card.costGold);
                    changeTracker.RecordOriginal($"card_{card.card_id}_aiGroup_ids", string.Join(",", card.aiGroup_ids));
                }

                if (currentRelationship?.aiDatas != null)
                {
                    foreach (var aiData in currentRelationship.aiDatas)
                    {
                        if (aiData != null)
                        {
                            changeTracker.RecordOriginal($"ai_{aiData.ai_Id}_atk_Cnt", aiData.atk_Cnt);
                            changeTracker.RecordOriginal($"ai_{aiData.ai_Id}_atk_Interval", aiData.atk_Interval);
                            changeTracker.RecordOriginal($"ai_{aiData.ai_Id}_value", aiData.value);
                            changeTracker.RecordOriginal($"ai_{aiData.ai_Id}_ability_Ids", string.Join(",", aiData.ability_Ids));
                        }
                    }
                }

                if (currentRelationship?.abilityDatas != null)
                {
                    foreach (var abilityData in currentRelationship.abilityDatas)
                    {
                        if (abilityData != null)
                        {
                            changeTracker.RecordOriginal($"ability_{abilityData.abilityId}_value", abilityData.value);
                            changeTracker.RecordOriginal($"ability_{abilityData.abilityId}_ratio", abilityData.ratio);
                            changeTracker.RecordOriginal($"ability_{abilityData.abilityId}_duration", abilityData.duration);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error recording original values: {e.Message}");
            }
        }

        private void DrawCardSelectionArea()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();

                // 검색 필드
                EditorGUI.BeginChangeCheck();
                string newSearchString = EditorGUILayout.TextField("Search Cards:", searchString ?? "");
                if (EditorGUI.EndChangeCheck())
                {
                    searchString = newSearchString;
                    FilterCards();
                }

                // 새로고침 버튼
                if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                {
                    LoadAllData();
                }

                EditorGUILayout.EndHorizontal();
            }
            catch (System.Exception e)
            {
                EditorGUILayout.LabelField($"Search Error: {e.Message}");
            }

            if (filteredCards != null && filteredCards.Count > 0)
            {
                try
                {
                    // 카드 선택 드롭다운
                    EditorGUI.BeginChangeCheck();
                    selectedCardIndex = EditorGUILayout.Popup("Select Card:", selectedCardIndex, cardNames ?? new string[0]);
                    if (EditorGUI.EndChangeCheck() && selectedCardIndex < filteredCards.Count)
                    {
                        if (isEditMode && changeTracker.HasChanges)
                        {
                            bool proceed = EditorUtility.DisplayDialog("Unsaved Changes",
                                "You have unsaved changes. Changing cards will discard them. Continue?",
                                "Yes", "No");

                            if (!proceed)
                            {
                                return;
                            }
                        }

                        AnalyzeCardRelationship(filteredCards[selectedCardIndex]);
                        if (isEditMode)
                        {
                            InitializeEditMode();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Card Selection Error: {e.Message}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No cards found matching the search criteria.", MessageType.Info);
            }
        }

        private void FilterCards()
        {
            if (string.IsNullOrEmpty(searchString))
            {
                filteredCards = new List<CardData>(allCards);
            }
            else
            {
                filteredCards = allCards.Where(card =>
                    card.nameDesc.ToLower().Contains(searchString.ToLower()) ||
                    card.card_id.ToString().Contains(searchString)
                ).ToList();
            }

            UpdateCardNames();
            selectedCardIndex = 0;

            if (filteredCards.Count > 0)
            {
                AnalyzeCardRelationship(filteredCards[0]);
                if (isEditMode)
                {
                    InitializeEditMode();
                }
            }
        }

        private void AnalyzeCardRelationship(CardData selectedCard)
        {
            try
            {
                if (selectedCard == null)
                {
                    currentRelationship = null;
                    return;
                }

                currentRelationship = new CardRelationshipInfo { cardData = selectedCard };

                // AiGroup 데이터 수집
                if (selectedCard.aiGroup_ids != null)
                {
                    foreach (int aiGroupId in selectedCard.aiGroup_ids)
                    {
                        AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);
                        if (aiGroupData != null)
                        {
                            currentRelationship.aiGroupDatas.Add(aiGroupData);

                            // AiData 수집
                            if (aiGroupData.ai_Groups != null)
                            {
                                foreach (var aiGroup in aiGroupData.ai_Groups)
                                {
                                    if (aiGroup != null)
                                    {
                                        foreach (int aiId in aiGroup)
                                        {
                                            AiData aiData = AiData.GetAiData(aiId);
                                            if (aiData != null && !currentRelationship.aiDatas.Contains(aiData))
                                            {
                                                currentRelationship.aiDatas.Add(aiData);

                                                // AbilityData 수집
                                                if (aiData.ability_Ids != null)
                                                {
                                                    foreach (int abilityId in aiData.ability_Ids)
                                                    {
                                                        AbilityData abilityData = AbilityData.GetAbilityData(abilityId);
                                                        if (abilityData != null && !currentRelationship.abilityDatas.Contains(abilityData))
                                                        {
                                                            currentRelationship.abilityDatas.Add(abilityData);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error analyzing card relationship: {e.Message}");
                currentRelationship = null;
            }
        }

        private void DrawRelationshipTree()
        {
            if (currentRelationship?.cardData == null) return;

            try
            {
                // 카드 정보
                EditorGUILayout.BeginVertical(cardStyle ?? EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"🃏 CARD: [{currentRelationship.cardData.card_id}] {currentRelationship.cardData.nameDesc}", headerStyle ?? EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                // Card Save 버튼
                EditorGUI.BeginDisabledGroup(!isEditMode || !HasCardChanges());
                Color originalBg = GUI.backgroundColor;
                GUI.backgroundColor = HasCardChanges() ? Color.green : Color.gray;
                if (GUILayout.Button("💾 Save Card", GUILayout.Width(100)))
                {
                    SaveCardData();
                }
                GUI.backgroundColor = originalBg;
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                if (isEditMode)
                {
                    DrawEditableCardDetails(currentRelationship.cardData);
                }
                else
                {
                    DrawCardDetails(currentRelationship.cardData);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                // AI Data 정보
                if (currentRelationship.aiDatas.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"⚔️ Connected AI Actions ({currentRelationship.aiDatas.Count})", headerStyle);
                    GUILayout.FlexibleSpace();

                    // AI Save 버튼
                    EditorGUI.BeginDisabledGroup(!isEditMode || !HasAiChanges());
                    GUI.backgroundColor = HasAiChanges() ? Color.green : Color.gray;
                    if (GUILayout.Button("💾 Save AI", GUILayout.Width(100)))
                    {
                        SaveAiData();
                    }
                    GUI.backgroundColor = originalBg;
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndHorizontal();

                    foreach (var aiData in currentRelationship.aiDatas)
                    {
                        EditorGUILayout.BeginVertical(aiStyle);
                        EditorGUILayout.LabelField($"  🎯 AI [{aiData.ai_Id}] {aiData.name} - {aiData.type}");

                        if (isEditMode)
                        {
                            DrawEditableAiDataDetails(aiData);
                        }
                        else
                        {
                            DrawAiDataDetails(aiData);
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                }

                EditorGUILayout.Space();

                // Ability 정보
                if (currentRelationship.abilityDatas.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"✨ FINAL ABILITY VALUES ({currentRelationship.abilityDatas.Count})", headerStyle);
                    GUILayout.FlexibleSpace();

                    // Ability Save 버튼
                    EditorGUI.BeginDisabledGroup(!isEditMode || !HasAbilityChanges());
                    GUI.backgroundColor = HasAbilityChanges() ? Color.green : Color.gray;
                    if (GUILayout.Button("💾 Save Abilities", GUILayout.Width(120)))
                    {
                        SaveAbilityData();
                    }
                    GUI.backgroundColor = originalBg;
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical(abilityStyle);
                    foreach (var abilityData in currentRelationship.abilityDatas)
                    {
                        if (isEditMode)
                        {
                            DrawEditableAbilityDetails(abilityData);
                        }
                        else
                        {
                            DrawAbilityDetails(abilityData);
                        }
                        EditorGUILayout.Space(3);
                    }
                    EditorGUILayout.EndVertical();
                }

                // Save All 버튼
                if (isEditMode)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginDisabledGroup(!changeTracker.HasChanges);
                    GUI.backgroundColor = changeTracker.HasChanges ? Color.red : Color.gray;
                    if (GUILayout.Button("💾 SAVE ALL CHANGES", GUILayout.Width(200), GUILayout.Height(30)))
                    {
                        SaveAllChanges();
                    }
                    GUI.backgroundColor = originalBg;
                    EditorGUI.EndDisabledGroup();

                    if (changeTracker.HasChanges)
                    {
                        if (GUILayout.Button("🔄 Discard Changes", GUILayout.Width(120)))
                        {
                            DiscardDataChanges();
                        }
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }

                // 요약 정보
                EditorGUILayout.Space();
                DrawSummary();
            }
            catch (System.Exception e)
            {
                EditorGUILayout.LabelField($"Error drawing relationship tree: {e.Message}");
            }
        }

        // 편집 가능한 카드 상세 정보
        private void DrawEditableCardDetails(CardData card)
        {
            EditorGUILayout.LabelField($"Type: {card.cardType} | Grade: {card.cardGrade}");

            // HP, ATK, DEF 편집
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stats:", GUILayout.Width(50));

            EditorGUI.BeginChangeCheck();
            int newHp = EditorGUILayout.IntField("HP", card.hp, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                card.hp = Mathf.Max(0, newHp);
                changeTracker.RecordChange($"card_{card.card_id}_hp", card.hp);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            int newAtk = EditorGUILayout.IntField("ATK", card.atk, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                card.atk = Mathf.Max(0, newAtk);
                changeTracker.RecordChange($"card_{card.card_id}_atk", card.atk);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            int newDef = EditorGUILayout.IntField("DEF", card.def, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                card.def = Mathf.Max(0, newDef);
                changeTracker.RecordChange($"card_{card.card_id}_def", card.def);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            // Mana, Gold Cost 편집
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cost:", GUILayout.Width(50));

            EditorGUI.BeginChangeCheck();
            int newMana = EditorGUILayout.IntField("Mana", card.costMana, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                card.costMana = Mathf.Max(0, newMana);
                changeTracker.RecordChange($"card_{card.card_id}_costMana", card.costMana);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            int newGold = EditorGUILayout.IntField("Gold", card.costGold, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                card.costGold = Mathf.Max(0, newGold);
                changeTracker.RecordChange($"card_{card.card_id}_costGold", card.costGold);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"AI Groups: [{string.Join(", ", card.aiGroup_ids)}]");

            // 연결 필드 편집 - AI Groups
            if (isEditMode)
            {
                var availableAiGroupIds = GetAvailableAiGroupIds();
                DrawConnectionFieldEditor("AiGroup", card.aiGroup_ids, availableAiGroupIds,
                    (newIds) =>
                    {
                        card.aiGroup_ids = newIds;
                        // 카드 관계 재분석
                        AnalyzeCardRelationship(card);
                    });
            }
        }

        // 편집 가능한 AI 데이터 상세 정보
        private void DrawEditableAiDataDetails(AiData ai)
        {
            EditorGUILayout.LabelField($"    Target: {ai.target} | Attack Type: {ai.attackType}");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("    ", GUILayout.Width(20));

            EditorGUI.BeginChangeCheck();
            int newAtkCnt = EditorGUILayout.IntField("Count", ai.atk_Cnt, GUILayout.MinWidth(100));
            if (EditorGUI.EndChangeCheck())
            {
                ai.atk_Cnt = Mathf.Max(1, newAtkCnt);
                changeTracker.RecordChange($"ai_{ai.ai_Id}_atk_Cnt", ai.atk_Cnt);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            float newInterval = EditorGUILayout.FloatField("Interval", ai.atk_Interval, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                ai.atk_Interval = Mathf.Max(0f, newInterval);
                changeTracker.RecordChange($"ai_{ai.ai_Id}_atk_Interval", ai.atk_Interval);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUILayout.IntField("Value", ai.value, GUILayout.MinWidth(100));
            if (EditorGUI.EndChangeCheck())
            {
                ai.value = newValue;
                changeTracker.RecordChange($"ai_{ai.ai_Id}_value", ai.value);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"    Ranges: [{string.Join(", ", ai.attackRanges)}]");
            EditorGUILayout.LabelField($"    Abilities: [{string.Join(", ", ai.ability_Ids)}]");

            // 연결 필드 편집 - Abilities
            if (isEditMode)
            {
                var availableAbilityIds = GetAvailableAbilityIds();
                DrawConnectionFieldEditor("Ability", ai.ability_Ids, availableAbilityIds,
                    (newIds) =>
                    {
                        ai.ability_Ids = newIds;
                        // AI 관계 재분석
                        if (currentRelationship?.cardData != null)
                        {
                            AnalyzeCardRelationship(currentRelationship.cardData);
                        }
                    });
            }
        }

        // 편집 가능한 어빌리티 상세 정보
        private void DrawEditableAbilityDetails(AbilityData ability)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"🔥 [{ability.abilityId}] {ability.nameDesc}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Type: {ability.abilityType}", GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("    ", GUILayout.Width(20));

            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUILayout.IntField("VALUE", ability.value, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                ability.value = newValue;
                changeTracker.RecordChange($"ability_{ability.abilityId}_value", ability.value);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            float newRatio = EditorGUILayout.FloatField("RATIO", ability.ratio, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                ability.ratio = Mathf.Max(0f, newRatio);
                changeTracker.RecordChange($"ability_{ability.abilityId}_ratio", ability.ratio);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            int newDuration = EditorGUILayout.IntField("DURATION", ability.duration, GUILayout.MinWidth(120));
            if (EditorGUI.EndChangeCheck())
            {
                ability.duration = Mathf.Max(0, newDuration);
                changeTracker.RecordChange($"ability_{ability.abilityId}_duration", ability.duration);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(ability.skillIcon))
            {
                EditorGUILayout.LabelField($"    Icon: {ability.skillIcon}");
            }
        }

        // 기존 읽기 전용 메서드들
        private void DrawCardDetails(CardData card)
        {
            EditorGUILayout.LabelField($"Type: {card.cardType} | Grade: {card.cardGrade}");
            EditorGUILayout.LabelField($"Cost: {card.costMana} Mana, {card.costGold} Gold");
            EditorGUILayout.LabelField($"Stats: HP {card.hp} | ATK {card.atk} | DEF {card.def}");
            EditorGUILayout.LabelField($"AI Groups: [{string.Join(", ", card.aiGroup_ids)}]");
        }

        private void DrawAiDataDetails(AiData ai)
        {
            EditorGUILayout.LabelField($"    Target: {ai.target} | Attack Type: {ai.attackType}");
            EditorGUILayout.LabelField($"    Attack Count: {ai.atk_Cnt} | Interval: {ai.atk_Interval}s");
            EditorGUILayout.LabelField($"    Ranges: [{string.Join(", ", ai.attackRanges)}]");
            EditorGUILayout.LabelField($"    Abilities: [{string.Join(", ", ai.ability_Ids)}]");
        }

        private void DrawAbilityDetails(AbilityData ability)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"🔥 [{ability.abilityId}] {ability.nameDesc}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Type: {ability.abilityType}", GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"    💥 VALUE: {ability.value}", valueStyle);
            EditorGUILayout.LabelField($"    📊 RATIO: {ability.ratio:F2}", valueStyle);
            EditorGUILayout.LabelField($"    ⏱️ DURATION: {ability.duration}", valueStyle);
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(ability.skillIcon))
            {
                EditorGUILayout.LabelField($"    Icon: {ability.skillIcon}");
            }
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 Summary", headerStyle);

            var damageAbilities = currentRelationship.abilityDatas.Where(a => a.abilityType == AbilityType.Damage).ToList();
            var healAbilities = currentRelationship.abilityDatas.Where(a => a.abilityType == AbilityType.Heal).ToList();
            var buffAbilities = currentRelationship.abilityDatas.Where(a => a.abilityType == AbilityType.AtkUp || a.abilityType == AbilityType.DefUp).ToList();

            if (damageAbilities.Count > 0)
            {
                int totalDamage = damageAbilities.Sum(a => a.value);
                EditorGUILayout.LabelField($"💀 Total Damage Potential: {totalDamage}", valueStyle);
            }

            if (healAbilities.Count > 0)
            {
                int totalHeal = healAbilities.Sum(a => a.value);
                EditorGUILayout.LabelField($"💚 Total Heal Potential: {totalHeal}", valueStyle);
            }

            if (buffAbilities.Count > 0)
            {
                EditorGUILayout.LabelField($"⬆️ Buff Abilities: {buffAbilities.Count}", valueStyle);
            }

            EditorGUILayout.LabelField($"🔗 Total Connection Depth: Card → {currentRelationship.aiGroupDatas.Count} Groups → {currentRelationship.aiDatas.Count} Actions → {currentRelationship.abilityDatas.Count} Abilities");

            EditorGUILayout.EndVertical();
        }

        // 변경사항 감지 메서드들
        private bool HasCardChanges()
        {
            if (currentRelationship?.cardData == null) return false;
            var card = currentRelationship.cardData;
            return changeTracker.currentValues.ContainsKey($"card_{card.card_id}_hp") ||
                   changeTracker.currentValues.ContainsKey($"card_{card.card_id}_atk") ||
                   changeTracker.currentValues.ContainsKey($"card_{card.card_id}_def") ||
                   changeTracker.currentValues.ContainsKey($"card_{card.card_id}_costMana") ||
                   changeTracker.currentValues.ContainsKey($"card_{card.card_id}_costGold") ||
                   changeTracker.currentValues.ContainsKey($"card_{card.card_id}_aiGroup_ids");
        }

        private bool HasAiChanges()
        {
            try
            {
                return currentRelationship?.aiDatas?.Any(ai =>
                    ai != null && (
                    changeTracker.currentValues.ContainsKey($"ai_{ai.ai_Id}_atk_Cnt") ||
                    changeTracker.currentValues.ContainsKey($"ai_{ai.ai_Id}_atk_Interval") ||
                    changeTracker.currentValues.ContainsKey($"ai_{ai.ai_Id}_value") ||
                    changeTracker.currentValues.ContainsKey($"ai_{ai.ai_Id}_ability_Ids"))) ?? false;
            }
            catch
            {
                return false;
            }
        }

        private bool HasAbilityChanges()
        {
            try
            {
                return currentRelationship?.abilityDatas?.Any(ability =>
                    ability != null && (
                    changeTracker.currentValues.ContainsKey($"ability_{ability.abilityId}_value") ||
                    changeTracker.currentValues.ContainsKey($"ability_{ability.abilityId}_ratio") ||
                    changeTracker.currentValues.ContainsKey($"ability_{ability.abilityId}_duration"))) ?? false;
            }
            catch
            {
                return false;
            }
        }

        // 저장 메서드들
        private void SaveCardData()
        {
            try
            {
                var cardDataTable = Resources.Load<CardDataTable>("TableExports/CardDataTable");
                if (cardDataTable != null && currentRelationship?.cardData != null)
                {
                    // 메모리상의 CardData 값을 ScriptableObject의 Entity에 반영
                    var cardData = currentRelationship.cardData;
                    var cardEntity = cardDataTable.items.FirstOrDefault(item => item.Card_Id == cardData.card_id);

                    if (cardEntity != null)
                    {
                        cardEntity.Hp = cardData.hp;
                        cardEntity.Atk = cardData.atk;
                        cardEntity.Def = cardData.def;
                        cardEntity.CostMana = cardData.costMana;
                        cardEntity.CostGold = cardData.costGold;
                        cardEntity.AiGroup_ids = string.Join(",", cardData.aiGroup_ids);

                        Debug.Log($"Updated CardEntity [{cardData.card_id}]: HP={cardEntity.Hp}, ATK={cardEntity.Atk}, DEF={cardEntity.Def}, AiGroups=[{string.Join(",", cardEntity.AiGroup_ids)}]");
                    }

                    EditorUtility.SetDirty(cardDataTable);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Success", "Card data saved successfully!", "OK");
                    Debug.Log($"Card data saved for card ID: {cardData.card_id}");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save card data: {e.Message}", "OK");
            }
        }

        private void SaveAiData()
        {
            try
            {
                var aiDataTable = Resources.Load<AiDataTable>("TableExports/AiDataTable");
                if (aiDataTable != null && currentRelationship?.aiDatas != null)
                {
                    // 메모리상의 AiData 값들을 ScriptableObject의 Entity에 반영
                    foreach (var aiData in currentRelationship.aiDatas)
                    {
                        if (aiData != null)
                        {
                            var aiEntity = aiDataTable.items.FirstOrDefault(item => item.Ai_Id == aiData.ai_Id);

                            if (aiEntity != null)
                            {
                                aiEntity.Atk_Cnt = aiData.atk_Cnt;
                                aiEntity.Atk_Interval = aiData.atk_Interval;
                                aiEntity.Value = aiData.value;
                                aiEntity.Ability_id = string.Join(",", aiData.ability_Ids);

                                Debug.Log($"Updated AiEntity [{aiData.ai_Id}]: Atk_Cnt={aiEntity.Atk_Cnt}, Atk_Interval={aiEntity.Atk_Interval}, Value={aiEntity.Value}, Abilities=[{string.Join(",", aiEntity.Ability_id)}]");
                            }
                        }
                    }

                    EditorUtility.SetDirty(aiDataTable);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Success", "AI data saved successfully!", "OK");
                    Debug.Log("AI data saved");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save AI data: {e.Message}", "OK");
            }
        }

        private void SaveAbilityData()
        {
            try
            {
                var abilityDataTable = Resources.Load<AbilityDataTable>("TableExports/AbilityDataTable");
                if (abilityDataTable != null && currentRelationship?.abilityDatas != null)
                {
                    // 메모리상의 AbilityData 값들을 ScriptableObject의 Entity에 반영
                    foreach (var abilityData in currentRelationship.abilityDatas)
                    {
                        if (abilityData != null)
                        {
                            var abilityEntity = abilityDataTable.items.FirstOrDefault(item => item.AbilityData_Id == abilityData.abilityId);

                            if (abilityEntity != null)
                            {
                                abilityEntity.Value = abilityData.value;
                                abilityEntity.Ratio = abilityData.ratio;
                                abilityEntity.Duration = abilityData.duration;

                                Debug.Log($"Updated AbilityEntity [{abilityData.abilityId}]: Value={abilityEntity.Value}, Ratio={abilityEntity.Ratio}, Duration={abilityEntity.Duration}");
                            }
                        }
                    }

                    EditorUtility.SetDirty(abilityDataTable);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Success", "Ability data saved successfully!", "OK");
                    Debug.Log("Ability data saved");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save ability data: {e.Message}", "OK");
            }
        }

        private void SaveAllChanges()
        {
            SaveCardData();
            SaveAiData();
            SaveAbilityData();

            // 원본 값 업데이트
            RecordOriginalValues();
            Repaint();
        }

        private void DiscardDataChanges()
        {
            try
            {
                // 원본 값으로 복원
                if (currentRelationship?.cardData != null)
                {
                    var card = currentRelationship.cardData;
                    if (changeTracker.originalValues.ContainsKey($"card_{card.card_id}_hp"))
                        card.hp = (int)changeTracker.originalValues[$"card_{card.card_id}_hp"];
                    if (changeTracker.originalValues.ContainsKey($"card_{card.card_id}_atk"))
                        card.atk = (int)changeTracker.originalValues[$"card_{card.card_id}_atk"];
                    if (changeTracker.originalValues.ContainsKey($"card_{card.card_id}_def"))
                        card.def = (int)changeTracker.originalValues[$"card_{card.card_id}_def"];
                    if (changeTracker.originalValues.ContainsKey($"card_{card.card_id}_costMana"))
                        card.costMana = (int)changeTracker.originalValues[$"card_{card.card_id}_costMana"];
                    if (changeTracker.originalValues.ContainsKey($"card_{card.card_id}_costGold"))
                        card.costGold = (int)changeTracker.originalValues[$"card_{card.card_id}_costGold"];
                    if (changeTracker.originalValues.ContainsKey($"card_{card.card_id}_aiGroup_ids"))
                    {
                        var originalIds = ((string)changeTracker.originalValues[$"card_{card.card_id}_aiGroup_ids"])
                            .Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
                        card.aiGroup_ids = originalIds;
                    }
                }

                if (currentRelationship?.aiDatas != null)
                {
                    foreach (var aiData in currentRelationship.aiDatas)
                    {
                        if (aiData != null)
                        {
                            if (changeTracker.originalValues.ContainsKey($"ai_{aiData.ai_Id}_atk_Cnt"))
                                aiData.atk_Cnt = (int)changeTracker.originalValues[$"ai_{aiData.ai_Id}_atk_Cnt"];
                            if (changeTracker.originalValues.ContainsKey($"ai_{aiData.ai_Id}_atk_Interval"))
                                aiData.atk_Interval = (float)changeTracker.originalValues[$"ai_{aiData.ai_Id}_atk_Interval"];
                            if (changeTracker.originalValues.ContainsKey($"ai_{aiData.ai_Id}_value"))
                                aiData.value = (int)changeTracker.originalValues[$"ai_{aiData.ai_Id}_value"];
                            if (changeTracker.originalValues.ContainsKey($"ai_{aiData.ai_Id}_ability_Ids"))
                            {
                                var originalIds = ((string)changeTracker.originalValues[$"ai_{aiData.ai_Id}_ability_Ids"])
                                    .Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
                                aiData.ability_Ids = originalIds;
                            }
                        }
                    }
                }

                if (currentRelationship?.abilityDatas != null)
                {
                    foreach (var abilityData in currentRelationship.abilityDatas)
                    {
                        if (abilityData != null)
                        {
                            if (changeTracker.originalValues.ContainsKey($"ability_{abilityData.abilityId}_value"))
                                abilityData.value = (int)changeTracker.originalValues[$"ability_{abilityData.abilityId}_value"];
                            if (changeTracker.originalValues.ContainsKey($"ability_{abilityData.abilityId}_ratio"))
                                abilityData.ratio = (float)changeTracker.originalValues[$"ability_{abilityData.abilityId}_ratio"];
                            if (changeTracker.originalValues.ContainsKey($"ability_{abilityData.abilityId}_duration"))
                                abilityData.duration = (int)changeTracker.originalValues[$"ability_{abilityData.abilityId}_duration"];
                        }
                    }
                }

                changeTracker.Reset();
                Repaint();
                Debug.Log("Data changes discarded - restored to original values");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error discarding data changes: {e.Message}");
                changeTracker.Reset();
            }
        }

        // ========================================
        // 연결 필드 편집 관련 메서드들
        // ========================================

        /// <summary>
        /// 사용 가능한 AI Group ID 목록 반환
        /// </summary>
        private List<int> GetAvailableAiGroupIds()
        {
            try
            {
                if (AiGroupData.aiGroups_list != null)
                {
                    return AiGroupData.aiGroups_list.Select(data => data.aiGroup_Id).ToList();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting available AI Group IDs: {e.Message}");
            }
            return new List<int>();
        }

        /// <summary>
        /// 사용 가능한 Ability ID 목록 반환
        /// </summary>
        private List<int> GetAvailableAbilityIds()
        {
            try
            {
                if (AbilityData.abilityData_list != null)
                {
                    return AbilityData.abilityData_list.Select(data => data.abilityId).ToList();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting available Ability IDs: {e.Message}");
            }
            return new List<int>();
        }

        /// <summary>
        /// 연결 필드 편집 UI - 개별 추가/삭제 버튼과 드롭다운 방식 (B + C 방식 합체)
        /// </summary>
        private void DrawConnectionFieldEditor(string fieldName, List<int> currentIds, List<int> availableIds, System.Action<List<int>> onChanged)
        {
            if (!isEditMode || currentIds == null || availableIds == null) return;

            try
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // 헤더
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"🔗 {fieldName} Connections ({currentIds.Count})", EditorStyles.boldLabel);

                // 전체 추가 버튼
                Color originalBg = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("➕ Add", GUILayout.Width(60)))
                {
                    // 사용 가능한 ID들 중 현재 연결되지 않은 것들만 표시
                    var unconnectedIds = availableIds.Where(id => !currentIds.Contains(id)).ToList();

                    if (unconnectedIds.Count > 0)
                    {
                        // 드롭다운 메뉴 생성 (정렬된 순서)
                        GenericMenu menu = new GenericMenu();

                        foreach (int id in unconnectedIds.OrderBy(x => x))
                        {
                            string displayName = GetDisplayNameForId(fieldName, id);
                            int capturedId = id; // 클로저 문제 해결
                            menu.AddItem(new GUIContent($"[{id}] {displayName}"), false, () =>
                            {
                                try
                                {
                                    if (!currentIds.Contains(capturedId))
                                    {
                                        currentIds.Add(capturedId);
                                        onChanged?.Invoke(currentIds);
                                        changeTracker.RecordChange($"{fieldName}_connections", string.Join(",", currentIds));
                                        Debug.Log($"Added {fieldName} connection: {capturedId}");
                                        Repaint();
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError($"Error adding {fieldName} connection: {e.Message}");
                                }
                            });
                        }

                        menu.ShowAsContext();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("No Available IDs",
                            $"All available {fieldName} IDs ({availableIds.Count}) are already connected.", "OK");
                    }
                }
                GUI.backgroundColor = originalBg;

                // 전체 삭제 버튼
                if (currentIds.Count > 0)
                {
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("🗑️ Clear All", GUILayout.Width(80)))
                    {
                        if (EditorUtility.DisplayDialog("Clear All Connections",
                            $"Are you sure you want to remove all {currentIds.Count} {fieldName} connections?", "Yes", "No"))
                        {
                            currentIds.Clear();
                            onChanged?.Invoke(currentIds);
                            changeTracker.RecordChange($"{fieldName}_connections", string.Join(",", currentIds));
                            Debug.Log($"Cleared all {fieldName} connections");
                            Repaint();
                        }
                    }
                    GUI.backgroundColor = originalBg;
                }

                EditorGUILayout.EndHorizontal();

                // 현재 연결된 ID들 표시 및 개별 삭제
                if (currentIds.Count > 0)
                {
                    EditorGUILayout.Space(5);

                    // 정렬된 순서로 표시
                    var sortedIds = currentIds.OrderBy(x => x).ToList();

                    // 삭제할 ID들을 임시로 저장 (iteration 중 수정 방지)
                    List<int> idsToRemove = new List<int>();

                    foreach (int id in sortedIds)
                    {
                        EditorGUILayout.BeginHorizontal();

                        // ID 정보 표시
                        string displayName = GetDisplayNameForId(fieldName, id);
                        EditorGUILayout.LabelField($"    [{id}] {displayName}", GUILayout.ExpandWidth(true));

                        // 개별 삭제 버튼
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(18)))
                        {
                            idsToRemove.Add(id);
                        }
                        GUI.backgroundColor = originalBg;

                        EditorGUILayout.EndHorizontal();
                    }

                    // 삭제 처리
                    if (idsToRemove.Count > 0)
                    {
                        try
                        {
                            foreach (int idToRemove in idsToRemove)
                            {
                                currentIds.Remove(idToRemove);
                                Debug.Log($"Removed {fieldName} connection: {idToRemove}");
                            }
                            onChanged?.Invoke(currentIds);
                            changeTracker.RecordChange($"{fieldName}_connections", string.Join(",", currentIds));
                            Repaint();
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error removing {fieldName} connections: {e.Message}");
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("    🚫 No connections", EditorStyles.centeredGreyMiniLabel);
                }

                EditorGUILayout.EndVertical();
            }
            catch (System.Exception e)
            {
                EditorGUILayout.LabelField($"Error in connection editor: {e.Message}", EditorStyles.helpBox);
                Debug.LogError($"DrawConnectionFieldEditor error: {e.Message}");
            }
        }

        /// <summary>
        /// ID에 따른 표시 이름 반환
        /// </summary>
        private string GetDisplayNameForId(string fieldType, int id)
        {
            try
            {
                switch (fieldType)
                {
                    case "AiGroup":
                        var aiGroupData = AiGroupData.GetAiGroupData(id);
                        return aiGroupData?.nameDesc ?? "Unknown";

                    case "Ability":
                        var abilityData = AbilityData.GetAbilityData(id);
                        return abilityData?.nameDesc ?? "Unknown";

                    default:
                        return "Unknown";
                }
            }
            catch
            {
                return "Error";
            }
        }
    }
}
