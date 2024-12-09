public class StingrayEffect : ICardEffect
{
    public string EffectDescription => "Shuffles this card and your entire hand back into the deck. Redraws a full hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var stageAreaController = GameManager.Instance.StageAreaController;
        var thisCard = stageAreaController.GetFirstStagedCard();
        var cardCount = 0;

        GameManager.Instance.GameDeck.AddCard(thisCard.Data);
        stageAreaController.ClearStageArea(true);
        
        for (var i = 0; i < playerHand.NumCardsInHand; i++)
        {
            GameManager.Instance.GameDeck?.AddCard(playerHand.CardsInHand[i].Data);
            cardCount++;
        }
        
        playerHand.ClearHandArea();
        
        GameManager.Instance.GameDeck?.ShuffleDeck();
        GameManager.Instance.AdditionalCardsDrawn += cardCount + 1 - GameManager.Instance.DefaultHandSize;
        GameManager.Instance.DrawFullHand(true);
    }
}
