using System.Collections.Generic;

namespace HunterScripts
{
    public class HunterHand
    {
        private List<HunterCardData> cardsInHand;
        private HunterDeck deck;

        public HunterHand(HunterDeck deck)
        {
            this.deck = deck;
            cardsInHand = new List<HunterCardData>();
        }

        public bool AddCard(HunterCardData card)
        {
            if (cardsInHand.Count >= 5)
                return false;

            cardsInHand.Add(card);
            return true;
        }

        public void RemoveCard(HunterCardData card)
        {
            if (cardsInHand.Contains(card))
            {
                cardsInHand.Remove(card);
            }
        }

        public bool IsFull()
        {
            return cardsInHand.Count >= 5;
        }

        public List<HunterCardData> GetCards()
        {
            return cardsInHand;
        }
    }
}
