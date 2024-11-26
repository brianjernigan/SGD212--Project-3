using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StageAreaController : MonoBehaviour
{
    public List<GameCard> CardsStaged { get; } = new();
    public int NumCardsStaged => CardsStaged.Count;

    [SerializeField] private TMP_Text _playButtonText;

    private void Update()
    {
        var canBeScored = StageContainsWhaleShark() || NumCardsStaged >= 3;
        _playButtonText.text = canBeScored ? "Score" : "Play";
    }
    
    private bool CanStageCard(CardData card)
    {
        if (NumCardsStaged == 0) return true;

        var cardsAreFull = CardsStaged.Count >= 4;
        if (cardsAreFull) return false;

        var containsKraken = CardsStaged.Exists(stagedCard => stagedCard.Data.CardRank == 12);
        if (containsKraken && NumCardsStaged == 1) return true;

        if (containsKraken && NumCardsStaged > 1)
        {
            var nonKrakenCard = CardsStaged.Find(stagedCard => stagedCard.Data.CardRank != 12);
            if (nonKrakenCard is not null)
            {
                return card.CardRank == nonKrakenCard.Data.CardRank;
            }
        }

        var firstCard = CardsStaged[0].Data;
        var cardIsMatch = firstCard.CardRank == card.CardRank;
        var cardIsKraken = card.CardRank == 12;

        return cardIsMatch || cardIsKraken;
    }

    public bool TryAddCardToStage(GameCard gameCard)
    {
        var cardData = gameCard.Data;
        
        if (!CanStageCard(cardData)) return false;
        
        CardsStaged.Add(gameCard);
        
        if (NumCardsStaged == 4)
        {
            GameManager.Instance.CurrentMultiplier += 1;
            GameManager.Instance.TriggerMultiplierChanged();
        }
        gameCard.IsStaged = true;
        gameCard.IsInHand = false;
        GameManager.Instance.RearrangeStage();
        return true;
    }

    public bool TryRemoveCardFromStage(GameCard gameCard)
    {
        if (CardsStaged.Contains(gameCard) && CardsStaged.Remove(gameCard))
        {
            gameCard.IsStaged = false;
            
            GameManager.Instance.RearrangeStage();
            
            return true;
        }

        return false;
    }

    public GameCard GetFirstStagedCard()
    {
        return NumCardsStaged > 0 ? CardsStaged[0] : null;
    }

    public void ClearStageArea()
    {
        foreach (var gameCard in CardsStaged.ToList())
        {
            if (gameCard.UI is not null)
            {
                gameCard.IsStaged = false;
                Destroy(gameCard.UI.gameObject);
            }
        }
        
        CardsStaged.Clear();
    }

    public int CalculateScore()
    {
        var score = 0;
        
        foreach (var card in CardsStaged)
        {
            score += card.Data.CardRank;
        }
        
        return score * GameManager.Instance.CurrentMultiplier;
    }

    private bool StageContainsWhaleShark()
    {
        return CardsStaged.Exists(stagedCard => stagedCard.Data.CardName == "Whaleshark");
    }
}
