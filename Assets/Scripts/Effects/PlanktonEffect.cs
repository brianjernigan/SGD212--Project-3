using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanktonEffect : ICardEffect
{
    public string EffectDescription => "Draw 1 card to your hand. Add 1 Plankton to the deck.";
    
    public void ActivateEffect()
    {
        var planktonCard = CardLibrary.Instance.GetCardDataByName("Plankton");

        var drawnCard = GameManager.Instance.GameDeck?.DrawCard();
        if (drawnCard is not null)
        {
            GameManager.Instance.PlayerHand.TryAddCardToHand(drawnCard);
        }

        if (planktonCard is not null)
        {
            GameManager.Instance.GameDeck?.AddCard(planktonCard);
        }
        
        GameManager.Instance.StageAreaController.ClearStage();
    }
}
