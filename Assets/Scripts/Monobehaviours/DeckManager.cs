using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private Deck _deck;
    [SerializeField] private HandManager _handManager;

    private List<CardData> _discardPile = new();

    private void Start()
    {
        _deck.InitializeDeck();
        _deck.ShuffleDeck();
    }

    public void DrawCard()
    {
        var drawnCardData = _deck.DrawCard();

        if (drawnCardData is not null)
        {
            _handManager.AddCardToHand(drawnCardData);
        }
    }
}
