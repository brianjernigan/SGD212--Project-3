using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private List<CardData> _allCards;
    private List<CardData> _currentDeck;

    public void InitializeDeck()
    {
        _currentDeck = new List<CardData>(_allCards);
    }

    public void ShuffleDeck()
    {
        for (var i = _currentDeck.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (_currentDeck[i], _currentDeck[j]) = (_currentDeck[j], _currentDeck[i]);
        }
    }

    public CardData DrawCard()
    {
        if (_currentDeck.Count == 0)
        {
            return null;
        }

        var drawnCard = _currentDeck[0];
        _currentDeck.RemoveAt(0);
        return drawnCard;
    }

    public void AddCardToDeck(CardData cardToAdd)
    {
        _currentDeck.Add(cardToAdd);
    }

    public void RemoveCardFromDeck(CardData cardToRemove)
    {
        _currentDeck.Remove(cardToRemove);
    }
}
