using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingrayEffect : ICardEffect
{
    public string EffectDescription => "Shuffle this card and your entire hand back into the deck. Redraw a full hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var stageAreaController = GameManager.Instance.StageAreaController;
        var thisCard = stageAreaController.GetFirstStagedCard();

        GameManager.Instance.GameDeck.AddCard(thisCard.Data);
        stageAreaController.TryRemoveCardFromStage(thisCard);
        
        for (var i = 0; i < playerHand.CardsInHand.Count; i++)
        {
            GameManager.Instance.GameDeck?.AddCard(playerHand.CardsInHand[i].Data);
        }
        
        playerHand.ClearHandArea();
        stageAreaController.ClearStageArea();
        
        GameManager.Instance.GameDeck?.ShuffleDeck();
        GameManager.Instance.DrawFullHand();
    }
}
