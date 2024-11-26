using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcaEffect : ICardEffect
{
    public string EffectDescription => "Discards this card and your entire hand. Redraws a full hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;

        for (var i = 0; i < playerHand.NumCardsInHand; i++)
        {
            playerHand.TryDiscardCardFromHand(playerHand.CardsInHand[i]);
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea();

        for (var i = 0; i < GameManager.Instance.HandSize; i++)
        {
            var drawnCard = GameManager.Instance.GameDeck?.DrawCard();
            if (drawnCard is not null)
            {
                playerHand.TryAddCardToHand(drawnCard);
            }
        }
        
        GameManager.Instance.TriggerCardsRemainingChanged();
    }
}
