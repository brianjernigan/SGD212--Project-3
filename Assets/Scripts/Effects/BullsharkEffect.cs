using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BullsharkEffect : ICardEffect
{
    public string EffectDescription => "Discard a random bullshark from your hand or deck. Increase your hand size by 1 this round.";
    
    public void ActivateEffect()
    {
        var playerHandCards = GameManager.Instance.PlayerHand.CardsInHand;
        var deckData = GameManager.Instance.GameDeck.CardDataInDeck;
        
        var bullsharkData = CardLibrary.Instance.GetCardDataByName("Bullshark");

        var bullsharkDataList = new List<GameCard>();

        foreach (var gameCard in playerHandCards)
        {
            if (gameCard.Data == bullsharkData)
            {
                bullsharkDataList.Add(gameCard);
            }
        }

        for (var i = 0; i < deckData.Count; i++)
        {
            if (deckData[i] == bullsharkData)
            {
                bullsharkDataList.Add(new GameCard(bullsharkData, null, null));
            }
        }

        if (bullsharkDataList.Count == 0)
        {
            Debug.Log("No bullsharks found");
            return;
        }

        var randomIndex = Random.Range(0, bullsharkDataList.Count);
        var randomBullshark = bullsharkDataList[randomIndex];

        if (playerHandCards.Contains(randomBullshark))
        {
            GameManager.Instance.PlayerHand.TryDiscardCardFromHand(randomBullshark);
        }
        else if (deckData.Contains(randomBullshark.Data))
        {
            GameManager.Instance.GameDeck.RemoveCard(randomBullshark.Data);
        }

        GameManager.Instance.HandSizeModifier += 1;
    }
}
