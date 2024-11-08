using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand
{
    private List<Card> _cardsInHand;
    private int _maxHandSize;

    public Hand(int initialHandSize)
    {
        _cardsInHand = new List<Card>();
        _maxHandSize = initialHandSize;
    }

    public bool CanAddCardToHand()
    {
        return _cardsInHand.Count < _maxHandSize;
    }

    public bool AddCardToHand(Card cardToAdd)
    {
        if (!CanAddCardToHand()) return false;
        _cardsInHand.Add(cardToAdd);
        return true;
    }

    public bool DiscardCardFromHand(Card cardToDiscard)
    {
        return _cardsInHand.Remove(cardToDiscard);
    }

    public List<Card> GetCardsInHand()
    {
        return new List<Card>(_cardsInHand);
    }

    public int GetCurrentHandSize()
    {
        return _cardsInHand.Count;
    }

    public bool HandIsFull()
    {
        return _cardsInHand.Count >= _maxHandSize;
    }

    public void SetMaxHandSize(int newMaxHandSize)
    {
        _maxHandSize = newMaxHandSize;
        
        // May want to discard far-right or far-left card if shrinking hand size
    }

    public int GetMaxHandSize()
    {
        return _maxHandSize;
    }
}
