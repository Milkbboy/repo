using System.Collections;
using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    public interface IAbility
    {
        public AbilityType AbilityType => AbilityType.None;
        public List<(bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)> Changes { get; set; }

        IEnumerator Apply(AiData aiData, AbilityData abilityData, BoardSlot selfSlot, List<BoardSlot> targetSlots);
        IEnumerator Release(Ability ability, BoardSlot selfSlot, BoardSlot targetSlot);
    }
}