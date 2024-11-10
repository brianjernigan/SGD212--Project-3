using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StageAreaController : MonoBehaviour
{
    private readonly List<CardData> _cardsStaged = new();

    private bool CanStageCard(CardData card)
    {
        var cardsAreEmpty = _cardsStaged.Count == 0;

        if (cardsAreEmpty) return true;
        
        var cardsAreFull = _cardsStaged.Count >= 3;
        var cardIsMatch = _cardsStaged[0].CardRank == card.CardRank;

        return !cardsAreFull && cardIsMatch;
    }

    public bool AddCardToStageArea(CardData card)
    {
        if (!CanStageCard(card)) return false;

        _cardsStaged.Add(card);
        return true;
    }

    public bool RemoveCardFromStageArea(CardData card)
    {
        return _cardsStaged.Remove(card);
    }
}
