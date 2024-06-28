using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Client;
using RogueEngine.UI;

namespace RogueEngine.Client
{
    /// <summary>
    /// Visual representation of a Slot.cs
    /// </summary>

    public class BoardSlot : MonoBehaviour
    {
        public int x;
        public bool enemy;

        protected SpriteRenderer glow;
        protected Collider collide;
        protected Bounds bounds;
        protected float start_alpha = 0f;
        protected float current_alpha = 0f;
        protected bool is_hover = false;

        private static List<BoardSlot> slot_list = new List<BoardSlot>();

        protected virtual void Awake()
        {
            slot_list.Add(this);
            glow = GetComponent<SpriteRenderer>();
            collide = GetComponent<Collider>();
            bounds = collide.bounds;
            start_alpha = glow.color.a;
            glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, 0f);
            glow.enabled = true;
        }

        protected virtual void OnDestroy()
        {
            slot_list.Remove(this);
        }

        private void Start()
        {
            if (x < Slot.x_min || x > Slot.x_max)
                Debug.LogError("Board Slot X and Y value must be within the min and max set for those values, check Slot.cs script to change those min/max.");
            if(!GetSlot().IsValid())
                Debug.LogError("Slot invalid: " + x + " " + enemy);

        }

        protected virtual void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            Slot slot = GetSlot();
            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter active = battle.GetActiveCharacter();
            HandCard dcard = HandCard.GetDrag();
            CardData icard = dcard != null ? dcard.GetCard().CardData : null;

            float target_alpha = 0f;
            if (active != null && active.player_id == GameClient.Get().GetPlayerID())
            {
                if (icard != null && icard.IsRequireTarget() && battle.CanPlayCard(dcard.GetCard(), slot))
                    target_alpha = 1f;
                if (icard == null && is_hover && battle.CanMoveCharacter(active, slot))
                    target_alpha = 1f;
            }

            current_alpha = Mathf.MoveTowards(current_alpha, target_alpha * start_alpha, 2f * Time.deltaTime);
            glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, current_alpha);
        }

        //When clicking on the slot
        public void OnMouseDown()
        {
            if (BattleUI.IsOverUI())
                return;

            Battle gdata = GameClient.Get().GetBattle();
            Player player = GameClient.Get().GetPlayer();
            Slot slot = GetSlot();

            if (gdata.selector == SelectorType.None && gdata.IsPlayerActionTurn(player.player_id))
            {
                BattleCharacter character = gdata.GetActiveCharacter();
                BattleCharacter slot_character = gdata.GetSlotCharacter(slot);
                if (slot_character == null)
                {
                    GameClient.Get().MoveCharacter(character, slot);
                }
            }

            if (gdata.selector == SelectorType.SelectTarget && gdata.IsPlayerSelectorTurn(player.player_id))
            {
                BattleCharacter slot_character = gdata.GetSlotCharacter(slot);
                if (slot_character != null)
                {
                    GameClient.Get().SelectCharacter(slot_character);
                }
                else
                {
                    GameClient.Get().SelectSlot(slot);
                }
            }
        }

        void OnMouseEnter()
        {
            is_hover = true;
        }

        void OnMouseExit()
        {
            is_hover = false;
        }

        void OnDisable()
        {
            is_hover = false;
        }

        //Find the actual slot coordinates of this board slot
        public virtual Slot GetSlot()
        {
            return new Slot(x, enemy);
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
            BoardSlot nearest = GetNearest(pos, range);
            return nearest == this;
        }

        public static BoardSlot GetNearest(Vector3 pos, float range = 999f)
        {
            BoardSlot nearest = null;
            float min_dist = range;
            foreach (BoardSlot slot in GetAll())
            {
                float dist = (slot.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = slot;
                }
            }
            return nearest;
        }

        public static BoardSlot GetMouseRaycast(float range = 999f)
        {
            Ray ray = GameCamera.GetCamera().ScreenPointToRay(Input.mousePosition);
            foreach (BoardSlot bslot in GetAll())
            {
                if (bslot.collide != null && bslot.collide.Raycast(ray, out RaycastHit hit, range))
                    return bslot;
            }
            return null;
        }

        public static BoardSlot Get(Slot slot)
        {
            foreach (BoardSlot bslot in GetAll())
            {
                if (bslot.HasSlot(slot))
                    return bslot;
            }
            return null;
        }

        public static List<BoardSlot> GetAll()
        {
            return slot_list;
        }

    }
}