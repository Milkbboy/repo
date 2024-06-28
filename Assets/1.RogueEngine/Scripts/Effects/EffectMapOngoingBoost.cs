using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Increase gold/xp earned by champions, in added percentage, 50 = +50%
    /// </summary>

    [CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/MapOngoingBoost", order = 10)]
    public class EffectMapOngoingBoost : EffectData
    {
        public MapOngoingBoostType type;

        public override void DoMapOngoingEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            if(type == MapOngoingBoostType.XPBonus)
                target.ongoing_xp_bonus += ability.GetValue() / 100f;
            if (type == MapOngoingBoostType.GoldBonus)
                target.ongoing_gold_bonus += ability.GetValue() / 100f;
            if (type == MapOngoingBoostType.ShopBuy)
                target.ongoing_buy_factor += ability.GetValue() / 100f;
            if (type == MapOngoingBoostType.ShopSell)
                target.ongoing_sell_factor += ability.GetValue() / 100f;
        }

    }

    public enum MapOngoingBoostType
    {
        None = 0,
        XPBonus = 10,
        GoldBonus = 15,
        ShopBuy = 20,
        ShopSell = 25,
    }
}