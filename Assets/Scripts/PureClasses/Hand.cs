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
        CardsInHand.Add(gameCard);
        gameCard.IsInHand = true;
        gameCard.IsStaged = false;
        GameManager.Instance.RearrangeHand();
        return true;
    }

    // Does NOT destroy card object
    public bool TryRemoveCardFromHand(GameCard gameCard)
    {
        if (CardsInHand.Contains(gameCard) && CardsInHand.Remove(gameCard))
        {
            gameCard.IsInHand = false;
            GameManager.Instance.RearrangeHand();
            return true;
        }

        return false;
    }

    // Destroys card object
    public bool TryDiscardCardFromHand(GameCard gameCard)
    {
        if (CardsInHand.Contains(gameCard) && CardsInHand.Remove(gameCard))
        {
            gameCard.IsInHand = false;
            GameManager.Instance.RearrangeHand();
            GameManager.Instance.FullDiscard(gameCard, true);
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
                gameCard.IsInHand = false;
                Object.Destroy(gameCard.UI.gameObject);
            }
        }
        
        CardsInHand.Clear();
    }
}
