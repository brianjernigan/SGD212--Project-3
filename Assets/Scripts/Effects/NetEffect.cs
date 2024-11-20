using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }

    public string EffectDescription =>
        "Draw a random pair to your hand (Will add cards to deck if no pairs remaining).";

    public NetEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
