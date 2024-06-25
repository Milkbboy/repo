using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Represent a player champion

    [System.Serializable]
    public class Champion
    {
        public string character_id;
        public int player_id;
        public string uid;
        public int position; //Ordering position in battle, 1 is front, then 2,3..

        public int level;
        public int xp;

        public int hp;
        public int damage;

        public float speed;
        public float hand;
        public float energy;

        public Reward reward = null;
        public int free_upgrades = 0;
        public bool action_completed = false;

        public float ongoing_gold_bonus = 0f; //in added percentage, 0.5f = +50%
        public float ongoing_xp_bonus = 0f;   //in added percentage, 0.5f = +50%
        public float ongoing_buy_factor = 0f;  //in added percentage, 0.5f = +50%, 0 = unchanged
        public float ongoing_sell_factor = 0f;  //in added percentage, 0.5f = +50%, 0 = unchanged

        public List<ChampionCard> cards = new List<ChampionCard>();
        public List<ChampionItem> inventory = new List<ChampionItem>();
        public List<CharacterAlly> allies = new List<CharacterAlly>();

        [System.NonSerialized] private ChampionData data = null;
        [System.NonSerialized] private int hash = 0;

        public Champion() { }
        public Champion(int id, string uid) { this.player_id = id; this.uid = uid; }

        public virtual int GetHP() { return Mathf.Max(hp - damage, 0); }
        public virtual int GetHPMax() { return Mathf.Max(hp, 0); }
        public virtual int GetDamage() { return Mathf.Max(damage, 0); }
        public virtual bool IsDead() { return GetHP() <= 0; }

        public virtual int GetSpeed() { return Mathf.Max(Mathf.FloorToInt(speed), 1); }
        public virtual int GetHand() { return Mathf.Max(Mathf.FloorToInt(hand), 1); }
        public virtual int GetEnergy() { return Mathf.Max(Mathf.FloorToInt(energy), 1); }
        public virtual int GetLevel() { return level; }

        public virtual void ClearOngoing() { ongoing_gold_bonus = 0f; ongoing_xp_bonus = 0f; ongoing_buy_factor = 0f; ongoing_sell_factor = 0f; }

        //------ Inventory ---------

        public void AddCard(CardData icard, int level = 1)
        {
            ChampionCard card = new ChampionCard(icard, level);
            cards.Add(card);
        }

        public void UpgradeCard(string uid)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].uid == uid && cards[i].level < cards[i].CardData.level_max)
                    cards[i].level++;
            }
        }

        public void RemoveCard(CardData icard)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].card_id == icard.id)
                {
                    cards.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveCard(string uid)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].uid == uid)
                {
                    cards.RemoveAt(i);
                    return;
                }
            }
        }

        public ChampionCard GetCard(CardData card)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].card_id == card.id)
                    return cards[i];
            }
            return null;
        }

        public ChampionCard GetCard(string uid)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].uid == uid)
                    return cards[i];
            }
            return null;
        }

        public void AddItem(CardData iitem, int quantity = 1)
        {
            if (iitem != null)
            {
                bool found = false;
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i].card_id == iitem.id)
                    {
                        inventory[i].quantity += quantity;
                        found = true;
                    }
                }

                if (!found)
                {
                    ChampionItem item = new ChampionItem(iitem, quantity);
                    inventory.Add(item);
                }
            }
        }

        public void RemoveItem(CardData icard, int quantity = 1)
        {
            if (icard != null)
            {
                RemoveItem(icard, quantity);
            }
        }

        public void RemoveItem(string item_id, int quantity = 1)
        {
            if (item_id != null)
            {
                for (int i = inventory.Count - 1; i >= 0; i--)
                {
                    if (inventory[i].card_id == item_id)
                    {
                        inventory[i].quantity -= quantity;
                        if(inventory[i].quantity <=0)
                            inventory.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public ChampionItem GetItem(string item_id)
        {
            for (int i = inventory.Count - 1; i >= 0; i--)
            {
                if (inventory[i].card_id == item_id)
                    return inventory[i];
            }
            return null;
        }

        public bool HasItem(CardData item)
        {
            if (item != null)
            {
                for (int i = inventory.Count - 1; i >= 0; i--)
                {
                    if (inventory[i].card_id == item.id)
                        return true;
                }
            }
            return false;
        }

        public int CountItem(CardData card)
        {
            int count = 0;
            foreach (ChampionItem citem in inventory)
            {
                if (citem.card_id == card.id)
                    count += citem.quantity;
            }
            return count;
        }

        public bool HasCard(CardData card)
        {
            if (card != null)
            {
                for (int i = cards.Count - 1; i >= 0; i--)
                {
                    if (cards[i].card_id == card.id)
                        return true;
                }
            }
            return false;
        }

        public int CountCard(CardData card)
        {
            int count = 0;
            foreach (ChampionCard citem in cards)
            {
                if (citem.card_id == card.id)
                    count++;
            }
            return count;
        }

        //---------------

        public List<ChampionCard> GetDeck()
        {
            List<ChampionCard> deck = new List<ChampionCard>();
            foreach (ChampionCard card in cards)
            {
                deck.Add(card);
            }
            return deck;
        }

        public bool CanLevelUp()
        {
            return xp >= GameplayData.Get().GetXpForNextLevel(level);
        }

        public int GetTotalXP()
        {
            int xp = this.xp;
            for (int i = 1; i < level; i++)
                xp += GameplayData.Get().GetXpForNextLevel(i);
            return xp;
        }

        public float GetGoldFactor()
        {
            return 1f + ongoing_gold_bonus;
        }

        public float GetXPFactor()
        {
            return 1f + ongoing_xp_bonus;
        }

        //-----------------

        public ChampionData ChampionData
        {
            get
            {
                if (data == null || data.id != character_id)
                    data = ChampionData.Get(character_id); //Optimization, store for future use
                return data;
            }
        }

        public int Hash
        {
            get
            {
                if (hash == 0)
                    hash = Mathf.Abs(uid.GetHashCode()); //Optimization, store for future use
                return hash;
            }
        }

        public static Champion Create(ChampionData champion, int player_id)
        {
            Champion character = new Champion();
            character.character_id = champion.id;
            character.player_id = player_id;
            character.uid = GameTool.GenerateRandomID();
            character.level = 1;
            character.hp = champion.hp;
            character.speed = champion.speed;
            character.hand = champion.hand;
            character.energy = champion.energy;
            return character;
        }
    }

    [System.Serializable]
    public class ChampionCard
    {
        public string card_id;
        public string uid;
        public int level;

        public ChampionCard()
        {
            uid = GameTool.GenerateRandomID();
        }

        public ChampionCard(CardData icard, int lvl)
        {
            card_id = icard.id;
            level = lvl;
            uid = GameTool.GenerateRandomID();
        }

        public CardData CardData { get { return CardData.Get(card_id); }  }
    }

    [System.Serializable]
    public class ChampionItem
    {
        public string card_id;
        public int quantity;

        public ChampionItem()
        {
            
        }

        public ChampionItem(CardData icard, int q)
        {
            card_id = icard.id;
            quantity = q;
        }

        public CardData CardData { get { return CardData.Get(card_id); } }
    }

    [System.Serializable]
    public class CharacterAlly
    {
        public string character_id;
        public string uid;
        public int player_id;
        public int position; //Ordering position in battle, 1 is front, then 2,3..
        public int damage;
        public int level;

        public CharacterAlly()
        {
            uid = GameTool.GenerateRandomID();
        }

        public CharacterAlly(CharacterData charact, int pid, int lvl = 1)
        {
            character_id = charact.id;
            player_id = pid;
            level = lvl;
            uid = GameTool.GenerateRandomID();
        }

        public CharacterData CharacterData { get { return CharacterData.Get(character_id); } }
    }

}