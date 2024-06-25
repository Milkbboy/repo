using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Contain UserData saved to local file
    /// </summary>

    [System.Serializable]
    public class UserData
    {
        public string id;
        public string username;
        public string version;

        public string email;
        public string avatar;
        public string cardback;

        private List<string> unlocked_cards = new List<string>();

        [System.NonSerialized] private List<string> just_unlocked_cards = new List<string>(); //Unlocked this run

        private const string extension = ".user";

        public UserData()
        {
        }

        public UserData(string user_id, string username)
        {
            this.id = user_id;
            this.username = username;
        }

        public void FixData()
        {
            if (unlocked_cards == null)
                unlocked_cards = new List<string>();
            if (just_unlocked_cards == null)
                just_unlocked_cards = new List<string>();
        }

        public void UnlockCard(CardData card)
        {
            if (!unlocked_cards.Contains(card.id))
            {
                unlocked_cards.Add(card.id);
                just_unlocked_cards.Add(card.id);
            }
        }

        public bool IsCardUnlocked(CardData card)
        {
            return unlocked_cards.Contains(card.id);
        }

        public List<string> GetJustUnlockedCards()
        {
            return just_unlocked_cards;
        }

        public void ClearJustUnlocked()
        {
            just_unlocked_cards.Clear();
        }

        public string GetAvatar()
        {
            if (avatar != null)
                return avatar;
            return "";
        }

        public string GetCardback()
        {
            if (cardback != null)
                return cardback;
            return "";
        }

        //----------------------


        public void Save()
        {
            Save(this);
        }

        public static void Save(UserData data)
        {
            if (!string.IsNullOrEmpty(data.id) && data != null && SaveTool.IsValidFilename(data.id))
            {
                data.version = Application.version;
                SaveTool.SaveFile<UserData>(GetFilename(data.id), data);
            }
        }

        public static UserData NewUser(string user_id, string username)
        {
            UserData udata = new UserData(user_id, username);
            udata.FixData();
            return udata;
        }

        public static UserData Load(string user_id)
        {
            if (string.IsNullOrEmpty(user_id))
                return null;

            UserData udata = SaveTool.LoadFile<UserData>(GetFilename(user_id));
            if (udata != null)
                udata.FixData();
            return udata;
        }

        public static bool HasSave(string user_id)
        {
            return !string.IsNullOrEmpty(user_id) && SaveTool.DoesFileExist(GetFilename(user_id));
        }

        public static void Delete(string user_id)
        {
            SaveTool.DeleteFile(GetFilename(user_id));
        }

        public static string GetFilename(string user_id)
        {
            return user_id + extension;
        }
    }
}

