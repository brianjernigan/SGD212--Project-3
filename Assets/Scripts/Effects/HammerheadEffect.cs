using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerheadEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }

    public string EffectDescription =>
        "Discard all remaining Stingrays. The next played set receives x-Mult for each Stingray discarded.";

    public HammerheadEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
