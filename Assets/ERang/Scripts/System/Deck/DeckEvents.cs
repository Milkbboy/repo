using System;

namespace ERang
{
    public class DeckEvents
    {
        public event Action<DeckCountInfo> OnCardCountChanged;
        public event Action<BaseCard> OnCardAdded;
        public event Action<BaseCard> OnCardRemoved;

        public void TriggerCardCountChanged(DeckCountInfo countInfo)
        {
            OnCardCountChanged?.Invoke(countInfo);
        }

        public void TriggerCardAdded(BaseCard card)
        {
            OnCardAdded?.Invoke(card);
        }

        public void TriggerCardRemoved(BaseCard card)
        {
            OnCardRemoved?.Invoke(card);
        }
    }

    public class DeckCountInfo
    {
        public int DeckCardCount { get; set; }
        public int HandCardCount { get; set; }
        public int GraveCardCount { get; set; }
        public int ExtinctionCardCount { get; set; }
    }
}
