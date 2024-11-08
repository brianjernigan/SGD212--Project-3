using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

// Handles internal deck logic
public class Deck
{
    private List<Card> _currentDeck;

    public Deck(Dictionary<Card, int> deckComposition)
    {
        _currentDeck = new List<Card>();
        InitializeDeck(deckComposition);
    }

    private void InitializeDeck(Dictionary<Card, int> deckComposition)
    {
        foreach (var entry in deckComposition)
        {
            for (var i = 0; i < entry.Value; i++)
            {
                _currentDeck.Add(entry.Key);
            }
        }

        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        for (var i = _currentDeck.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (_currentDeck[i], _currentDeck[j]) = (_currentDeck[j], _currentDeck[i]);
        }
    }

    public Card DrawCard()
    {
        if (_currentDeck.Count == 0) return null;

        var drawnCard = _currentDeck[0];
        _currentDeck.RemoveAt(0);
        return drawnCard;
    }

    public bool DeckIsEmpty()
    {
        return _currentDeck.Count == 0;
    }

    public int RemainingCardsInDeck()
    {
        return _currentDeck.Count;
    }
}
