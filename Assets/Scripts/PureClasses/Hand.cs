using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Defines data and structure of a player's hand
public class Hand
{
    public List<GameCard> CardsInHand { get; } = new();
    public int NumCardsInHand => CardsInHand.Count;

    public bool TryAddCardToHand(GameCard gameCard)
    {
        if (gameCard is null) return false;
        CardsInHand.Add(gameCard);
        return true;
    }

    // Does NOT destroy card object
    public bool TryRemoveCardFromHand(GameCard gameCard)
    {
        if (gameCard is null) return false;

        if (CardsInHand.Contains(gameCard) && CardsInHand.Remove(gameCard))
        {
            GameManager.Instance.RearrangeHand();
            return true;
        }

        return false;
    }

    // Destroys card objects
    public bool TryDiscardCardFromHand(GameCard gameCard)
    {
        if (gameCard is null) return false;

        if (CardsInHand.Contains(gameCard) && CardsInHand.Remove(gameCard))
        {
            Object.Destroy(gameCard.UI.gameObject);
            GameManager.Instance.RearrangeHand();
            return true;
        }

        return false;
    }

    public void ClearHandArea()
    {
        foreach (var gameCard in CardsInHand.ToList())
        {
            if (gameCard.UI is not null)
            {
                Object.Destroy(gameCard.UI.gameObject);
            }
        }
        
        CardsInHand.Clear();
    }
}
