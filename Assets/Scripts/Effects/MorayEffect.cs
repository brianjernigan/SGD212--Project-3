using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MorayEffect : ICardEffect
{
    public string EffectDescription =>
        "Discards all cards that are unable to form a set (2 or less remaining). Discards this card.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var gameDeck = GameManager.Instance.GameDeck;

        var deckDictionary = gameDeck.CardDataInDeck.GroupBy(card => card)
            .ToDictionary(group => group.Key, group => group.Count());

        var discardedCardsFromHand = new List<GameCard>();
        var discardedCardsFromDeck = new List<CardData>();

        foreach (var card in playerHand.CardsInHand.ToList())
        {
            if (deckDictionary.TryGetValue(card.Data, out var count) && count <= 2)
            {
                discardedCardsFromHand.Add(card);
                playerHand.TryDiscardCardFromHand(card);
            }
        }

        foreach (var cardData in gameDeck.CardDataInDeck.ToList())
        {
            if (deckDictionary.TryGetValue(cardData, out var count) && count <= 2)
            {
                discardedCardsFromDeck.Add(cardData);
                gameDeck.RemoveCard(cardData);
            }
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea();
    }
}
