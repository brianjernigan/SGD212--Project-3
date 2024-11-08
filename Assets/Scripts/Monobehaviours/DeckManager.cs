using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all visual aspects of the deck and game
public class DeckManager : MonoBehaviour
{
    private Dictionary<CardData, int> _defaultDeckConfiguration;
    public Deck CurrentDeck { get; private set; }

    private void Awake()
    {
        InitializeDecks();
        CurrentDeck = new Deck(_defaultDeckConfiguration);
    }

    private void InitializeDecks()
    {
        _defaultDeckConfiguration = new Dictionary<CardData, int>();
        var allCards = GameManager.Instance.AllPossibleCards;

        foreach (var card in allCards)
        {
            _defaultDeckConfiguration.Add(card, 4);
        }
    }
}
