using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using RogueEngine.Client;

namespace RogueEngine.FX
{
    /// <summary>
    /// All FX/anims related to a card on the board
    /// </summary>

    public class BoardCharacterFX : MonoBehaviour
    {
        public Transform fx_target;

        [Header("FX")]
        public GameObject spawn_fx;
        public GameObject death_fx;
        public AudioClip spawn_audio;
        public AudioClip death_audio;
        public string spawn_anim = "spawn";
        public string death_anim = "death";

        private BoardCharacter bcharacter;
        private SpriteRenderer render;
        private Animator animator;

        private Dictionary<StatusEffect, GameObject> status_fx_list = new Dictionary<StatusEffect, GameObject>();

        void Awake()
        {
            bcharacter = GetComponent<BoardCharacter>();
            render = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
            bcharacter.onKill += OnKill;
        }

        void Start()
        {
            GameClient client = GameClient.Get();
            client.onCharacterMoved += OnMove;
            client.onCharacterDamaged += OnDamaged;
            client.onCardPlayed += OnCardPlayed;
            client.onAbilityStart += OnAbilityStart;
            client.onAbilityTargetCharacter += OnAbilityEffect;
            client.onAbilityEnd += OnAbilityAfter;

            OnSpawn();
        }

        private void OnDestroy()
        {
            GameClient client = GameClient.Get();
            client.onCharacterMoved -= OnMove;
            client.onCharacterDamaged -= OnDamaged;
            client.onAbilityStart -= OnAbilityStart;
            client.onAbilityTargetCharacter -= OnAbilityEffect;
            client.onAbilityEnd -= OnAbilityAfter;
        }
        
        void Update()
        {
            if (!GameClient.Get().IsBattleReady())
                return;

            BattleCharacter card = bcharacter.GetCharacter();
            if (card == null)
                return;

            //Status FX
            List<CardStatus> status_all = card.GetAllStatus();
            foreach (CardStatus status in status_all)
            {
                StatusData istatus = StatusData.Get(status.id);
                if (istatus != null && !status_fx_list.ContainsKey(status.effect) && istatus.status_fx != null)
                {
                    GameObject fx = Instantiate(istatus.status_fx, transform);
                    fx.transform.localPosition = fx_target.localPosition;
                    status_fx_list[istatus.effect] = fx;
                    SetAnimationBool(istatus.animation, true);
                }
            }

            //Remove status FX
            List<StatusEffect> remove_list = new List<StatusEffect>();
            foreach (KeyValuePair<StatusEffect, GameObject> pair in status_fx_list)
            {
                if (!card.HasStatus(pair.Key))
                {
                    StatusData status = StatusData.Get(pair.Key);
                    SetAnimationBool(status.animation, false);
                    remove_list.Add(pair.Key);
                    Destroy(pair.Value);
                }
            }

            foreach (StatusEffect status in remove_list)
                status_fx_list.Remove(status);
        }

        private void OnSpawn()
        {
            //Spawn FX
            AudioTool.Get().PlaySFX("character", GetSpawnAudio());
            FXTool.DoFX(GetSpawnFX(), FXTransform);
            TriggerAnimation(spawn_anim);
        }

        private void OnKill()
        {
            StartCoroutine(KillRoutine());
        }

        private IEnumerator KillRoutine()
        {
            float time = Mathf.Min(bcharacter.death_delay, 0.4f);
            yield return new WaitForSeconds(time);

            TriggerAnimation(death_anim);
            FXTool.DoFX(GetDeathFX(), FXTransform);
            AudioTool.Get().PlaySFX("character", GetDeathAudio());
        }

        private void OnMove(BattleCharacter character, Slot slot)
        {
            
        }

        private void OnDamaged(BattleCharacter character, int damage)
        {
            if (bcharacter.GetUID() == character.uid)
            {
                DamageFX(FXTransform, damage);
            }
        }

        private void DamageFX(Transform target, int value, float delay = 0.3f)
        {
            TimeTool.WaitFor(delay, () =>
            {
                if (target != null)
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float range = Random.Range(0f, 0.5f);
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * range, Mathf.Sin(angle) * range, 0f);
                    GameObject fx = FXTool.DoFX(AssetData.Get().damage_fx, target.position + offset);
                    fx.GetComponent<DamageFX>().SetValue(value);
                }
            });
        }

        private void OnCardPlayed(Card card, Slot slot)
        {
            Battle battle = GameClient.Get().GetBattle();
            BattleCharacter character = battle.GetCharacter(card.owner_uid);
            if (character != null && bcharacter.GetUID() == card.owner_uid)
            {
                TriggerAnimation(card.CardData.caster_anim);
            }
        }

        private void OnAbilityStart(AbilityData iability, Card card)
        {
            if (iability != null && card != null)
            {
                Battle battle = GameClient.Get().GetBattle();
                BattleCharacter character = battle.GetCharacter(card.owner_uid);
                if (character.uid == bcharacter.GetUID())
                {
                    FXTool.DoSnapFX(iability.caster_fx, bcharacter.transform);
                    AudioTool.Get().PlaySFX("ability", iability.cast_audio);
                    TriggerAnimation(iability.caster_anim);
                }
            }
        }

        private void OnAbilityAfter(AbilityData iability, Card card)
        {
            if (iability != null && card != null)
            {
                Battle battle = GameClient.Get().GetBattle();
                BattleCharacter character = battle.GetCharacter(card.owner_uid);
                if (character.uid == bcharacter.GetUID())
                {

                }
            }
        }

        private void OnAbilityEffect(AbilityData iability, Card card, BattleCharacter target)
        {
            if (iability != null && card != null && target != null)
            {
                if (target.uid == bcharacter.GetUID())
                {
                    FXTool.DoSnapFX(iability.target_fx, FXTransform, Vector3.up * 0.5f);
                    AudioTool.Get().PlaySFX("ability_effect", iability.target_audio);

                    TriggerAnimation(card.CardData.target_anim);
                    TriggerAnimation(iability.target_anim);
                }

            }
        }

        private void TriggerAnimation(string anim_id)
        {
            if (animator != null && HasAnimParam(anim_id))
            {
                animator.SetTrigger(anim_id);
            }
        }

        private void SetAnimationBool(string anim_id, bool value)
        {
            if (animator != null && HasAnimParam(anim_id))
            {
                animator.SetBool(anim_id, value);
            }
        }

        private bool HasAnimParam(string anim_id)
        {
            if (string.IsNullOrEmpty(anim_id))
                return false;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == anim_id)
                    return true;
            }
            return false;
        }

        private AudioClip GetSpawnAudio()
        {
            CharacterData character = bcharacter.GetCharacter()?.CharacterData;
            ChampionData champion = bcharacter.GetCharacter()?.ChampionData;

            if (character != null && character.spawn_audio != null)
                return character.spawn_audio;
            if (champion != null && spawn_audio != null)
                return spawn_audio;
            return AssetData.Get().character_spawn_audio;
        }

        private AudioClip GetDeathAudio()
        {
            CharacterData character = bcharacter.GetCharacter()?.CharacterData;
            ChampionData champion = bcharacter.GetCharacter()?.ChampionData;

            if (character != null && character.death_audio != null)
                return character.death_audio;
            if (champion != null && death_audio != null)
                return death_audio;
            return AssetData.Get().character_destroy_audio;
        }

        private GameObject GetSpawnFX()
        {
            CharacterData character = bcharacter.GetCharacter()?.CharacterData;
            ChampionData champion = bcharacter.GetCharacter()?.ChampionData;

            if (character != null && character.spawn_fx != null)
                return character.spawn_fx;
            if (champion != null && spawn_fx != null)
                return spawn_fx;
            return AssetData.Get().character_spawn_fx;
        }

        private GameObject GetDeathFX()
        {
            CharacterData character = bcharacter.GetCharacter()?.CharacterData;
            ChampionData champion = bcharacter.GetCharacter()?.ChampionData;

            if (character != null && character.death_fx != null)
                return character.death_fx;
            if (champion != null && death_fx != null)
                return death_fx;
            return AssetData.Get().character_destroy_fx;
        }

        private Transform FXTransform
        {
            get {
                if (fx_target != null)
                    return fx_target;
                return transform;
            }
        }
    }
}
