using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StageAreaController : MonoBehaviour
{
    private readonly List<GameCard> _cardsStaged = new();
    public int NumCardsStaged => _cardsStaged.Count;
    public int Score => _cardsStaged.Sum(card => card.Data.CardRank);
    
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
        return _cardsStaged.Contains(gameCard) && _cardsStaged.Remove(gameCard);
    }

    public int CheckStagedCards()
    {
        return _cardsStaged.Count;
    }

    public void ClearStagedCards()
    {
        foreach (var card in _cardsStaged.ToList())
        {
            Destroy(card.UI.gameObject);
        }
        
        _cardsStaged.Clear();
    }
}
