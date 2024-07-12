using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ERang
{
    public class ChampionData : ScriptableObject
    {
        public string uid;
        public string championName;
        public string desc;
        public int atk;
        public int hp;
        public int def;
        public int mana;

        public List<string> startCardUids;

        public void Initialize(ChampionEntity entity)
        {
            this.uid = entity.uid;
            this.championName = entity.name;
            this.desc = entity.desc;
            this.atk = entity.atk;
            this.hp = entity.hp;
            this.def = entity.def;
            this.mana = entity.mana;

            if (!string.IsNullOrEmpty(entity.startCard_1)) startCardUids.Add(entity.startCard_1);
            if (!string.IsNullOrEmpty(entity.startCard_2)) startCardUids.Add(entity.startCard_2);
            if (!string.IsNullOrEmpty(entity.startCard_3)) startCardUids.Add(entity.startCard_3);
            if (!string.IsNullOrEmpty(entity.startCard_4)) startCardUids.Add(entity.startCard_4);
            if (!string.IsNullOrEmpty(entity.startCard_5)) startCardUids.Add(entity.startCard_5);
        }

        public static List<ChampionData> champ_list = new List<ChampionData>();
        public static Dictionary<string, ChampionData> champ_dict = new Dictionary<string, ChampionData>();

        public static void Load(string path = "")
        {
            ExcelChampion champItems = Resources.Load<ExcelChampion>(path);

            if (champItems == null)
            {
                Debug.LogError("ChampionData not loaded");
                return;
            }

            foreach (var championEntity in champItems.items)
            {
                string assetPath = $"Assets/ERang/Resources/Champions/{championEntity.name}.asset";
                ChampionData championData = AssetDatabase.LoadAssetAtPath<ChampionData>(assetPath);

                if (championData == null)
                {
                    championData = CreateInstance<ChampionData>();
                    AssetDatabase.CreateAsset(championData, assetPath);
                }
                else
                {
                    championData.startCardUids = new List<string>();
                }

                championData.Initialize(championEntity);

                champ_list.Add(championData);
                champ_dict.Add(championData.uid, championData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("ChampionData loaded");
        }
    }
}