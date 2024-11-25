using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcaEffect : ICardEffect
{
    public string EffectDescription => "Discard your entire hand. Redraw a full hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;

        for (var i = 0; i < playerHand.NumCardsInHand; i++)
        {
            playerHand.TryDiscardCardFromHand(playerHand.CardsInHand[i]);
        }
    }
}
