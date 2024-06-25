using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace RogueEngine
{
    [System.Serializable]
    public enum GameType
    {
        Solo = 0,
        MultiHost = 10,     //Server Host
        MultiJoin = 15,     //Join Game
        RelayHost = 20,
        RelayJoin = 25,
    }

    [System.Serializable]
    public class ConnectSettings
    {
        public GameType game_type;      //Solo or multiplayer
        public string server_url;       //Server url
        public string game_uid;         //Unique game ID on the server
        public string filename;         //Filename to load/save if host
        public string title;            //Display name
        public bool file_host;          //Host upload the save file, may not be same as server host
        public bool load;               //If should load save or new game

        [System.NonSerialized]
        public RelayConnectData relay_data; //Relay data

        public virtual string GetUrl()
        {
            if (!string.IsNullOrEmpty(server_url))
                return server_url;
            return NetworkData.Get().url;
        }

        public virtual bool IsOnline()
        {
            return game_type == GameType.MultiHost || game_type == GameType.MultiJoin || game_type == GameType.RelayHost || game_type == GameType.RelayJoin;
        }

        public virtual bool IsServerHost()
        {
            return game_type == GameType.MultiHost || game_type == GameType.RelayHost || game_type == GameType.Solo;
        }

        public virtual bool IsRelay()
        {
            return game_type == GameType.RelayHost || game_type == GameType.RelayJoin;
        }

        public virtual bool IsFileHost()
        {
            return file_host;
        }

        public static ConnectSettings Default
        {
            get
            {
                ConnectSettings settings = new ConnectSettings();
                settings.game_type = GameType.Solo;
                settings.game_uid = GameTool.GenerateRandomID();
                settings.server_url = "";
                settings.file_host = true;
                settings.filename = "";
                settings.title = "";
                return settings;
            }
        }
    }
}