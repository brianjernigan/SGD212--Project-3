using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapAndDiscard : ICardEffect
{
    private Hand _playerHand;
    private Deck _gameDeck;
    
    public string Description  => "Discard one card from your hand and swap it with the deck's top card.";

    public SwapAndDiscard(Hand playerHand, Deck gameDeck)
    {
        _playerHand = playerHand;
        _gameDeck = gameDeck;
    }
    public void ActivateEffect()
    {
        Debug.Log(Description);
    }
}
