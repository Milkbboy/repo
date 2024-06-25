using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    /// <summary>
    /// Main config file for all network-related things
    /// Server API password is not in this file (and is in the Server scene instead) to prevent exposing it to client build
    /// </summary>

    [CreateAssetMenu(fileName = "NetworkData", menuName = "TcgEngine/NetworkData", order = 0)]
    public class NetworkData : ScriptableObject
    {
        [Header("Game Server")]
        public string url;
        public ushort port;
        public ServerType server_type;      //For Lobby only (lobby is dedicated server), will it connect players directly or will the dedicated server host the full game
        public SoloType solo_type;          //For solo play only, is it using Netcode or not

        //[Header("API")]
        //public AuthenticatorType auth_type;
        //public string api_url;
        //public bool api_https;

        public static NetworkData Get()
        {
            return TcgNetwork.Get().data;
        }
    }

    public enum ServerType
    {
        Peer2Peer = 0,
        UnityServices = 10,         //Lobby Services + Relay Services
        Dedicated = 20,             //Dedicated Server
    }

    public enum SoloType
    {
        UseNetcode = 0,     //Use Netcode network messages in solo to have more similar behavior on both multiplayer and solo, Recommended for consistency between multi/solo
        Offline = 10        //Make solo totally offline (no netcode) but may behave differently than multiplayer, required for WebGL since StartHost don't work on webgl.
    }
}
