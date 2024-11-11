using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    private Dictionary<CardData, int> _defaultDeckConfiguration;
    public Deck CurrentDeck { get; private set; }
    public List<CardData> DiscardPile { get; private set; } = new List<CardData>();

    public event Action<CardData> OnCardDrawn;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDecks();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        CurrentDeck = new Deck(_defaultDeckConfiguration);
    }

    private void InitializeDecks()
    {
        _defaultDeckConfiguration = new Dictionary<CardData, int>();
        var allCards = GameManager.Instance.AllPossibleCards;

        foreach (var card in allCards)
        {
            _defaultDeckConfiguration.Add(card, 4); // Adjust quantity as needed
        }
    }

    public CardData DrawCard()
    {
        var card = CurrentDeck.DrawCardFromDeck();
        if (card != null)
        {
            OnCardDrawn?.Invoke(card);
        }
        else
        {
            Debug.LogWarning("Deck is empty!");
        }
        return card;
    }

    public void DiscardCard(CardData card)
    {
        if (card != null)
        {
            DiscardPile.Add(card);
            Debug.Log($"Card Discarded: {card.CardName}");
        }
        else
        {
            Debug.LogWarning("Attempted to discard a null card.");
        }
    }
}
