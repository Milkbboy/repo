using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ERang.Data;

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

        public void SetCard(GameCard card)
        {
            foreach (var pair in statObjectPairs)
                pair.gameObject.SetActive(false);

            string cardName, cardDesc, cardShortDesc;

            if (card is MasterCard)
                Utils.GetMasterText(card.Id, out cardName, out cardDesc, out cardShortDesc);
            else
                Utils.GetCardText(card.Id, out cardName, out cardDesc, out cardShortDesc);

            cardNameText.text = cardName;
            descText.text = cardShortDesc;

            cardTypeText.text = card.CardType.ToString();

            if (card.CardType == CardType.Creature)
            {
                ActiveStatObjects(new List<StatType> { StatType.Hp, StatType.Mana, StatType.Atk, StatType.Def }, true);

                hpText.text = card.State.Hp.ToString();
                manaText.text = card.State.Mana.ToString();
                atkText.text = card.State.Atk.ToString();
                defText.text = card.State.Def.ToString();
            }
            else if (card.CardType == CardType.Master || card.CardType == CardType.EnemyMaster)
            {
                ActiveStatObjects(new List<StatType> { StatType.Hp, StatType.Mana, StatType.Def }, true);

                hpText.text = card.State.Hp.ToString();
                manaText.text = card.State.Mana.ToString();
                defText.text = card.State.Def.ToString();
            }
            else if (card.CardType == CardType.Magic)
            {
                List<StatType> statTypes = new List<StatType> { StatType.Mana };

                int atk = card.State.Atk;
                if (atk > 0)
                {
                    statTypes.Add(StatType.Atk);
                    atkText.text = atk.ToString();
                }

                ActiveStatObjects(statTypes, true);
                manaText.text = card.State.Mana.ToString();
            }
            else if (card.CardType == CardType.Building)
            {
                ActiveStatObjects(new List<StatType> { StatType.Mana, StatType.Gold }, true);
                manaText.text = card.State.Mana.ToString();
            }
            else if (card.CardType == CardType.Gold)
            {
                // GoldCard 또는 HpCard와 같은 아이템 카드
                if (card is IGoldCard goldCard)
                {
                    descText.text = $"골드 {goldCard.Gold} 획득";
                }
                else
                {
                    // HpCard 등 다른 아이템 카드
                    int hpValue = card.State.Hp;
                    if (hpValue > 0)
                    {
                        descText.text = $"hp {hpValue} 회복";
                    }
                }
            }

            Texture2D cardTexture = card.CardImage;

            if (cardMeshRenderer != null)
            {
                // 원래 텍스쳐 저장
                if (originTexture == null)
                    originTexture = (Texture2D)cardMeshRenderer.materials[0].GetTexture("_BaseMap");

                cardMeshRenderer.materials[0].SetTexture("_MainTex", cardTexture);
            }
            else
            {
                cardImage.sprite = Sprite.Create(cardTexture, new Rect(0, 0, cardTexture.width, cardTexture.height), Vector2.zero);
            }
        }

        public void SetMasterStat(int hp, int maxHp, int atk, int def, int mana = 0, int maxMana = 0)
        {
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

        public void SetFloatingGold(int beforeGold, int afterGold)
        {
            ShowFloatingText("Gold", beforeGold.ToString(), afterGold.ToString());
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

        public void ShowDesc(int cardId)
        {
            descText.text = Utils.GetCardDescText(cardId);
        }

        public void ShowShortDesc(int cardId)
        {
            descText.text = Utils.GetCardShortDescText(cardId);
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
            if (floatingTextPrefab == null || floatingTextParent == null)
            {
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

            string diffText = diff > 0 ? $"<color=green>+{diff}</color>" : $"<color=red>{diff}</color>";
            TextMeshPro floatingText = Instantiate(floatingTextPrefab, floatingTextParent);
            floatingText.text = $"{text} {diffText}";

            RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition += new Vector2(0, floatingTextList.Count * 25f);

            floatingTextList.Add(floatingText);

            StartCoroutine(BlinkFloatingText(floatingText));
        }

        private IEnumerator ScrollFloatingText(TextMeshPro floatingText)
        {
            Vector3 startPos = floatingText.transform.position;
            Vector3 endPos = startPos + new Vector3(0, 50, 0);
            float duration = 4f;
            float elapsed = 0;

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
            float duration = 3f;
            float blinkInterval = 0.5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += blinkInterval;
                yield return new WaitForSeconds(blinkInterval);
            }

            floatingTextList.Remove(floatingText);
            Destroy(floatingText.gameObject);
        }
    }
}