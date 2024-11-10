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
    private readonly GameObject _cardUIPrefab;
    private readonly RectTransform _handArea;

    public Deck(IEnumerable<CardData> initialCards, GameObject uiPrefab)
    {
        _cardsInDeck = new List<CardData>(initialCards);
        _cardUIPrefab = uiPrefab;
        _handArea = GameManager.Instance.HandArea;

        ShuffleDeck();
    }

    // private void InitializeDeck(Dictionary<CardData, int> deckComposition)
    // {
    //     foreach (var entry in deckComposition)
    //     {
    //         for (var i = 0; i < entry.Value; i++)
    //         {
    //             CardsInDeck.Add(entry.Key);
    //         }
    //     }
    //
    //     ShuffleDeck();
    // }
    //
    
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

        var cardUIObject = Object.Instantiate(_cardUIPrefab, _handArea);
        var cardUI = cardUIObject.GetComponent<CardUI>();
        cardUI.InitializeCard(drawnCardData, cardUIObject);

        var gameCard = new GameCard(drawnCardData, cardUI);

        return gameCard;
    }
}
