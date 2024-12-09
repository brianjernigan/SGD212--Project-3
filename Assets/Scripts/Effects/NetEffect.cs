using System.Linq;
using UnityEngine;

public class NetEffect : ICardEffect
{
    public string EffectDescription =>
        "Draws a random pair to your hand (Will add cards to deck if no pairs remaining). Discards this card.";
    
    public void ActivateEffect()
    {
        var gameDeck = GameManager.Instance.GameDeck;
        var playerHand = GameManager.Instance.PlayerHand;

        var pairsInDeck = gameDeck.CardDataInDeck
            .GroupBy(card => card)
            .Where(group => group.Count() >= 2)
            .Select(group => group.Key)
            .ToList();

        if (pairsInDeck.Count > 0)
        {
            var randomPair = pairsInDeck[Random.Range(0, pairsInDeck.Count)];

            for (var i = 0; i < 2; i++)
            {
                var card = gameDeck.DrawSpecificCard(randomPair);
                if (card is not null)
                {
                    playerHand.TryAddCardToHand(card);
                }
            }
        }
        else
        {
            var randomCard =
                CardLibrary.Instance.AllPossibleCards[Random.Range(0, CardLibrary.Instance.AllPossibleCards.Count)];
            gameDeck.AddCard(randomCard, 2);
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
    }
}
