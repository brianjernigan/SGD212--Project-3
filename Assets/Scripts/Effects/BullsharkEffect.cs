using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BullsharkEffect : ICardEffect
{
    public string EffectDescription => "Discards all Bullsharks. Permanently increase your hand size by 1 this round.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var playerHandCards = playerHand.CardsInHand;
        var gameDeck = GameManager.Instance.GameDeck;
        var deckData = GameManager.Instance.GameDeck.CardDataInDeck;
        
        var bullsharkData = CardLibrary.Instance.GetCardDataByName("Bullshark");
        if (bullsharkData is null) return;

        var discardedFromHand = new List<GameCard>();
        var discardedFromDeck = new List<CardData>();

        foreach (var gameCard in playerHandCards.ToList())
        {
            if (gameCard.Data == bullsharkData)
            {
                discardedFromHand.Add(gameCard);
                playerHand.TryDiscardCardFromHand(gameCard);
            }
        }

        foreach (var cardData in deckData.ToList())
        {
            if (cardData == bullsharkData)
            {
                discardedFromDeck.Add(cardData);
                gameDeck.RemoveCard(cardData);
            }
        }

        if (discardedFromDeck.Count == 0 && discardedFromHand.Count == 0) return;
        
        GameManager.Instance.StageAreaController.ClearStageArea();

        GameManager.Instance.PermanentHandSizeModifier += 1;
        GameManager.Instance.TriggerHandSizeChanged();
    }
}
