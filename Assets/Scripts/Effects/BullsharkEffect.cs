using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullsharkEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Discard a random bullshark. Increase your hand size by 1 this round.";

    public BullsharkEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
