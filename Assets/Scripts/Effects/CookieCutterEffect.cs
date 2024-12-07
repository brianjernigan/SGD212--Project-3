using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieCutterEffect : ICardEffect
{
    public string EffectDescription =>
        "Discard this card, all higher ranked cards, and all unranked cards in hand. Next turn, draw 1 additional card for each card discarded.";
    
    public void ActivateEffect()
    {
        var cardRank = GameManager.Instance.StageAreaController.GetFirstStagedCard().Data.CardRank;
        var discardCount = 0;

        var cardsInHand = GameManager.Instance.PlayerHand.CardsInHand;

        for (var i = cardsInHand.Count - 1; i >= 0; i--)
        {
            if (cardsInHand[i].Data.CardRank > cardRank || cardsInHand[i].Data.Type == CardType.Unranked)
            {
                GameManager.Instance.PlayerHand.TryDiscardCardFromHand(cardsInHand[i]);
                discardCount++;
            }
        }

        GameManager.Instance.AdditionalCardsDrawn += discardCount + 1;
        GameManager.Instance.TriggerHandSizeChanged();
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
    }
}
