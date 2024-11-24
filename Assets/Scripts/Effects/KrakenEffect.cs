using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrakenEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "No effect. This card can be played as any rank.";

    public KrakenEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
