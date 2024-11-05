using System.Collections;
using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    public interface IAbility
    {
        public AbilityType AbilityType => AbilityType.None;
        public List<(bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)> Changes { get; set; }

        IEnumerator Apply(AiData aiData, AbilityData abilityData, BSlot selfSlot, List<BSlot> targetSlots);
        IEnumerator Release(Ability ability, BSlot selfSlot, BSlot targetSlot);
    }
}