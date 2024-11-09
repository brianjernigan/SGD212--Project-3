using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all visual aspects and interactions of the deck and game
public class DeckManager : MonoBehaviour
{
    private Dictionary<CardData, int> _defaultDeckConfiguration;
    public Deck CurrentDeck { get; private set; }

    public event Action<CardData> OnCardDrawn;

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

    public CardData DrawCard()
    {
        var card = CurrentDeck.DrawCardFromDeck();
        if (card != null)
        {
            OnCardDrawn?.Invoke(card);
        }
        return card;
    }
}
