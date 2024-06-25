using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueEngine.Client;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    public class SetupSlot : MonoBehaviour
    {
        public int x;

        [Header("UI")]
        public GameObject empty_area;
        public GameObject edit_area;
        public Text title;

        private int index = 0;
        private int max_index;
        private Bounds bounds;

        private BoardCharacter character;

        private static List<SetupSlot> slot_list = new List<SetupSlot>();

        protected virtual void Awake()
        {
            slot_list.Add(this);
        }

        protected virtual void OnDestroy()
        {
            slot_list.Remove(this);
        }

        private void Start()
        {
            empty_area.SetActive(false);
            edit_area.SetActive(false);
            max_index = ChampionData.GetAll().Count;
        }

        protected virtual void Update()
        {
            if (!GameClient.Get().IsReady())
                return;

            RefreshChampion();
        }

        private void RefreshChampion()
        {
            World world = GameClient.Get().GetWorld();
            ScenarioData scenario = ScenarioData.Get(world.scenario_id);
            Champion champion = world.GetSlotChampion(x);
            if (champion != null)
            {
                if (character == null || character.GetUID() != champion.uid)
                {
                    ReplaceChampion(champion);
                    title.text = champion.ChampionData.title;
                }
            }
            else
            {
                if (character != null)
                    Destroy(character.gameObject);
                character = null;
                title.text = "";
            }

            empty_area.SetActive(character == null && scenario != null && x <= scenario.champions);
            edit_area.SetActive(character != null && champion.player_id == GameClient.Get().GetPlayerID());
        }

        private void ReplaceChampion(Champion champion)
        {
            if (character != null)
                Destroy(character.gameObject);

            GameObject obj = Instantiate(champion.ChampionData.prefab, transform.position, Quaternion.identity);
            character = obj.GetComponent<BoardCharacter>();
            character.SetChampion(champion);
        }

        //When clicking on the slot
        public void OnMouseDown()
        {
            if (BattleUI.IsOverUI())
                return;

        }

        public void OnClickPrev()
        {
            index -= 1;
            index = (index + max_index) % max_index;

            ChampionData cdata = ChampionData.GetAll()[index];
            GameClient.Get().CreateChampion(cdata, x);
        }

        public void OnClickNext()
        {
            index += 1;
            index = (index + max_index) % max_index;

            ChampionData cdata = ChampionData.GetAll()[index];
            GameClient.Get().CreateChampion(cdata, x);
        }

        public void OnClickNew()
        {
            index = 0;
            ChampionData cdata = ChampionData.GetAll()[index];
            GameClient.Get().CreateChampion(cdata, x);
        }

        public void OnClickDelete()
        {
            GameClient.Get().DeleteChampion(x); 
        }
        
        //Find the actual slot coordinates of this board slot
        public virtual Slot GetSlot()
        {
            return new Slot(x, false);
        }

        public virtual bool HasSlot(Slot slot)
        {
            Slot aslot = GetSlot();
            return aslot == slot;
        }

        public virtual bool IsInside(Vector3 wpos)
        {
            return bounds.Contains(wpos);
        }

        public virtual bool IsNearest(Vector3 pos, float range = 999f)
        {
            SetupSlot nearest = GetNearest(pos, range);
            return nearest == this;
        }

        public static SetupSlot GetNearest(Vector3 pos, float range = 999f)
        {
            SetupSlot nearest = null;
            float min_dist = range;
            foreach (SetupSlot slot in GetAll())
            {
                float dist = (slot.transform.position - pos).magnitude;
                if (slot.IsInside(pos) && dist < min_dist)
                {
                    min_dist = dist;
                    nearest = slot;
                }
            }
            return nearest;
        }

        public static SetupSlot Get(Slot slot)
        {
            foreach (SetupSlot bslot in GetAll())
            {
                if (bslot.HasSlot(slot))
                    return bslot;
            }
            return null;
        }

        public static List<SetupSlot> GetAll()
        {
            return slot_list;
        }

    }
}