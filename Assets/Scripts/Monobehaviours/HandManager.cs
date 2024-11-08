using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _handArea;
    [SerializeField] private DeckManager _deckManager;
    
    private int _initialHandSize = 5;
    private Hand _playerHand;
    private List<CardData> _onScreenCards = new();

    private void Awake()
    {
        _playerHand = new Hand(_initialHandSize);
    }

    public bool DrawCard()
    {
        var drawnCard = _deckManager.CurrentDeck.DrawCard();

        if (drawnCard is not null)
        {
            if (_playerHand.AddCardToHand(drawnCard))
            {
                var onScreenCard = Instantiate(_cardPrefab, _handArea);
            }
        }

        return false;
    }
}
