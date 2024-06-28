using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Client;
using RogueEngine.UI;

namespace RogueEngine.FX
{
    /// <summary>
    /// FX that are not related to any card/player, and appear in the middle of the board
    /// Usually when big abilities are played
    /// </summary>

    public class GameBoardFX : MonoBehaviour
    {
        void Start()
        {
            GameClient client = GameClient.Get();
            client.onNewTurn += OnNewTurn;
            client.onCardPlayed += OnPlayCard;
            client.onAbilityStart += OnAbility;
            client.onSecretTrigger += OnSecret;
            client.onValueRolled += OnRoll;
        }

        private void OnDestroy()
        {
            GameClient client = GameClient.Get();
            if (client != null)
            {
                client.onNewTurn -= OnNewTurn;
                client.onCardPlayed -= OnPlayCard;
                client.onAbilityStart -= OnAbility;
                client.onSecretTrigger -= OnSecret;
                client.onValueRolled -= OnRoll;
            }
        }

        private void Update()
        {

        }

        void OnNewTurn()
        {
            AudioTool.Get().PlaySFX("turn", AssetData.Get().new_turn_audio);
            FXTool.DoFX(AssetData.Get().new_turn_fx, Vector3.zero);
        }

        void OnPlayCard(Card card, Slot target)
        {
            if (card != null)
            {
                CardData icard = CardData.Get(card.card_id);
                if (icard.card_type == CardType.Skill)
                {
                    GameObject prefab = AssetData.Get().play_card_fx;
                    Vector3 pos = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
                    GameObject obj = FXTool.DoFX(prefab, pos);
                    CardUI ui = obj.GetComponentInChildren<CardUI>();

                    Battle battle = GameClient.Get().GetBattle();
                    BattleCharacter character = battle.GetCharacter(card.owner_uid);
                    ui.SetCard(character, card);

                    AudioClip spawn_audio = icard.spawn_audio != null ? icard.spawn_audio : AssetData.Get().character_spawn_audio;
                    AudioTool.Get().PlaySFX("card_spell", spawn_audio);
                }
            }
        }

        private void OnAbility(AbilityData iability, Card caster)
        {
            if (iability != null)
            {
                FXTool.DoFX(iability.board_fx, Vector3.zero);
            }
        }

        private void OnSecret(Card secret, Card triggerer)
        {
            
        }

        private void OnRoll(int value)
        {
            GameObject fx = FXTool.DoFX(AssetData.Get().dice_roll_fx, Vector3.zero);
            DiceRollFX dice = fx?.GetComponent<DiceRollFX>();
            if (dice != null)
            {
                dice.value = value;
            }
        }

    }
}