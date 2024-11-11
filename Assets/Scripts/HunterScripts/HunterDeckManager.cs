using System.Collections.Generic;
using UnityEngine;

namespace HunterScripts
{
    public class HunterDeckManager : MonoBehaviour
    {
        public static HunterDeckManager Instance { get; private set; }
        public HunterDeck CurrentDeck { get; private set; }

        public List<HunterCardData> allPossibleCards; // Assign this in the Inspector

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
            if (allPossibleCards == null || allPossibleCards.Count == 0)
            {
                Debug.LogWarning("InitializeDeck: allPossibleCards is empty or null. Please assign cards in the Inspector.");
                return;
            }

            Dictionary<HunterCardData, int> deckComposition = new Dictionary<HunterCardData, int>();

            foreach (var card in allPossibleCards)
            {
                deckComposition[card] = 1; // Add each card with a quantity of 1
            }

            CurrentDeck = new HunterDeck(deckComposition);
            Debug.Log($"Deck initialized with {CurrentDeck.RemainingCards()} cards.");
        }

        public void DrawCard()
        {
            if (CurrentDeck == null)
            {
                Debug.LogError("CurrentDeck is null. Initialize the deck before drawing.");
                return;
            }

            var drawnCard = CurrentDeck.DrawCard();

            if (drawnCard == null)
            {
                Debug.LogWarning("Attempted to draw a card, but the deck is empty!");
                return;
            }

            OnCardDrawn?.Invoke(drawnCard);
        }

        public void DiscardFromHand(HunterCardData card)
        {
            // Implement discard logic if necessary
        }
    }
}
