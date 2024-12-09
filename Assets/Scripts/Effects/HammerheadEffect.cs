public class HammerheadEffect : ICardEffect
{
    public string EffectDescription =>
        "Discards all remaining Stingrays and this card. The next played set receives x-Mult for each Stingray discarded.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        var deck = GameManager.Instance.GameDeck;

        var stingrayCount = 0;
        
        for (var i = playerHand.CardsInHand.Count - 1; i >= 0; i--)
        {
            if (playerHand.CardsInHand[i].Data.CardName == "Stingray")
            {
                playerHand.TryDiscardCardFromHand(playerHand.CardsInHand[i]);
                stingrayCount++;
            }
        }

        for (var i = deck.CardDataInDeck.Count - 1; i >= 0; i--)
        {
            if (deck.CardDataInDeck[i].CardName == "Stingray")
            {
                deck.CardDataInDeck.Remove(deck.CardDataInDeck[i]);
                GameManager.Instance.TriggerCardsRemainingChanged();
                stingrayCount++;
            }
        }

        GameManager.Instance.CurrentMultiplier += stingrayCount;
        GameManager.Instance.TriggerMultiplierChanged();
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
    }
}
