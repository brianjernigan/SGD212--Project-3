using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieCutterEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }

    public string EffectDescription =>
        "Discards all higher ranked cards in hand. Draws 1 card for each card discarded.";

    public CookieCutterEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
