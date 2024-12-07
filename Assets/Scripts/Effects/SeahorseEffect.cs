public class SeahorseEffect : ICardEffect
{
    public string EffectDescription => "Transforms all Fish Eggs remaining into Seahorses. This card remains staged.";
    
    public void ActivateEffect()
    {
        var deck = GameManager.Instance.GameDeck;
        var seahorseData = CardLibrary.Instance.GetCardDataByName("Seahorse");

        if (deck is null || seahorseData is null) return;

        for (var i = 0; i < deck.CardDataInDeck.Count; i++)
        {
            if (deck.CardDataInDeck[i].CardName == "FishEggs")
            {
                deck.CardDataInDeck[i] = seahorseData;
            }
        }

        var playerHand = GameManager.Instance.PlayerHand;

        foreach (var card in playerHand.CardsInHand)
        {
            if (card.Data.CardName == "FishEggs")
            {
                card.TransformCard(seahorseData, card.UI, new SeahorseEffect());
            }
        }
    }
}
