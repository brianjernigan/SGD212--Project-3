using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;


namespace HunterScripts
{
    public class HunterDeck
    {
        private List<HunterCardData> currentDeck;

        public HunterDeck(Dictionary<HunterCardData, int> deckComposition)
        {
            currentDeck = new List<HunterCardData>();
            InitializeDeck(deckComposition);
        }

        private void InitializeDeck(Dictionary<HunterCardData, int> deckComposition)
        {
            foreach (var entry in deckComposition)
            {
                for (int i = 0; i < entry.Value; i++)
                {
                    currentDeck.Add(entry.Key);
                }
            }

            ShuffleDeck();
        }

        public void ShuffleDeck()
        {
            for (int i = currentDeck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = currentDeck[i];
                currentDeck[i] = currentDeck[j];
                currentDeck[j] = temp;
            }
        }

        public HunterCardData DrawCard()
        {
            if (currentDeck.Count == 0)
            {
                Debug.LogWarning("Deck is empty!");
                return null;
            }

            HunterCardData drawnCard = currentDeck[0];
            currentDeck.RemoveAt(0);
            return drawnCard;
        }

        public List<HunterCardData> DrawMultipleCards(int numberOfCards)
        {
            List<HunterCardData> drawnCards = new List<HunterCardData>();

            for (int i = 0; i < numberOfCards; i++)
            {
                HunterCardData card = DrawCard();
                if (card != null)
                {
                    drawnCards.Add(card);
                }
                else
                {
                    break; // Stop drawing if deck is empty
                }
            }

            return drawnCards;
        }

        public bool IsEmpty()
        {
            return currentDeck.Count == 0;
        }

        public int RemainingCards()
        {
            return currentDeck.Count;
        }
    }
}