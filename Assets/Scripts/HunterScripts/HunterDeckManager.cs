using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;


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
            Dictionary<HunterCardData, int> deckComposition = new Dictionary<HunterCardData, int>();

            foreach (var card in allPossibleCards)
            {
                deckComposition[card] = 1; // Add each card with a quantity of 1
            }

            CurrentDeck = new HunterDeck(deckComposition);
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
                Debug.LogWarning("Deck is empty!");
                return;
            }

            // Invoke the event to notify that a card has been drawn
            OnCardDrawn?.Invoke(drawnCard);
        }
    }
}
