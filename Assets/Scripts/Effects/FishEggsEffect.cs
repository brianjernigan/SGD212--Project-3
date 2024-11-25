using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishEggsEffect : ICardEffect
{
    public string EffectDescription => "Transform this card into a random card from your hand. The transformed card remains staged.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        if (playerHand.CardsInHand.Count == 0) return;

        var randomCardInHand = playerHand.CardsInHand[Random.Range(0, playerHand.NumCardsInHand)];
        var stagedCard = GameManager.Instance.StageAreaController.GetFirstStagedCard();

        var handCardData = randomCardInHand.Data;
        var handCardUI = randomCardInHand.UI;
        var handCardEffect = randomCardInHand.CardEffect;

        stagedCard.TransformCard(handCardData, handCardUI, handCardEffect);
    }
}
