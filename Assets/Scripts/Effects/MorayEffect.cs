using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorayEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Discards all cards that are unable to form a set (2 or less remaining).";

    public MorayEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
