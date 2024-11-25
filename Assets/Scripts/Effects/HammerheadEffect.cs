using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerheadEffect : ICardEffect
{
    public string EffectDescription =>
        "Discard all remaining Stingrays. The next played set receives x-Mult for each Stingray discarded.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var deck = GameManager.Instance.GameDeck;

        var stingrayCount = 0;
        
        for (var i = 0; i < playerHand.CardsInHand.Count; i++)
        {
            if (playerHand.CardsInHand[i].Data.CardName == "Stingray")
            {
                playerHand.TryDiscardCardFromHand(playerHand.CardsInHand[i]);
                stingrayCount++;
            }
        }

        for (var i = 0; i < deck.CardDataInDeck.Count; i++)
        {
            if (deck.CardDataInDeck[i].CardName == "Stingray")
            {
                deck.CardDataInDeck.Remove(deck.CardDataInDeck[i]);
                GameManager.Instance.TriggerCardsRemainingChanged();
                stingrayCount++;
            }
        }

        GameManager.Instance.CurrentMultiplier = stingrayCount;
        GameManager.Instance.TriggerMultiplierChanged();
    }
}
