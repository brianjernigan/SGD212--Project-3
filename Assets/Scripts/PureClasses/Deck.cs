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
    public List<CardData> CardDataInDeck { get; private set; }
    private readonly GameObject _cardPrefab;

    public bool IsEmpty => CardDataInDeck.Count == 0;

    public Deck(Dictionary<CardData, int> deckComposition, GameObject prefab)
    {
        CardDataInDeck = new List<CardData>();
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
                CardDataInDeck.Add(entry.Key);
            }
        }
    }
    
    public void ShuffleDeck()
    {
        for (var i = CardDataInDeck.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (CardDataInDeck[i], CardDataInDeck[j]) = (CardDataInDeck[j], CardDataInDeck[i]);
        }
    }
    
    public GameCard DrawCard()
    {
        if (IsEmpty) return null;

        var drawnCardData = CardDataInDeck[0];
        CardDataInDeck.RemoveAt(0);

        return CreateGameCard(drawnCardData);
    }

    public GameCard DrawRandomCard()
    {
        if (IsEmpty) return null;

        var randomIndex = Random.Range(0, CardDataInDeck.Count);
        var drawnCardData = CardDataInDeck[randomIndex];
        CardDataInDeck.RemoveAt(randomIndex);

        return CreateGameCard(drawnCardData);
    }

    public void AddCard(CardData data, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            CardDataInDeck.Add(data);
        }
    }

    public GameCard DrawSpecificCard(CardData data)
    {
        var cardIndex = CardDataInDeck.FindIndex(card => card == data);

        if (cardIndex == -1)
        {
            Debug.Log("Card not found");
            return null;
        }

        var drawnCardData = CardDataInDeck[cardIndex];
        CardDataInDeck.RemoveAt(cardIndex);

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
