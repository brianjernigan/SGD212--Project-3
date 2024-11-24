using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanktonEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Draw 1 card to your hand. Add 1 Plankton to the deck.";

    public PlanktonEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
