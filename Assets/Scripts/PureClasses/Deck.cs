using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// Handles internal deck logic
public class Deck
{
    private readonly List<CardData> _cardsInDeck;
    private readonly GameObject _cardPrefab;
    private List<Transform> _cardPositions;

    public bool IsEmpty => _cardsInDeck.Count == 0;

    public Deck(Dictionary<CardData, int> deckComposition, GameObject prefab, List<Transform> cardPositions)
    {
        _cardsInDeck = new List<CardData>();
        ConfigureDeck(deckComposition);
        _cardPrefab = prefab;
        _cardPositions = cardPositions;

        ShuffleDeck();
    }

    private void ConfigureDeck(Dictionary<CardData, int> composition)
    {
        foreach (var entry in composition)
        {
            for (var i = 0; i < entry.Value; i++)
            {
                _cardsInDeck.Add(entry.Key);
            }
        }
    }
    
    private void ShuffleDeck()
    {
        for (var i = _cardsInDeck.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (_cardsInDeck[i], _cardsInDeck[j]) = (_cardsInDeck[j], _cardsInDeck[i]);
        }
    }
    
    public GameCard DrawCard()
    {
        if (_cardsInDeck.Count == 0) return null;

        var drawnCardData = _cardsInDeck[0];
        _cardsInDeck.RemoveAt(0);

        var cardUIObject = Object.Instantiate(_cardPrefab);
        
        var cardUI = cardUIObject.GetComponent<CardUI>();
        var cardEffect = GameManager.Instance.GetEffectForRank(drawnCardData.CardRank);

        var gameCard = new GameCard(drawnCardData, cardUI, cardEffect);
        cardUI.InitializeCard(drawnCardData, gameCard);

        GameManager.Instance.PlaceCardInHand(gameCard, true);
        
        return gameCard;
    }

    public GameCard DrawRandomCard()
    {
        if (_cardsInDeck.Count == 0) return null;

        var randomIndex = Random.Range(0, _cardsInDeck.Count);
        var drawnCardData = _cardsInDeck[randomIndex];
        _cardsInDeck.RemoveAt(randomIndex);
        
        var cardEffect = GameManager.Instance.GetEffectForRank(drawnCardData.CardRank);
        var cardUIObject = Object.Instantiate(_cardPrefab);
        var cardUI = cardUIObject.GetComponent<CardUI>();

        var gameCard = new GameCard(drawnCardData, cardUI, cardEffect);
        cardUI.InitializeCard(drawnCardData, gameCard);

        return gameCard;
    }
}
