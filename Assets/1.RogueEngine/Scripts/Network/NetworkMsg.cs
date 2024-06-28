
using Unity.Netcode;
using UnityEngine.Events;

namespace RogueEngine
{
    //-------- Connection --------

    public class MsgPlayerConnect : INetworkSerializable
    {
        public string user_id;
        public string username;
        public string game_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref game_uid);
        }
    }

    public class MsgNewScenario : INetworkSerializable
    {
        public string scenario;
        public string filename;
        public string title;
        public bool online;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref scenario);
            serializer.SerializeValue(ref filename);
            serializer.SerializeValue(ref title);
            serializer.SerializeValue(ref online);
        }
    }

    public class MsgLoadScenario : INetworkSerializable
    {
        public World world;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.Serializer(serializer, ref world);
        }
    }

    public class MsgAddChampion : INetworkSerializable
    {
        public string champion_id;
        public int slot_x;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_id);
            serializer.SerializeValue(ref slot_x);
        }
    }

    public class MsgStartGame : INetworkSerializable
    {
        public int seed;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref seed);
        }
    }

    public class MsgStartTest : INetworkSerializable
    {
        public int seed;
        public WorldState test_state = WorldState.None;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref seed);
            serializer.SerializeValue(ref test_state);
        }
    }

    public class MsgAfterConnected : INetworkSerializable
    {
        public bool success;
        public int player_id;
        public World world;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref success);
            serializer.SerializeValue(ref player_id);
            NetworkTool.Serializer(serializer, ref world);
        }
    }

    //-------- Matchmaking --------

    public class MsgMatchmaking : INetworkSerializable
    {
        public string user_id;
        public string username;
        public string group;
        public int players;
        public bool refresh;
        public float time;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref refresh);
            serializer.SerializeValue(ref time);
        }
    }

    public class MatchmakingResult : INetworkSerializable
    {
        public bool success;
        public int players;
        public string group;
        public string server_url;
        public string game_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref success);
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref group);
            serializer.SerializeValue(ref server_url);
            serializer.SerializeValue(ref game_uid);
        }
    }

    public class MsgLobbyCreate : INetworkSerializable
    {
        public string user_id;
        public string username;
        public string title;
        public string subtitle;
        public string filename;
        public bool load;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref title);
            serializer.SerializeValue(ref subtitle);
            serializer.SerializeValue(ref filename);
            serializer.SerializeValue(ref load);
        }
    }

    public class MsgLobbyJoin : INetworkSerializable
    {
        public string user_id;
        public string username;
        public string uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref user_id);
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref uid);
        }
    }

    public class LobbyList : INetworkSerializable
    {
        public LobbyGame[] games;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            NetworkTool.NetSerializeArray(serializer, ref games);
        }
    }

    public class MsgLobbyUID : INetworkSerializable
    {
        public string uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref uid);
        }
    }

    //-------- In Map --------

    public class MsgMapMove : INetworkSerializable
    {
        public string map_id;
        public int location_id;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref map_id);
            serializer.SerializeValue(ref location_id);
        }
    }

    public class MsgEventChoice : INetworkSerializable
    {
        public string champion_uid;
        public string choice;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
            serializer.SerializeValue(ref choice);
        }
    }

    public class MsgRewardChoice : INetworkSerializable
    {
        public string champion_uid;
        public string target_id;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
            serializer.SerializeValue(ref target_id);
        }
    }

    public class MsgChampionCard : INetworkSerializable
    {
        public string champion_uid;
        public string card_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
            serializer.SerializeValue(ref card_uid);
        }
    }

    public class MsgLevelUp : INetworkSerializable
    {
        public string champion_uid;
        public string skill_id;
        public string target_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
            serializer.SerializeValue(ref skill_id);
            serializer.SerializeValue(ref target_uid);
        }
    }

    public class MsgShop : INetworkSerializable
    {
        public string champion_uid;
        public string item_id;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
            serializer.SerializeValue(ref item_id);
        }
    }

    public class MsgChampion : INetworkSerializable
    {
        public string champion_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
        }
    }

    public class MsgID : INetworkSerializable
    {
        public string champion_uid;
        public string select_id;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref champion_uid);
            serializer.SerializeValue(ref select_id);
        }
    }

    //-------- In Battle --------

    public class MsgPlayCard : INetworkSerializable
    {
        public string card_uid;
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref card_uid);
            serializer.SerializeValue(ref slot);
        }
    }

    public class MsgMove : INetworkSerializable
    {
        public string character_uid;
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref character_uid);
            serializer.SerializeValue(ref slot);
        }
    }

    public class MsgDamaged : INetworkSerializable
    {
        public string character_uid;
        public int damage;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref character_uid);
            serializer.SerializeValue(ref damage);
        }
    }

    public class MsgUse : INetworkSerializable
    {
        public string character_uid;
        public string uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref character_uid);
            serializer.SerializeValue(ref uid);
        }
    }


    public class MsgUID : INetworkSerializable
    {
        public string uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref uid);
        }
    }

    public class MsgCastAbility : INetworkSerializable
    {
        public string ability_id;
        public string card_uid;
        public string target_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ability_id);
            serializer.SerializeValue(ref card_uid);
            serializer.SerializeValue(ref target_uid);
        }
    }

    public class MsgCastAbilitySlot : INetworkSerializable
    {
        public string ability_id;
        public string card_uid;
        public Slot slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ability_id);
            serializer.SerializeValue(ref card_uid);
            serializer.SerializeNetworkSerializable(ref slot);
        }
    }

    public class MsgSecret : INetworkSerializable
    {
        public string secret_uid;
        public string triggerer_uid;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref secret_uid);
            serializer.SerializeValue(ref triggerer_uid);
        }
    }

    public class MsgInt : INetworkSerializable
    {
        public int value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref value);
        }
    }

    public class MsgChat : INetworkSerializable
    {
        public string username;
        public string text;
        public int index;

        public MsgChat() { }
        public MsgChat(string user, string txt) { username = user; text = txt; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref username);
            serializer.SerializeValue(ref text);
            serializer.SerializeValue(ref index);
        }
    }

    public class MsgUserData : INetworkSerializable
    {
        public UserData user_data;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    user_data = NetworkTool.Deserialize<UserData>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(user_data);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if (size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }

    public class MsgRefreshWorld : INetworkSerializable
    {
        public World world_data;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    world_data = NetworkTool.Deserialize<World>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(world_data);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if (size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }

    public class MsgRefreshBattle : INetworkSerializable
    {
        public Battle battle_data;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                int size = 0;
                serializer.SerializeValue(ref size);
                if (size > 0)
                {
                    byte[] bytes = new byte[size];
                    serializer.SerializeValue(ref bytes);
                    battle_data = NetworkTool.Deserialize<Battle>(bytes);
                }
            }

            if (serializer.IsWriter)
            {
                byte[] bytes = NetworkTool.Serialize(battle_data);
                int size = bytes.Length;
                serializer.SerializeValue(ref size);
                if (size > 0)
                    serializer.SerializeValue(ref bytes);
            }
        }
    }

}