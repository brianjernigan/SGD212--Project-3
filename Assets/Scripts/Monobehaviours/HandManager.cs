using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private RectTransform _handArea;
    [SerializeField] private DeckManager _deckManager;
    
    private Hand _playerHand;
    private Deck _currentDeck;
    private List<CardUI> _onScreenCards = new();

    private void Awake()
    {
        _currentDeck = _deckManager.CurrentDeck;
        _playerHand = new Hand(_currentDeck);
    }

    private void OnEnable()
    {
        _deckManager.OnCardDrawn += HandleCardDrawn;
    }

    private void OnDisable()
    {
        _deckManager.OnCardDrawn -= HandleCardDrawn;
    }

    public void DrawCardToHand()
    {
        _deckManager.DrawCard();
    }

    private void HandleCardDrawn(CardData drawnCard)
    {
        if (drawnCard == null) return;
        if (!_playerHand.AddCardToHand(drawnCard)) return;
        
        var onScreenCard = Instantiate(_cardPrefab, _handArea);
        var cardUI = onScreenCard.GetComponent<CardUI>();
        cardUI.InitializeCard(drawnCard);
        _onScreenCards.Add(cardUI);

        // Trigger draw effects and animations
        cardUI.OnCardDrawn();
    }
}
