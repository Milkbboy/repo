using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Contains world data persisting across battles
    //This file is also everything that will be saved to a save file

    [System.Serializable]
    public class World
    {
        public string filename;
        public string version;
        public string title;
        public string game_uid;
        public bool online;

        public string scenario_id;
        public int seed;

        public WorldState state = WorldState.None;
        public bool completed = false;
        public string map_id;
        public int map_location_id;
        public int map_index = 0;

        public Battle battle = null;
        public string event_id;
        public string event_text;
        public string event_champion;

        public List<Player> players = new List<Player>();
        public List<Champion> champions = new List<Champion>();
        public List<CharacterAlly> allies = new List<CharacterAlly>();
        public List<Map> maps = new List<Map>();
        public Dictionary<string, int> custom_ints = new Dictionary<string, int>();

        public List<string> shop_cards = new List<string>();
        public List<string> shop_items = new List<string>();
        public float shop_buy_ratio = 1f;
        public float shop_sell_ratio = 1f;

        private static World loaded_data;
        private const string last_save_id = "tcg_last_save";
        private const string last_save_multi_id = "tcg_last_save_multi";
        private const string extension = ".tcg";

        public World() { }

        public World(string filename)
        {
            this.filename = filename;
            version = Application.version;
            game_uid = GameTool.GenerateRandomID();
            this.seed = GameTool.GenerateRandomInt();
        }

        public void FixData()
        {
            if (string.IsNullOrEmpty(game_uid))
                game_uid = GameTool.GenerateRandomID();
            if (string.IsNullOrEmpty(title))
                title = "Game";
            if (string.IsNullOrEmpty(filename))
                filename = "game";

            if (champions == null)
                champions = new List<Champion>();
            if (allies == null)
                allies = new List<CharacterAlly>();
            if (maps == null)
                maps = new List<Map>();
            if (custom_ints == null)
                custom_ints = new Dictionary<string, int>();
        }

        //----- Players --------

        public virtual bool IsMultiplayer()
        {
            return online;
        }

        public virtual bool AreAllPlayersReady()
        {
            int ready = 0;
            foreach (Player player in players)
            {
                if (player.IsReady())
                    ready++;
            }
            return ready >= players.Count;
        }

        public virtual bool AreAllPlayersConnected()
        {
            int ready = 0;
            foreach (Player player in players)
            {
                if (player.IsConnected())
                    ready++;
            }
            return ready >= players.Count;
        }

        public virtual bool AreAllChampionsDead()
        {
            foreach (Champion champion in champions)
            {
                if (champion.GetHP() > 0)
                    return false;
            }
            return true;
        }

        public virtual void ResetActionCompleted(bool completed = false)
        {
            foreach (Champion champ in champions)
                champ.action_completed = completed;
        }

        public virtual void SetActionCompleted(int player_id, bool completed)
        {
            foreach (Champion champ in champions)
            {
                if (champ.player_id == player_id)
                    champ.action_completed = completed;
            }
        }

        public virtual bool AreAllActionsCompleted()
        {
            foreach (Champion champ in champions)
            {
                Player player = GetPlayer(champ.player_id);
                if (player.IsConnected() && !champ.action_completed)
                    return false; //Waiting for a player action
            }
            return true;
        }

        public virtual bool AreAllActionsCompleted(int player_id)
        {
            foreach (Champion champ in champions)
            {
                if (champ.player_id == player_id)
                {
                    Player player = GetPlayer(champ.player_id);
                    if (player.IsConnected() && !champ.action_completed)
                        return false; //Waiting for a player action
                }
            }
            return true;
        }

        public Player AddPlayer(string user_id, string username)
        {
            Player player = GetPlayer(user_id);
            if (player != null)
                return player;

            Player nplayer = new Player(players.Count, user_id, username);
            nplayer.connected = true;
            players.Add(nplayer);
            return nplayer;
        }

        public void DisconnectAll()
        {
            foreach (Player player in players)
                player.connected = false;
        }

        public Player GetPlayer(string user_id)
        {
            foreach (Player player in players)
            {
                if (player.user_id == user_id)
                    return player;
            }
            return null;
        }

        public Player GetPlayer(int id)
        {
            foreach (Player player in players)
            {
                if (player.player_id == id)
                    return player;
            }
            return null;
        }

        public void AddChampion(Champion champ)
        {
            champions.Add(champ);
        }

        public void RemoveChampion(string uid)
        {
            for (int i = champions.Count - 1; i >= 0; i--)
            {
                if (champions[i].uid == uid)
                    champions.RemoveAt(i);
            }
        }

        public void RemoveChampion(int slot_x)
        {
            for (int i = champions.Count - 1; i >= 0; i--)
            {
                if (champions[i].position == slot_x)
                    champions.RemoveAt(i);
            }
        }

        public Champion GetSlotChampion(int slot_x)
        {
            foreach (Champion champion in champions)
            {
                if (champion.position == slot_x)
                    return champion;
            }
            return null;
        }

        public Champion GetChampion(int player_id)
        {
            foreach (Champion champion in champions)
            {
                if (champion.player_id == player_id)
                    return champion;
            }
            return null;
        }

        public Champion GetChampion(string uid)
        {
            foreach (Champion champion in champions)
            {
                if (champion.uid == uid)
                    return champion;
            }
            return null;
        }

        //Get first champion of player that is still action_completed == false
        public Champion GetNextActionChampion(int player_id)
        {
            foreach (Champion champion in champions)
            {
                if (!champion.IsDead() && champion.player_id == player_id && !champion.action_completed)
                    return champion;
            }
            return null;
        }

        //-----------------------

        public void AddAlly(CharacterAlly ally)
        {
            allies.Add(ally);
        }

        public void RemoveAlly(string uid)
        {
            for (int i = allies.Count - 1; i >= 0; i--)
            {
                if (allies[i].uid == uid)
                {
                    allies.RemoveAt(i);
                    return;
                }
            }
        }

        public CharacterAlly GetAlly(string uid)
        {
            foreach (CharacterAlly ally in allies)
            {
                if (ally.uid == uid)
                    return ally;
            }
            return null;
        }

        public CharacterAlly GetAlly(CharacterData character)
        {
            foreach (CharacterAlly ally in allies)
            {
                if (ally.CharacterData.id == character.id)
                    return ally;
            }
            return null;
        }

        public CharacterAlly GetSlotAly(int slot_x)
        {
            foreach (CharacterAlly ally in allies)
            {
                if (ally.position == slot_x)
                    return ally;
            }
            return null;
        }

        //-----------------------

        public void SetCustomValue(string id, int value)
        {
            custom_ints[id] = value;
        }

        public void AddCustomValue(string id, int value)
        {
            if (custom_ints.ContainsKey(id))
                custom_ints[id] += value;
            else
                custom_ints[id] = value;
        }

        public int GetCustomValue(string id)
        {
            if (custom_ints.ContainsKey(id))
                return custom_ints[id];
            return 0;
        }

        //-----------------------

        public bool CanMoveTo(MapLocation dest)
        {
            if (state != WorldState.Map)
                return false; //Cant move if another state


            MapLocation current = GetLocation(map_id, map_location_id);
            if (current == null)
            {
                return dest.depth == 1; //Can only move to first level if not at any location
            }

            foreach (int loc_id in current.adjacency)
            {
                if (loc_id == dest.ID)
                    return true;
            }
            return false;
        }

        public void AddExplored(MapLocation location)
        {
            Map map = GetMap(location.map_id);
            if (map != null)
            {
                map.AddExplored(location.ID);
            }
        }

        public bool IsExplored(MapLocation location)
        {
            Map map = GetMap(location.map_id);
            if (map != null)
            {
                return map.IsExplored(location.ID);
            }
            return false;
        }

        public Map GetMap(MapData mdata)
        {
            if (mdata != null)
                return GetMap(mdata.id);
            return null;
        }

        public Map GetMap(string map_id)
        {
            foreach (Map amap in maps)
            {
                if (amap.map_id == map_id)
                    return amap;
            }
            return null;
        }

        public MapLocation GetLocation(string map_id, int loc_id)
        {
            Map map = GetMap(map_id);
            return map?.GetLocation(loc_id);
        }

        public MapLocation GetCurrentLocation()
        {
            return GetLocation(map_id, map_location_id);
        }

        public int GetCurrentLocationDepth()
        {
            MapLocation loc = GetCurrentLocation();
            if (loc != null)
                return loc.depth;
            return 0;
        }

        public int GetLocationSeed(int offset = 0)
        {
            MapLocation loc = GetCurrentLocation();
            if (loc != null)
                return loc.seed + offset;
            return seed + offset;
        }
        
        public int GetEmptySlotPos()
        {
            for (int x = Slot.x_min; x <= Slot.x_max; x++)
            {
                if (GetSlotChampion(x) == null && GetSlotAly(x) == null)
                    return x;
            }
            return 0;
        }

        public virtual int CountCharacters()
        {
            return allies.Count + champions.Count;
        }

        public int GetAverageXP()
        {
            int count = 0;
            int xp = 0;
            foreach (Champion champion in champions)
            {
                xp += champion.GetTotalXP();
                count++;
            }
            return xp / count;
        }

        public int GetBuyCost(CardData item)
        {
            int cost = Mathf.RoundToInt(item.cost * shop_buy_ratio);
            return cost;
        }

        public int GetSellCost(CardData item)
        {
            int cost = Mathf.RoundToInt(item.cost * shop_sell_ratio);
            return cost;
        }

        public int GetUpgradeCost(CardData card, int level)
        {
            int cost = card.GetUpgradeCost(level);
            return cost;
        }

        public bool HasStarted()
        {
            return state != WorldState.None && state != WorldState.Setup && state != WorldState.SetupLoad;
        }

        public bool HasEnded()
        {
            return state == WorldState.Ended;
        }

        //---------------------

        public int GetCompleteScore()
        {
            return completed ? 100 : 0;
        }

        public int GetXpScore()
        {
            int xp = GetAverageXP();
            return Mathf.RoundToInt(xp / 10f);
        }

        public int GetCardsScore()
        {
            int score = 0;
            foreach (Champion champion in champions)
            {
                foreach (ChampionCard card in champion.cards)
                {
                    if (card.level == 1)
                        score += 2;
                    if (card.level == 2)
                        score += 3;
                    if (card.level == 3)
                        score += 4;
                    if (card.level == 4)
                        score += 5;
                    if (card.level == 5)
                        score += 7;
                    if (card.level == 6)
                        score += 8;
                    if (card.level >= 7)
                        score += 10;
                }
            }
            return score;
        }

        public int GetTotalScore()
        {
            int c_score = GetCompleteScore();
            int xp_score = GetXpScore();
            int card_score = GetCardsScore();
            int score = Mathf.RoundToInt(c_score + xp_score + card_score);
            return score;
        }


        //----------------------


        public void Save()
        {
            Save(filename, this);
        }

        public void Save(string filename)
        {
            Save(filename, this);
        }

        public static void Save(string filename, World data)
        {
            if (!string.IsNullOrEmpty(filename) && data != null)
            {
                filename = filename.ToLower();
                data.filename = filename;
                data.version = Application.version;
                loaded_data = data;

                string user_id = Authenticator.Get().UserID;
                SaveTool.SaveFile<World>(filename + extension, data);
                if (data.IsMultiplayer())
                    SetLastSaveMulti(user_id, filename);
                else
                    SetLastSave(user_id, filename);
            }
        }

        public static World NewGame(string filename)
        {
            filename = filename.ToLower();
            loaded_data = new World(filename);
            loaded_data.FixData();
            return loaded_data;
        }

        public static World Load(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            filename = filename.ToLower();
            loaded_data = SaveTool.LoadFile<World>(filename + extension);
            if (loaded_data != null)
            {
                loaded_data.filename = filename;
                loaded_data.FixData();
            }
            return loaded_data;
        }

        //Use when loading a new scene, it will keep the current save file, unless there is none, then it will load
        public static World AutoLoad(string filename)
        {
            if (!SaveTool.IsValidFilename(filename))
                return null;
            if (loaded_data == null)
                loaded_data = Load(filename);
            if (loaded_data == null)
                loaded_data = NewGame(filename);
            return loaded_data;
        }

        //Clear current load, and then try to either load or create a new game
        public static World NewOrLoad(string filename)
        {
            if (!SaveTool.IsValidFilename(filename))
                return null;
            Unload();
            loaded_data = Load(filename);
            if (loaded_data == null)
                loaded_data = NewGame(filename);
            return loaded_data;
        }

        public static void Override(World data)
        {
            loaded_data = data;
        }

        public static void SetLastSave(string user_id, string filename)
        {
            if (SaveTool.IsValidFilename(filename))
            {
                PlayerPrefs.SetString(last_save_id + "_" + user_id, filename);
            }
        }

        public static string GetLastSave(string user_id)
        {
            string name = PlayerPrefs.GetString(last_save_id + "_" + user_id, "");
            return name;
        }

        public static void SetLastSaveMulti(string user_id, string filename)
        {
            if (SaveTool.IsValidFilename(filename))
            {
                PlayerPrefs.SetString(last_save_multi_id + "_" + user_id, filename);
            }
        }

        public static string GetLastSaveMulti(string user_id)
        {
            string name = PlayerPrefs.GetString(last_save_multi_id + "_" + user_id, "");
            return name;
        }

        public static bool HasLastSave(string user_id)
        {
            return HasSave(GetLastSave(user_id));
        }

        public static bool HasSave(string filename)
        {
            return SaveTool.DoesFileExist(filename.ToLower() + extension);
        }

        public static void Delete(string filename)
        {
            SaveTool.DeleteFile(filename.ToLower() + extension);
        }

        public static void Unload()
        {
            loaded_data = null;
        }

        public static World GetLoaded()
        {
            return loaded_data;
        }

        public static List<World> GetAllSolo(string user_id)
        {
            List<World> worlds = new List<World>();
            List<string> files = SaveTool.GetAllSave(extension);
            foreach (string file in files)
            {
                World world = SaveTool.LoadFile<World>(file);
                if (world != null && !world.IsMultiplayer() && world.GetPlayer(user_id) != null)
                {
                    world.FixData();
                    worlds.Add(world);
                }
            }
            return worlds;
        }

        public static List<World> GetAllMultiplayer(string user_id)
        {
            List<World> worlds = new List<World>();
            List<string> files = SaveTool.GetAllSave(extension);
            foreach (string file in files)
            {
                World world = SaveTool.LoadFile<World>(file);
                if (world != null && world.IsMultiplayer() && world.GetPlayer(user_id) != null)
                {
                    world.FixData();
                    worlds.Add(world);
                }
            }
            return worlds;
        }

        public static List<World> GetAll(string user_id)
        {
            List<World> worlds = new List<World>();
            List<string> files = SaveTool.GetAllSave(extension);
            foreach (string file in files)
            {
                World world = SaveTool.LoadFile<World>(file);
                if (world != null && world.GetPlayer(user_id) != null)
                {
                    world.FixData();
                    worlds.Add(world);
                }
            }
            return worlds;
        }

        public static List<World> GetAll()
        {
            List<World> worlds = new List<World>();
            List<string> files = SaveTool.GetAllSave(extension);
            foreach (string file in files)
            {
                World world = SaveTool.LoadFile<World>(file);
                if (world != null)
                {
                    world.FixData();
                    worlds.Add(world);
                }
            }
            return worlds;
        }
    }

    [System.Serializable]
    public class EventQueueItem
    {
        public string event_id;
        public string champion_uid;
    }

    [System.Serializable]
    public enum WorldState
    {
        None = 0,

        Setup = 2,
        SetupLoad = 4,

        Map = 10,
        Battle = 20,
        EventChoice = 30,
        EventText = 32,

        Reward = 50,
        Trash = 51,
        Shop = 52,
        Upgrade = 54,
        LevelUp = 55,

        Ended = 100,
    }

}