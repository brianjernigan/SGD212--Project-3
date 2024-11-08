using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all visual aspects of the deck and game
public class DeckManager : MonoBehaviour
{
    [SerializeField] private List<CardData> _allCards;

    private Deck _currentDeck;
    public Deck CurrentDeck => _currentDeck;
    
    private void Start()
    {
        var defaultDeckComposition = new Dictionary<CardData, int>
        {
            // Card, Count
            { _allCards[0], 4 }, // 4 aces
            { _allCards[1], 4 }, // 4 twos
            { _allCards[2], 4 }, // 4 threes
            { _allCards[3], 4 }, // 4 fours
            { _allCards[4], 4 }, // 4 fives
            { _allCards[5], 4 }, // 4 sixes
            { _allCards[6], 4 }, // 4 sevens
            { _allCards[7], 4 }, // 4 eights
            { _allCards[8], 4 }, // 4 nines
            { _allCards[9], 4 }, // 4 tens
        };

        _currentDeck = new Deck(defaultDeckComposition);
    }
}
