using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand
{
    private List<CardData> _cardsInHand;
    private int _maxHandSize;

    public Hand(int initialHandSize)
    {
        _cardsInHand = new List<CardData>();
        _maxHandSize = initialHandSize;
    }

    public bool AddCardToHand(CardData cardDataToAdd)
    {
        if (HandIsFull()) return false;
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
