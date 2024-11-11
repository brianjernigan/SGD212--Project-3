using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines data and structure of a player's hand
public class Hand
{
    private readonly List<GameCard> _cardsInHand = new();
    public int NumCardsInHand => _cardsInHand.Count;

    public bool TryAddCardToHand(GameCard gameCard)
    {
        if (gameCard is null) return false;
        _cardsInHand.Add(gameCard);
        return true;
    }

    public bool TryRemoveCardFromHand(GameCard gameCard)
    {
        if (gameCard is null || !_cardsInHand.Contains(gameCard)) return false;

        _cardsInHand.Remove(gameCard);
        return true;
    }

    public IEnumerable<GameCard> GetCardsInHand()
    {
        return _cardsInHand;
    }

    public void ClearHand()
    {
        foreach (var gameCard in _cardsInHand)
        {
            if (gameCard.UI is not null)
            {
                Object.Destroy(gameCard.UI.gameObject);
            }
        }
        
        _cardsInHand.Clear();
    }
}
