using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ERang.Table;

namespace ERang.Data
{
    public class MasterData
    {
        public int master_Id; // 마왕의 Id 값
        public string nameDesc; // 마왕 캐릭터의 실제 이름
        public MasterType masterType; // 마왕의 타입
        public string cardNameId; // 마왕 캐릭터의 실제 이름 String이 들어간 데이터 Id
        public string cardDescId; // 마왕의 캐릭터 설명을 담은 String이 들어간 데이터 Id
        public string cardShortDescId; // 마왕 간단 설명
        public int masterAi_id; // 마왕의 공격 타입에 대한 Id (근거리, 원거리, 혹은 폭파 등)
        public int hp; // 마왕 캐릭터의 초기 체력 값
        public int atk; // 마왕 캐릭터의 초기 공격력 값 (공격력 값이 0인 캐릭터는 공격을 시도하지 않는다)
        public int def; // 마왕 캐릭터의 초기 방어력 값
        public int startMana; // 마왕 캐릭터의 초기 마나 값
        public int maxMana; // 마왕 캐릭터의 최대 마나 값
        public int rechargeMana; // 턴이 다시 시작 될 때 얻게 되는 마나 초기 값
        public string startCardDeckIds; // 마왕이 처음 스테이지에 진입 할 때 갖게되는 카드의 복수 값
        public int startArtiFact_id; // 마왕이 처음 시작 시 갖고 있는 아티팩트의 id 값
        public string StartAbilityIds; // 마왕이 선천적으로 가지고 있는 특성 id 값
        public int creatureSlots; // 마왕이 소환할 수 있는 크리쳐의 슬롯 개수
        public int satietyGauge; // 마왕의 포만감 게이지 초기 값
        public int maxSatietyGauge; // 마왕의 포만감 게이지 최대 값
        public List<int> startCardIds = new List<int>(); // 마왕이 처음 스테이지에 진입 할 때 갖게되는 카드 id 리스트
        public List<int> startAbilityIds = new List<int>(); // 마왕이 선천적으로 가지고 있는 특성 id 리스트

        [Header("Display")]
        public Texture2D masterTexture;

        public void Initialize(MasterDataEntity entity)
        {
            master_Id = entity.Master_Id;
            nameDesc = entity.NameDesc;
            masterType = GetMasterType(master_Id);
            cardNameId = entity.MasterNameDesc_Id;
            cardDescId = entity.MasterShortDesc_Id;
            cardShortDescId = entity.MasterDesc_Id;
            masterAi_id = entity.MasterAi_id;
            hp = entity.Hp;
            atk = entity.Atk;
            def = entity.Def;
            startMana = entity.StartMana;
            maxMana = entity.MaxMana;
            rechargeMana = entity.RechargeMana;
            creatureSlots = entity.CreatureSlots;
            satietyGauge = entity.SatietyGauge;
            maxSatietyGauge = entity.MaxSatietyGauge;
            startArtiFact_id = entity.StartArtiFact_Id;

            // entity.StartCardDeck_Id 문자열을 ","로 분리하고, 결과에서 빈칸을 제거합니다.
            startCardIds.AddRange(Utils.ParseIntArray(entity.StartCardDeck_Id).Where(x => x != 0));
            startAbilityIds.AddRange(Utils.ParseIntArray(entity.StartAbility_Id).Where(x => x != 0));

            // 이미지 로드
            string texturePath = $"Textures/{master_Id}";
            masterTexture = Resources.Load<Texture2D>(texturePath);

            if (masterTexture == null)
                Debug.LogError($"MasterData {master_Id} texture is null");
        }

        public static List<MasterData> master_list = new List<MasterData>();
        public static Dictionary<int, MasterData> master_dict = new Dictionary<int, MasterData>();

        public static void Load(string path = "")
        {
            MasterDataTable masterDataTable = Resources.Load<MasterDataTable>(path);

            if (masterDataTable == null)
            {
                Debug.LogError("MasterDataTable not found");
                return;
            }

            foreach (var MasterEntity in masterDataTable.items)
            {
                if (master_dict.ContainsKey(MasterEntity.Master_Id))
                    continue;

                MasterData masterData = new();

                masterData.Initialize(MasterEntity);

                master_list.Add(masterData);
                master_dict.Add(masterData.master_Id, masterData);

                // Debug.Log("MasterData loaded: " + MasterData.championName + " " + MasterData.uid + " " + MasterData.atk + " " + MasterData.hp + " " + MasterData.def + " " + MasterData.mana + " " + MasterData.startCardIds.Count);
            }
        }

        public static MasterData GetMasterData(int master_id)
        {
            return master_dict.TryGetValue(master_id, out MasterData masterData) ? masterData : null;
        }

        public static List<MasterData> GetDatas()
        {
            return master_list;
        }

        public Texture2D GetMasterTexture()
        {
            return masterTexture;
        }

        /// <summary>
        /// 카드 id, name을 반환
        /// </summary>
        public static List<(int, string)> GetCardIdNames()
        {
            List<(int, string)> cardIds = new();

            foreach (var master in master_list)
            {
                cardIds.Add((master.master_Id, master.nameDesc));
            }

            return cardIds;
        }

        public MasterType GetMasterType(int masterId)
        {
            return masterId switch
            {
                1001 => MasterType.Luci,
                1002 => MasterType.CrawlSeul,
                1003 => MasterType.BarakRahum,
                _ => MasterType.None,
            };
        }
    }
}