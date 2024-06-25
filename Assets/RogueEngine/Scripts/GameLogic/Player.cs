using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Represent the current state of a player during the game (data only)

    [System.Serializable]
    public class Player
    {
        public int player_id;
        public string user_id;
        public string username;
        public string avatar;

        public bool connected = false; //Connected to server and game
        public bool ready = false;     //Sent all player data, ready to play

        public int gold = 0;

        public UserData udata;

        public Player(int id, string user_id, string username) 
        { 
            this.player_id = id;
            this.user_id = user_id;
            this.username = username;
            udata = new UserData(user_id, username); 
            udata.FixData(); 
        }

        public bool IsReady() { return ready; }
        public bool IsConnected() { return connected; }

        public virtual bool IsEnemy()
        {
            return player_id == 0; //Enemies have player_id 0, players have player_id 1,2,3,4
        }
    }

}