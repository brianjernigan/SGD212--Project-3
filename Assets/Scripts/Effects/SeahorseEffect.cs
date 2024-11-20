using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeahorseEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Transform all Fish Eggs remaining into Seahorses.";

    public SeahorseEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
