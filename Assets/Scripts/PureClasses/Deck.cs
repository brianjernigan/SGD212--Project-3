using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

// Handles internal deck logic
public class Deck
{
    public List<CardData> CardsInDeck { get; }

    public Deck(Dictionary<CardData, int> deckComposition)
    {
        CardsInDeck = new List<CardData>();
        InitializeDeck(deckComposition);
    }

    private void InitializeDeck(Dictionary<CardData, int> deckComposition)
    {
        foreach (var entry in deckComposition)
        {
            for (var i = 0; i < entry.Value; i++)
            {
                CardsInDeck.Add(entry.Key);
            }
        }

        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        for (var i = CardsInDeck.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (CardsInDeck[i], CardsInDeck[j]) = (CardsInDeck[j], CardsInDeck[i]);
        }
    }

    public bool IsEmpty()
    {
        return CardsInDeck.Count == 0;
    }

    public int GetCardsRemaining()
    {
        return CardsInDeck.Count;
    }
}
