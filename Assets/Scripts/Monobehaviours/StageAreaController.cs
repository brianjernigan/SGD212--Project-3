using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StageAreaController : MonoBehaviour
{
    private readonly List<GameCard> _cardsStaged = new();

    private bool CanStageCard(CardData card)
    {
        var cardsAreEmpty = _cardsStaged.Count == 0;

        if (cardsAreEmpty) return true;
        
        var cardsAreFull = _cardsStaged.Count >= 3;
        var cardIsMatch = _cardsStaged[0].Data.CardRank == card.CardRank;

        return !cardsAreFull && cardIsMatch;
    }

    public bool TryAddCardToStageArea(GameCard gameCard)
    {
        var cardData = gameCard.Data;
        
        if (!CanStageCard(cardData)) return false;

        _cardsStaged.Add(gameCard);
        return true;
    }

    public bool TryRemoveCardFromStageArea(GameCard gameCard)
    {
        return _cardsStaged.Remove(gameCard);
    }
}
