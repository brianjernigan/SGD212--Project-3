using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Gain $5.";

    public TreasureEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
