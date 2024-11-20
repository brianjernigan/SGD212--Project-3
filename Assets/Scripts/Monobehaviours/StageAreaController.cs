using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StageAreaController : MonoBehaviour
{
    public List<GameCard> CardsStaged { get; } = new();

    public int NumCardsStaged => CardsStaged.Count;
    public int Score => CardsStaged.Sum(card => card.Data.CardRank);
    
    private bool CanStageCard(CardData card)
    {
        var cardsAreEmpty = CardsStaged.Count == 0;

        if (cardsAreEmpty) return true;
        
        var cardsAreFull = CardsStaged.Count >= 4;
        var cardIsMatch = CardsStaged[0].Data.CardRank == card.CardRank;

        return !cardsAreFull && cardIsMatch;
    }

    public bool TryAddCardToStage(GameCard gameCard)
    {
        var cardData = gameCard.Data;
        
        if (!CanStageCard(cardData)) return false;

        CardsStaged.Add(gameCard);
        GameManager.Instance.RearrangeStage();
        return true;
    }

    public bool TryRemoveCardFromStage(GameCard gameCard)
    {
        if (CardsStaged.Contains(gameCard) && CardsStaged.Remove(gameCard))
        {
            if (CardsStaged.Count == 0)
            {
                ClearStage();
            }
            else
            {
                GameManager.Instance.RearrangeStage();
            }
            
            return true;
        }

        return false;
    }

    public GameCard GetFirstStagedCard()
    {
        return CardsStaged.Count > 0 ? CardsStaged[0] : null;
    }

    public void ClearStage()
    {
        foreach (var gameCard in CardsStaged.ToList())
        {
            if (gameCard.UI is not null)
            {
                Destroy(gameCard.UI.gameObject);
            }
        }
        
        CardsStaged.Clear();
    }
}
