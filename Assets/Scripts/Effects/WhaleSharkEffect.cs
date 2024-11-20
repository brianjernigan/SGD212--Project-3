using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleSharkEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }

    public string EffectDescription =>
        "This card can always be scored. Value is multiplied by the number of plankton remaining in deck.";

    public WhaleSharkEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
