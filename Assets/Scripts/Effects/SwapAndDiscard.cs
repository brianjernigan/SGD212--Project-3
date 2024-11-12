using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwapAndDiscard : ICardEffect
{
    private readonly Hand _playerHand;
    private readonly Deck _gameDeck;
    
    public string EffectDescription  => "Discards a random card from your hand and draws a card from the deck.";

    public SwapAndDiscard(Hand playerHand, Deck gameDeck)
    {
        _playerHand = playerHand;
        _gameDeck = gameDeck;
    }
    public void ActivateEffect()
    {
        Debug.Log("Activating effect");
        
        var cardsInHand = _playerHand.CardsInHand;
        var randomIndex = Random.Range(0, _playerHand.CardsInHand.Count);

        if (_playerHand.NumCardsInHand != 0)
        {
            var randomCard = cardsInHand[randomIndex];
            _playerHand.TryDiscardCardFromHand(randomCard);
        }
        
        _playerHand.TryAddCardToHand(_gameDeck.DrawCard());
    }
}
