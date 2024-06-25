using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueEngine
{
    //Default fx and audio, some can be overrided on each individual card

    [CreateAssetMenu(fileName = "AssetData", menuName = "TcgEngine/AssetData", order = 0)]
    public class AssetData : ScriptableObject
    {
        [Header("FX")]
        public GameObject character_spawn_fx;
        public GameObject character_destroy_fx;
        public GameObject damage_fx;
        public GameObject play_card_fx;
        public GameObject dice_roll_fx;
        public GameObject hover_text_box;
        public GameObject new_turn_fx;
        public GameObject win_fx;
        public GameObject lose_fx;
        public Material color_ui;
        public Material grayscale_ui;

        [Header("Audio")]
        public AudioClip character_spawn_audio;
        public AudioClip character_destroy_audio;
        public AudioClip hand_card_click_audio;
        public AudioClip new_turn_audio;
        public AudioClip win_audio;
        public AudioClip defeat_audio;
        public AudioClip win_music;
        public AudioClip defeat_music;

        public static AssetData Get()
        {
            return DataLoader.Get().assets;
        }
    }
}
