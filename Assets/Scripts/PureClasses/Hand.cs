using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines data and structure of a player's hand
public class Hand
{
    private const int BaseMaxHandSize = 5;
    private readonly List<CardData> _cardsInHand;
    private Deck _currentDeck;
    private int _currentMaxHandSize;

    public Hand(Deck currentDeck)
    {
        _cardsInHand = new List<CardData>();
        _currentDeck = currentDeck;
        _currentMaxHandSize = BaseMaxHandSize;
    }

    public bool AddCardToHand(CardData cardDataToAdd)
    {
        if (IsFull()) return false;
        _cardsInHand.Add(cardDataToAdd);
        return true;
    }

    public bool DiscardCardFromHand(CardData cardDataToDiscard)
    {
        return _cardsInHand.Remove(cardDataToDiscard);
    }

    public List<CardData> GetCardsInHand()
    {
        return new List<CardData>(_cardsInHand);
    }

    public int GetCurrentHandSize()
    {
        return _cardsInHand.Count;
    }
    
    public bool IsFull()
    {
        return _cardsInHand.Count >= _currentMaxHandSize;
    }

    public void SetMaxHandSize(int newMaxHandSize)
    {
        _currentMaxHandSize = newMaxHandSize;
        
        // May want to discard far-right or far-left card if shrinking hand size
    }

    public int GetMaxHandSize()
    {
        return _currentMaxHandSize;
    }
}
