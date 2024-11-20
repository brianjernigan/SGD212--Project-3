using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingrayEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Shuffle your hand back into the deck. Redraw your hand.";

    public StingrayEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
