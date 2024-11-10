using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Creates and initializes the deck(s)
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
        // Default deck has 4 of every card, could create different decks if in scope
        _defaultDeckConfiguration = new Dictionary<CardData, int>();
        var allCards = GameManager.Instance.AllPossibleCards;

        foreach (var card in allCards)
        {
            _defaultDeckConfiguration.Add(card, 4);
        }
    }
}
