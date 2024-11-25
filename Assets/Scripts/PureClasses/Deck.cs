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

    public bool IsEmpty => _cardsInDeck.Count == 0;

    public Deck(Dictionary<CardData, int> deckComposition, GameObject prefab)
    {
        _cardsInDeck = new List<CardData>();
        ConfigureDeck(deckComposition);
        _cardPrefab = prefab;

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
        if (IsEmpty) return null;

        var drawnCardData = _cardsInDeck[0];
        _cardsInDeck.RemoveAt(0);

        return CreateGameCard(drawnCardData);
    }

    public GameCard DrawRandomCard()
    {
        if (IsEmpty) return null;

        var randomIndex = Random.Range(0, _cardsInDeck.Count);
        var drawnCardData = _cardsInDeck[randomIndex];
        _cardsInDeck.RemoveAt(randomIndex);

        return CreateGameCard(drawnCardData);
    }

    public void AddCard(CardData data, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            _cardsInDeck.Add(data);
        }
    }

    public GameCard DrawSpecificCard(CardData data)
    {
        var cardIndex = _cardsInDeck.FindIndex(card => card == data);

        if (cardIndex == -1)
        {
            Debug.Log("Card not found");
            return null;
        }

        var drawnCardData = _cardsInDeck[cardIndex];
        _cardsInDeck.RemoveAt(cardIndex);

        return CreateGameCard(drawnCardData);
    }

    public GameCard CreateGameCard(CardData data)
    {
        var cardUIObject = Object.Instantiate(_cardPrefab);
        var cardUI = cardUIObject.GetComponent<CardUI>();
        var cardEffect = CardLibrary.Instance.GetCardEffectByName(data.CardName);

        var gameCard = new GameCard(data, cardUI, cardEffect);
        cardUI.InitializeCard(data, gameCard);

        return gameCard;
    }
}
