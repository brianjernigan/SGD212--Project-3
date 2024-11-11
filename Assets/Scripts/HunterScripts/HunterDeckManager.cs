using System.Collections.Generic;
using UnityEngine;

namespace HunterScripts
{
    public class HunterDeckManager : MonoBehaviour
    {
        public static HunterDeckManager Instance { get; private set; }
        public HunterDeck CurrentDeck { get; private set; }

        public delegate void CardDrawnHandler(HunterCardData drawnCard);
        public event CardDrawnHandler OnCardDrawn;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void InitializeDeck(List<HunterCardData> allPossibleCards)
        {
            Dictionary<HunterCardData, int> deckComposition = new Dictionary<HunterCardData, int>();

            foreach (var card in allPossibleCards)
            {
                deckComposition.Add(card, 1); // Adjust quantity as needed
            }

            CurrentDeck = new HunterDeck(deckComposition);
        }

        public void DrawCard()
        {
            if (CurrentDeck != null)
            {
                var drawnCard = CurrentDeck.DrawCard();
                OnCardDrawn?.Invoke(drawnCard);
            }
        }

        public void DiscardFromHand(HunterCardData card)
        {
            // Implement discard logic if necessary
        }
    }
}
