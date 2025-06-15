using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ERang.Data;

namespace ERang
{
    public class DataRelationshipWindow : EditorWindow
    {
        [System.Serializable]
        public class CardRelationshipInfo
        {
            public CardData cardData;
            public List<AiGroupData> aiGroupDatas = new();
            public List<AiData> aiDatas = new();
            public List<AbilityData> abilityDatas = new();
        }

        private Vector2 scrollPosition;
        private int selectedCardIndex = 0;
        private string[] cardNames;
        private List<CardData> allCards;
        private CardRelationshipInfo currentRelationship;
        private bool isDataLoaded = false;
        private string searchString = "";
        private List<CardData> filteredCards;

        // GUI 스타일
        private GUIStyle headerStyle;
        private GUIStyle cardStyle;
        private GUIStyle groupStyle;
        private GUIStyle aiStyle;
        private GUIStyle abilityStyle;
        private GUIStyle valueStyle;

        [MenuItem("ERang/Data Relationship Analyzer")]
        public static void ShowWindow()
        {
            DataRelationshipWindow window = GetWindow<DataRelationshipWindow>("Data Relationship Analyzer");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            LoadAllData();
            InitializeStyles();
        }

        private void InitializeStyles()
        {
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
            // 모든 데이터 테이블 로드
            CardData.Load("TableExports/CardDataTable");
            AiGroupData.Load("TableExports/AiGroupDataTable");
            AiData.Load("TableExports/AiDataTable");
            AbilityData.Load("TableExports/AbilityDataTable");

            allCards = CardData.card_list;
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
            }
        }

        private void UpdateCardNames()
        {
            cardNames = filteredCards.Select(card => $"[{card.card_id}] {card.nameDesc}").ToArray();
        }

        private void OnGUI()
        {
            if (!isDataLoaded)
            {
                EditorGUILayout.HelpBox("Data is not loaded. Please make sure all data tables exist in Resources/TableExports/", MessageType.Error);
                if (GUILayout.Button("Reload Data"))
                {
                    LoadAllData();
                }
                return;
            }

            EditorGUILayout.LabelField("카드 데이터 관계 분석기", headerStyle);
            EditorGUILayout.Space();

            DrawCardSelectionArea();
            EditorGUILayout.Space();

            if (currentRelationship != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawRelationshipTree();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawCardSelectionArea()
        {
            EditorGUILayout.BeginHorizontal();

            // 검색 필드
            EditorGUI.BeginChangeCheck();
            string newSearchString = EditorGUILayout.TextField("Search Cards:", searchString);
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

            if (filteredCards.Count > 0)
            {
                // 카드 선택 드롭다운
                EditorGUI.BeginChangeCheck();
                selectedCardIndex = EditorGUILayout.Popup("Select Card:", selectedCardIndex, cardNames);
                if (EditorGUI.EndChangeCheck() && selectedCardIndex < filteredCards.Count)
                {
                    AnalyzeCardRelationship(filteredCards[selectedCardIndex]);
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
            }
        }

        private void AnalyzeCardRelationship(CardData selectedCard)
        {
            currentRelationship = new CardRelationshipInfo { cardData = selectedCard };

            // AiGroup 데이터 수집
            foreach (int aiGroupId in selectedCard.aiGroup_ids)
            {
                AiGroupData aiGroupData = AiGroupData.GetAiGroupData(aiGroupId);
                if (aiGroupData != null)
                {
                    currentRelationship.aiGroupDatas.Add(aiGroupData);

                    // AiData 수집
                    foreach (var aiGroup in aiGroupData.ai_Groups)
                    {
                        foreach (int aiId in aiGroup)
                        {
                            AiData aiData = AiData.GetAiData(aiId);
                            if (aiData != null && !currentRelationship.aiDatas.Contains(aiData))
                            {
                                currentRelationship.aiDatas.Add(aiData);

                                // AbilityData 수집
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

        private void DrawRelationshipTree()
        {
            if (currentRelationship?.cardData == null) return;

            // 카드 정보
            EditorGUILayout.BeginVertical(cardStyle);
            EditorGUILayout.LabelField($"🃏 CARD: [{currentRelationship.cardData.card_id}] {currentRelationship.cardData.nameDesc}", headerStyle);
            DrawCardDetails(currentRelationship.cardData);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // AI Group 정보
            if (currentRelationship.aiGroupDatas.Count > 0)
            {
                EditorGUILayout.LabelField($"🔗 Connected AI Groups ({currentRelationship.aiGroupDatas.Count})", headerStyle);

                foreach (var aiGroupData in currentRelationship.aiGroupDatas)
                {
                    EditorGUILayout.BeginVertical(groupStyle);
                    EditorGUILayout.LabelField($"  📁 AI Group [{aiGroupData.aiGroup_Id}] - Type: {aiGroupData.aiGroupType}");
                    DrawAiGroupDetails(aiGroupData);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.Space();

            // AI Data 정보
            if (currentRelationship.aiDatas.Count > 0)
            {
                EditorGUILayout.LabelField($"⚔️ Connected AI Actions ({currentRelationship.aiDatas.Count})", headerStyle);

                foreach (var aiData in currentRelationship.aiDatas)
                {
                    EditorGUILayout.BeginVertical(aiStyle);
                    EditorGUILayout.LabelField($"  🎯 AI [{aiData.ai_Id}] {aiData.name} - {aiData.type}");
                    DrawAiDataDetails(aiData);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.Space();

            // Ability 정보 - 가장 중요한 최종 값들
            if (currentRelationship.abilityDatas.Count > 0)
            {
                EditorGUILayout.LabelField($"✨ FINAL ABILITY VALUES ({currentRelationship.abilityDatas.Count})", headerStyle);

                EditorGUILayout.BeginVertical(abilityStyle);
                foreach (var abilityData in currentRelationship.abilityDatas)
                {
                    DrawAbilityDetails(abilityData);
                    EditorGUILayout.Space(3);
                }
                EditorGUILayout.EndVertical();
            }

            // 요약 정보
            EditorGUILayout.Space();
            DrawSummary();
        }

        private void DrawCardDetails(CardData card)
        {
            EditorGUILayout.LabelField($"Type: {card.cardType} | Grade: {card.cardGrade}");
            EditorGUILayout.LabelField($"Cost: {card.costMana} Mana, {card.costGold} Gold");
            EditorGUILayout.LabelField($"Stats: HP {card.hp} | ATK {card.atk} | DEF {card.def}");
            EditorGUILayout.LabelField($"AI Groups: [{string.Join(", ", card.aiGroup_ids)}]");
        }

        private void DrawAiGroupDetails(AiGroupData aiGroup)
        {
            EditorGUILayout.LabelField($"    Groups: {string.Join(" | ", new[] { aiGroup.ai_Group_1, aiGroup.ai_Group_2, aiGroup.ai_Group_3, aiGroup.ai_Group_4, aiGroup.ai_Group_5, aiGroup.ai_Group_6 }.Where(x => !string.IsNullOrEmpty(x)))}");

            if (aiGroup.reactions.Count > 0)
            {
                EditorGUILayout.LabelField($"    Reactions: {aiGroup.reactions.Count} conditional reactions");
            }
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
    }
}
