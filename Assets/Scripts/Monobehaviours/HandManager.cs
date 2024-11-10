using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles on screen interactions
public class HandManager : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private RectTransform _handArea;
    [SerializeField] private DeckManager _deckManager;
    
    private Hand _playerHand;
    private List<CardUI> _onScreenHand;
    
    private void Awake()
    {
        _playerHand = new Hand(_deckManager.CurrentDeck);
    }
    
    public void DrawFullHand()
    {
        while (!_playerHand.HandIsFull())
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (!_playerHand.AddCardToHand(out var card)) return;
        
        var onScreenCard = Instantiate(_cardPrefab, _handArea);
        var cardUI = onScreenCard.GetComponent<CardUI>();
        cardUI.InitializeCard(card, this);
        // _onScreenCards.Add(cardUI);
    }

    public void DiscardCardFromHand(CardData card)
    {
        _playerHand.RemoveCardFromHand(card);
    }
}
