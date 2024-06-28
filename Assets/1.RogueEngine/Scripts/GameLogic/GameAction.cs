
using Unity.Netcode;
using UnityEngine.Events;

namespace RogueEngine
{
    /// <summary>
    /// List of game actions and refreshes, that can be performed by the player or received
    /// </summary>

    public static class GameAction
    {
        public const ushort None = 0;

        //Initialization
        public const ushort NewScenario = 100;
        public const ushort LoadScenario = 110;
        public const ushort CreateChampion = 120;
        public const ushort DeleteChampion = 122;
        public const ushort SendUserData = 130;
        public const ushort StartGame = 150;
        public const ushort StartTest = 155;

        //Generic refresh
        public const ushort Connected = 200;
        public const ushort ServerMessage = 220;
        public const ushort RefreshWorld = 250;
        public const ushort RefreshBattle = 260;

        //Map Commands (client to server)
        public const ushort MapMove = 300;
        public const ushort MapEventDone = 310;
        public const ushort MapEventDoneChampion = 311;

        public const ushort MapEventChoice = 320;
        public const ushort MapRewardCardChoice = 322;
        public const ushort MapRewardItemChoice = 323;
        public const ushort MapUpgradeCard = 324;
        public const ushort MapLevelUp = 326;
        public const ushort MapTrashCard = 328;

        public const ushort ShopBuyCard = 340;
        public const ushort ShopSellCard = 341;
        public const ushort ShopBuyItem = 342;
        public const ushort ShopSellItem = 343;

        public const ushort GainChampion = 350;
        public const ushort RemoveChampion = 352;
        public const ushort GainAlly = 354;
        public const ushort RemoveAlly = 356;

        //Map Refresh (server to client)
        public const ushort GameStarted = 500;
        public const ushort GameEnded = 502;
        public const ushort MapMoved = 510;
        public const ushort MapRewardSelected = 520;
        public const ushort MapChoiceSelected = 522;

        //Battle Commands (client to server)
        public const ushort PlayCard = 1000;
        public const ushort Attack = 1010;
        public const ushort AttackPlayer = 1012;
        public const ushort Move = 1015;
        public const ushort CastAbility = 1020;
        public const ushort UseItem = 1025;
        public const ushort SelectCard = 1030;
        public const ushort SelectCharacter = 1032;
        public const ushort SelectSlot = 1034;
        public const ushort SelectChoice = 1036;
        public const ushort CancelSelect = 1039;
        public const ushort EndTurn = 1040;
        public const ushort Resign = 1050;
        public const ushort ChatMessage = 1090;

        //Battle Refresh (server to client)
        public const ushort BattleStarted = 2010;
        public const ushort BattleEnded = 2012;
        public const ushort NewTurn = 2015;

        public const ushort CardPlayed = 2020;
        public const ushort CardSummoned = 2022;
        public const ushort CardTransformed = 2023;
        public const ushort CardDiscarded = 2025;
        public const ushort CardDrawn = 2026;

        public const ushort CharacterMoved = 2030;
        public const ushort CharacterDamaged = 2032;
        public const ushort ItemUsed = 2035;

        public const ushort AbilityTrigger = 2040;
        public const ushort AbilityTargetCard = 2042;
        public const ushort AbilityTargetCharacter = 2043;
        public const ushort AbilityTargetSlot = 2044;
        public const ushort AbilityEnd = 2048;

        public const ushort SecretTriggered = 2060;
        public const ushort SecretResolved = 2061;
        public const ushort ValueRolled = 2070;

        public static string GetString(ushort type)
        {
            if (type == GameAction.PlayCard)
                return "play";
            if (type == GameAction.Move)
                return "move";
            if (type == GameAction.Attack)
                return "attack";
            if (type == GameAction.AttackPlayer)
                return "attack_player";
            if (type == GameAction.CastAbility)
                return "cast_ability";
            if (type == GameAction.EndTurn)
                return "end_turn";
            if (type == GameAction.SelectCard)
                return "select_card";
            if (type == GameAction.SelectCharacter)
                return "select_player";
            if (type == GameAction.SelectChoice)
                return "select_choice";
            if (type == GameAction.SelectSlot)
                return "select_slot";
            if (type == GameAction.CancelSelect)
                return "cancel_select";
            if (type == GameAction.Resign)
                return "resign";
            if (type == GameAction.ChatMessage)
                return "chat";
            return type.ToString();
        }
    }
}