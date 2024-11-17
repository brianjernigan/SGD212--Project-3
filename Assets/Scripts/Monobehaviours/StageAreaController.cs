using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StageAreaController : MonoBehaviour
{
    public List<GameCard> CardStaged { get; } = new();

    public int NumCardsStaged => CardStaged.Count;
    public int Score => CardStaged.Sum(card => card.Data.CardRank);
    
    private bool CanStageCard(CardData card)
    {
        var cardsAreEmpty = CardStaged.Count == 0;

        if (cardsAreEmpty) return true;
        
        var cardsAreFull = CardStaged.Count >= 3;
        var cardIsMatch = CardStaged[0].Data.CardRank == card.CardRank;

        return !cardsAreFull && cardIsMatch;
    }

    public bool TryAddCardToStage(GameCard gameCard)
    {
        var cardData = gameCard.Data;
        
        if (!CanStageCard(cardData)) return false;

        CardStaged.Add(gameCard);
        return true;
    }

    public bool TryRemoveCardFromStage(GameCard gameCard)
    {
        if (CardStaged.Contains(gameCard) && CardStaged.Remove(gameCard))
        {
            GameManager.Instance.RearrangeStage();
            return true;
        }

        return false;
    }

    public GameCard GetFirstStagedCard()
    {
        return CardStaged.Count > 0 ? CardStaged[0] : null;
    }

    public void ClearStage()
    {
        foreach (var gameCard in CardStaged.ToList())
        {
            if (gameCard.UI is not null)
            {
                Destroy(gameCard.UI.gameObject);
            }
        }
        
        CardStaged.Clear();
    }
}
