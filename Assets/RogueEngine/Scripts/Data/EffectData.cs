using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RogueEngine.Gameplay;

namespace RogueEngine
{
    /// <summary>
    /// Base class for all ability effects, override the IsConditionMet function
    /// </summary>
    
    public class EffectData : ScriptableObject
    {
        public virtual void DoMapEventEffect(WorldLogic logic, EventEffect evt, Champion champion)
        {
            //Effect for map events
        }

        public virtual void DoMapEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            //Ongoing effect for map events (like passive items with out-of-battle effects)
        }

        public virtual void DoMapOngoingEffect(WorldLogic logic, AbilityData ability, Champion champion, ChampionItem item, Champion target)
        {
            //Ongoing effect for map events (like passive items with out-of-battle effects)
        }

        public virtual void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card)
        {
            //Effect for abilities with no target
        }

        public virtual void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, Card target)
        {
            //Effect for abilities targeting a card
        }

        public virtual void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, BattleCharacter target)
        {
            //Effect for abilities targeting a character
        }

        public virtual void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, Slot target)
        {
            //Effect for abilities targeting a Slot
        }

        public virtual void DoEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, CardData target)
        {
            //Effect for abilities targeting a CardData
        }

        public virtual void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, Card target)
        {
            //Effect for Ongoing abilities targeting a card
        }

        public virtual void DoOngoingEffect(BattleLogic logic, AbilityData ability, BattleCharacter character, Card card, BattleCharacter target)
        {
            //Effect for Ongoing abilities targeting a character
        }
    }
}