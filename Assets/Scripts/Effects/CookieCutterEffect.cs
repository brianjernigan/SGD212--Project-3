using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieCutterEffect : ICardEffect
{
    public string EffectDescription =>
        "Discards all higher ranked and unranked cards in hand. Draws 1 card for each card discarded. Discards this card.";
    
    public void ActivateEffect()
    {
        var cardRank = GameManager.Instance.StageAreaController.GetFirstStagedCard().Data.CardRank;
        var discardCount = 0;

        var cardsInHand = GameManager.Instance.PlayerHand.CardsInHand;

        for (var i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].Data.CardRank > cardRank || cardsInHand[i].Data.Type == CardType.Unranked)
            {
                GameManager.Instance.PlayerHand.TryDiscardCardFromHand(cardsInHand[i]);
                discardCount++;
            }
        }

        for (var i = 0; i < discardCount; i++)
        {
            var drawnCard = GameManager.Instance.GameDeck?.DrawCard();
            if (drawnCard is null) return;

            GameManager.Instance.PlayerHand.TryAddCardToHand(drawnCard);
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea();
    }
}
