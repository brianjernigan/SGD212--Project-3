using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClownFishEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Next played set receives x-Mult for each anemone in deck or hand.";

    public ClownFishEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
