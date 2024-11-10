using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines data and structure of a player's hand
public class Hand
{
    private const int MaxHandSize = 5;
    private readonly List<GameCard> _cardsInHand = new();
    private int NumCardsInHand => _cardsInHand.Count;
    public bool IsFull => NumCardsInHand >= MaxHandSize;

    public void TryAddCardToHand(GameCard gameCard)
    {
        if (gameCard is null) return;
        _cardsInHand.Add(gameCard);
    }

    public void RemoveCardFromHand(GameCard gameCard)
    {
        if (gameCard is null || !_cardsInHand.Contains(gameCard)) return;

        _cardsInHand.Remove(gameCard);
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
    }
}
