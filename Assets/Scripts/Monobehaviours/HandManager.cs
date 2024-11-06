using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [SerializeField] private Transform _handPanel;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private RectTransform _playArea;

    public int MaxHandSize { get; set; } = 5;
    public int CardsInHand { get; set; }

    private List<CardDisplay> _hand = new();

    public bool CanDraw => _hand.Count < MaxHandSize;

    public void AddCardToHand(CardData drawnCard)
    {
        if (CardsInHand >= MaxHandSize) return;
        
        var cardToDisplay = Instantiate(_cardPrefab, _handPanel);
        var cardDisplay = cardToDisplay.GetComponent<CardDisplay>();

        cardDisplay.InitializeCard(drawnCard);
        cardDisplay.PlayArea = _playArea;
        
        _hand.Add(cardDisplay);
        CardsInHand++;
    }

    public void RemoveCardFromHand(CardData cardToDiscard)
    {
        throw new System.NotImplementedException();
    }
}
