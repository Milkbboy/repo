using System.Collections;
using System.Collections.Generic;
using ERang.Data;

namespace ERang
{
    public interface IAbility
    {
        public AbilityType AbilityType => AbilityType.None;
        public List<(StatType statType, bool isAffect, int slot, int cardId, CardType cardType, int before, int after, int changeValue)> Changes { get; set; }

        IEnumerator ApplySingle(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);
        IEnumerator Release(CardAbility cardAbility, BSlot selfSlot, BSlot targetSlot);
    }
}