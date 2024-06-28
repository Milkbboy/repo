using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine.Client
{
    //Unlock permanent rewards after a match

    public class ProgressManager : MonoBehaviour
    {
        private static ProgressManager instance;

        void Awake()
        {
            instance = this;
        }

        //Called when a run end to unlock new cards and items
        public async void UnlockNewRewards(int nb_cards, int nb_items)
        {
            World world = GameClient.Get().GetWorld();
            UserData udata = Authenticator.Get().UserData;
            Player player = GameClient.Get().GetPlayer();

            if (udata == null || world == null || player == null)
                return; //Invalid data

            //int score = world.GetTotalScore();
            //bool victory = world.completed;
            udata.ClearJustUnlocked();

            //Loop on champions owned by the player
            int nb = 0;
            for (int n = 0; n < nb_cards; n++)
            {
                foreach (Champion champ in world.champions)
                {
                    if (champ.player_id == player.player_id && nb < nb_cards)
                    {
                        //Unlock class cards
                        List<CardData> cards = champ.ChampionData.GetLockedCards(player);
                        List<CardData> unlock_cards = new List<CardData>();
                        unlock_cards = GameTool.PickXRandom(cards, unlock_cards, 1);
                        nb++;

                        foreach (CardData card in unlock_cards)
                            udata.UnlockCard(card);
                    }
                }
            }

            //Unlock Items
            List<CardData> items = CardData.GetLockedItems(udata, null);
            List<CardData> unlock_items = new List<CardData>();
            unlock_items = GameTool.PickXRandom(items, unlock_items, nb_items);

            foreach (CardData card in unlock_items)
                udata.UnlockCard(card);

            //Save
            await Authenticator.Get().SaveUserData();
        }


        public static ProgressManager Get()
        {
            return instance;
        }

    }
}
