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

    private bool _shellyIsUpdated;

    private void Update()
    {
        var canBeScored = StageContainsWhaleShark() || NumCardsStaged >= 3;
        _playButtonText.text = canBeScored ? "Score" : "Play";
        UIManager.Instance.UpdatePlaysRemainingText(canBeScored ? "!" : GameManager.Instance.PlaysRemaining.ToString());

        if (canBeScored && !_shellyIsUpdated && !GameManager.Instance.IsTutorialMode)
        {
            UpdateShellyWithScore();
        }
        else if (!canBeScored && _shellyIsUpdated && NumCardsStaged != 0 && !GameManager.Instance.IsTutorialMode)
        {
            UpdateShellyThinking();
            _shellyIsUpdated = false;
        }
    }

    private void UpdateShellyThinking()
    {
        var shellyController = GameManager.Instance.ShellyController;
        var thinkingMessage = shellyController.GetRandomThinkingDialog();
        shellyController.ActivateTextBox(thinkingMessage);
    }

    private void UpdateShellyWithScore()
    {
        var shellyController = GameManager.Instance.ShellyController;
        var scoreMessage = $"That set will score {CalculateScore()} points!";

        shellyController.ActivateTextBox(scoreMessage);
        _shellyIsUpdated = true;
    }

    private bool CanStageCard(CardData card)
    {
        if (NumCardsStaged == 0) return true;
        if (card.CardName == "Kraken") return true;

        var cardsAreFull = CardsStaged.Count >= 4;
        if (cardsAreFull) return false;

        var containsKraken = CardsStaged.Exists(stagedCard => stagedCard.Data.CardName == "Kraken");
        if (containsKraken && NumCardsStaged == 1) return true;

        if (containsKraken && NumCardsStaged > 1)
        {
            var nonKrakenCard = CardsStaged.Find(stagedCard => stagedCard.Data.CardName != "Kraken");
            if (nonKrakenCard is not null)
            {
                return card.CardRank == nonKrakenCard.Data.CardRank;
            }
        }

        var firstCard = CardsStaged[0].Data;
        var cardIsMatch = firstCard.CardRank == card.CardRank;
        var cardIsKraken = card.CardName == "Kraken";

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
        var stageHadFour = NumCardsStaged == 4;
        
        if (CardsStaged.Contains(gameCard) && CardsStaged.Remove(gameCard))
        {
            if (stageHadFour)
            {
                GameManager.Instance.CurrentMultiplier -= 1;
                GameManager.Instance.TriggerMultiplierChanged();
            }
            
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

    public void ClearStageArea(bool isFromPlay)
    {
        foreach (var gameCard in CardsStaged.ToList())
        {
            if (gameCard.UI is not null)
            {
                gameCard.IsStaged = false;
                GameManager.Instance.FullDiscard(gameCard, isFromPlay);
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

    public bool StageContainsWhaleShark()
    {
        return CardsStaged.Exists(stagedCard => stagedCard.Data.CardName == "Whaleshark");
    }
}
