using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEggsEffect : ICardEffect
{
    public Hand PlayerHand { get; }
    public Deck GameDeck { get; }
    public string EffectDescription => "Transform this card into a random card in your hand. Add it back to your hand.";

    public FishEggsEffect(Hand hand, Deck deck)
    {
        PlayerHand = hand;
        GameDeck = deck;
    }
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
