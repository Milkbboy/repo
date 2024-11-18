using UnityEngine;
using TMPro;
using ERang.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ERang
{
    [System.Serializable]
    public class StatObjectPair
    {
        public StatType statType;
        public GameObject gameObject;
    }

    public class CardUI : MonoBehaviour
    {
        public Image cardImage;
        public MeshRenderer cardMeshRenderer;
        public TextMeshPro cardNameText;
        public TextMeshPro cardTypeText;
        public TextMeshPro descText;
        public TextMeshPro hpText;
        public TextMeshPro manaText;
        public TextMeshPro atkText;
        public TextMeshPro defText;
        public TextMeshPro floatingTextPrefab;
        public Transform floatingTextParent;
        public Dictionary<StatType, GameObject> statObjects = new();

        [SerializeField]
        private List<StatObjectPair> statObjectPairs = new List<StatObjectPair>();

        private Texture2D originTexture;
        private List<TextMeshPro> floatingTextList = new();

        void Awake()
        {
            foreach (var pair in statObjectPairs)
            {
                if (!statObjects.ContainsKey(pair.statType))
                    statObjects.Add(pair.statType, pair.gameObject);
            }
        }

        public void SetCard(BaseCard card)
        {
            foreach (var pair in statObjectPairs)
                pair.gameObject.SetActive(false);

            string cardName, cardDesc, cardShortDesc;

            if (card is MasterCard)
                Utils.GetMasterText(card.Id, out cardName, out cardDesc, out cardShortDesc);
            else
                Utils.GetCardText(card.Id, out cardName, out cardDesc, out cardShortDesc);  

            cardNameText.text = cardName;
            descText.text = cardDesc;

            cardTypeText.text = card.CardType.ToString();

            if (card is CreatureCard creatureCard)
            {
                ActiveStatObjects(new List<StatType> { StatType.Hp, StatType.Mana, StatType.Atk, StatType.Def }, true);

                hpText.text = creatureCard.Hp.ToString();
                manaText.text = creatureCard.Mana.ToString();
                atkText.text = creatureCard.Atk.ToString();
                defText.text = creatureCard.Def.ToString();
            }

            if (card is MasterCard masterCard)
            {
                ActiveStatObjects(new List<StatType> { StatType.Hp, StatType.Mana, StatType.Def }, true);

                hpText.text = masterCard.Hp.ToString();
                manaText.text = masterCard.Mana.ToString();
                defText.text = masterCard.Def.ToString();
            }

            if (card is MagicCard magicCard)
            {
                List<StatType> statTypes = new List<StatType> { StatType.Mana, StatType.Atk };

                if (magicCard.Atk > 0)
                    statTypes.Add(StatType.Atk);

                ActiveStatObjects(statTypes, true);

                hpText.text = string.Empty;
                manaText.text = magicCard.Mana.ToString();
                atkText.text = magicCard.Atk.ToString();
                defText.text = string.Empty;
            }

            if (card is BuildingCard buildingCard)
            {
                ActiveStatObjects(new List<StatType> { StatType.Mana, StatType.Gold }, true);

                hpText.text = string.Empty;
                manaText.text = buildingCard.Gold.ToString();
                atkText.text = string.Empty;
                defText.text = string.Empty;
            }

            Texture2D cardTexture = card.CardImage;

            if (cardMeshRenderer != null)
            {
                // 원래 텍스쳐 저장
                if (originTexture == null)
                    originTexture = (Texture2D)cardMeshRenderer.materials[0].GetTexture("_BaseMap");

                // cardMeshRenderer.materials[0].SetTexture("_BaseMap", cardTexture);
                cardMeshRenderer.materials[0].SetTexture("_MainTex", cardTexture);
            }
            else
            {
                cardImage.sprite = Sprite.Create(cardTexture, new Rect(0, 0, cardTexture.width, cardTexture.height), Vector2.zero);
            }
        }

        public void SetEnemyMasterCard(Enemy enemy)
        {
            // Debug.Log("CardUI SetEnemyMasterCard: " + enemy.enemyId);
            MasterData enemyData = MasterData.master_dict[enemy.enemyId];

            // Debug.Log("CardUI SetCard: " + cardId);
            Texture2D enemyTexture = enemyData.GetMasterTexture();

            if (!enemyTexture)
            {
                Debug.LogError($"${enemy.enemyId} Enemy Master texture is null");
                return;
            }

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", enemyTexture);
            }

            if (cardTypeText != null)
            {
                cardTypeText.text = "Enemy Master";
            }

            SetMasterStat(enemy.Hp, enemy.MaxHp, enemy.Atk, enemy.Def);
        }

        public void SetMasterStat(int hp, int maxHp, int atk, int def, int mana = 0, int maxMana = 0)
        {
            // if (descText != null)
            //     descText.text = $"hp: {hp}/{maxHp}\nmana: {mana}/{maxMana}\natk: {atk}\ndef: {def}";

            hpText.text = $"{hp}";
            manaText.text = $"{mana}";
            atkText.text = atk.ToString();
            defText.text = def.ToString();
        }

        public void SetMana(int mana)
        {
            ShowFloatingText("mana", manaText.text, mana.ToString());
            manaText.text = mana.ToString();
        }

        public void SetHp(int hp)
        {
            ShowFloatingText("hp", hpText.text, hp.ToString());
            hpText.text = hp.ToString();
        }

        public void SetAtk(int atk)
        {
            if (atk < 0)
                atk = 0;

            ShowFloatingText("Atk", atkText.text, atk.ToString());
            atkText.text = atk.ToString();
        }

        public void SetDef(int def)
        {
            if (def < 0)
                def = 0;

            ShowFloatingText("Def", defText.text, def.ToString());
            defText.text = def.ToString();
        }

        public void SetFloatingGold(int beforeGlod, int afterGold)
        {
            ShowFloatingText("Gold", beforeGlod.ToString(), afterGold.ToString());
        }

        public void ResetStat()
        {
            hpText.text = string.Empty;
            manaText.text = string.Empty;
            atkText.text = string.Empty;
            defText.text = string.Empty;

            // 원래 텍스쳐로 복구
            if (originTexture != null)
            {
                cardMeshRenderer.materials[0].SetTexture("_BaseMap", originTexture);
            }
        }

        public void SetDesc(string desc)
        {
            descText.text = desc;
        }

        private void ActiveStatObjects(List<StatType> statTypes, bool activate)
        {
            foreach (var pair in statObjectPairs)
            {
                if (statTypes.Contains(pair.statType))
                {
                    pair.gameObject.SetActive(activate);
                }
            }
        }

        private void ShowFloatingText(string text, string oldValue, string newValue)
        {
            // Debug.Log($"ShowFloatingText: {stat}, {oldValue}, {newValue}");

            if (floatingTextPrefab == null || floatingTextParent == null)
            {
                Debug.LogWarning("FloatingTextPrefab or FloatingTextParent is null");
                return;
            }

            if (!int.TryParse(oldValue, out int oldValueInt))
            {
                Debug.LogWarning("oldValue is not int");
                return;
            }


            if (!int.TryParse(newValue, out int newValueInt))
            {
                Debug.LogWarning("newValue is not int");
                return;
            }


            int diff = newValueInt - oldValueInt;

            if (diff == 0) return;

            string diffText = diff > 0 ? $"<color=green>+{diff}</color>" : $"<color=red>{diff.ToString()}</color>";
            TextMeshPro floatingText = Instantiate(floatingTextPrefab, floatingTextParent);
            floatingText.text = $"{text} {diffText}";

            RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition += new Vector2(0, floatingTextList.Count * 25f);

            floatingTextList.Add(floatingText);

            StartCoroutine(BlinkFloatingText(floatingText));
        }

        private IEnumerator ScrollFloatingText(TextMeshPro floatingText)
        {
            Vector3 startPos = floatingText.transform.position; // 시작 위치
            Vector3 endPos = startPos + new Vector3(0, 50, 0);
            float duration = 4f; // 지속시간
            float elapsed = 0; // 경과 시간

            while (elapsed < duration)
            {
                floatingText.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            floatingTextList.Remove(floatingText);
            Destroy(floatingText.gameObject);
        }

        private IEnumerator BlinkFloatingText(TextMeshPro floatingText)
        {
            float duration = 3f; // 지속시간
            float blinkInterval = 0.5f; // 깜빡이는 간격
            float elapsed = 0; // 경과 시간

            while (elapsed < duration)
            {
                // 투명도 변경
                // Color color = floatingText.color;
                // color.a = color.a == 1 ? 0 : 1;
                // floatingText.color = color;

                elapsed += blinkInterval;
                yield return new WaitForSeconds(blinkInterval);
            }

            floatingTextList.Remove(floatingText);
            Destroy(floatingText.gameObject);
        }
    }
}