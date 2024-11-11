using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Defines data and structure of a player's hand
public class Hand
{
    private readonly List<GameCard> _cardsInHand = new();
    public List<GameCard> CardsInHand => _cardsInHand;
    public int NumCardsInHand => _cardsInHand.Count;

    public bool TryAddCardToHand(GameCard gameCard)
    {
        if (gameCard is null) return false;
        _cardsInHand.Add(gameCard);
        return true;
    }

    // Does NOT destroy card object
    public bool TryRemoveCardFromHand(GameCard gameCard)
    {
        return _cardsInHand.Contains(gameCard) && _cardsInHand.Remove(gameCard);
    }

    // Destroys card objects
    public bool TryDiscardCardFromHand(GameCard gameCard)
    {
        if (gameCard is null || !_cardsInHand.Contains(gameCard)) return false;

        _cardsInHand.Remove(gameCard);
        Object.Destroy(gameCard.UI.gameObject);
        return true;
    }

    public void ClearHandArea()
    {
        foreach (var gameCard in _cardsInHand.ToList())
        {
            if (gameCard.UI is not null)
            {
                Object.Destroy(gameCard.UI.gameObject);
            }
        }
        
        _cardsInHand.Clear();
    }
}
