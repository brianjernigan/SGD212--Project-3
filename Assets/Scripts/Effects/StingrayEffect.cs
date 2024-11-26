using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingrayEffect : ICardEffect
{
    public string EffectDescription => "Shuffles this card and your entire hand back into the deck. Redraws a full hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var stageAreaController = GameManager.Instance.StageAreaController;
        var thisCard = stageAreaController.GetFirstStagedCard();

        GameManager.Instance.GameDeck.AddCard(thisCard.Data);
        stageAreaController.ClearStageArea();
        
        for (var i = 0; i < playerHand.CardsInHand.Count; i++)
        {
            GameManager.Instance.GameDeck?.AddCard(playerHand.CardsInHand[i].Data);
        }
        
        playerHand.ClearHandArea();
        
        GameManager.Instance.GameDeck?.ShuffleDeck();
        GameManager.Instance.DrawFullHand();
    }
}
