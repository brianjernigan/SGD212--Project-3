public class SeahorseEffect : ICardEffect
{
    public string EffectDescription => "Transforms all Fish Eggs remaining into Seahorses. Return this card to your hand.";
    
    public void ActivateEffect()
    {
        var deck = GameManager.Instance.GameDeck;
        var seahorseData = CardLibrary.Instance.GetCardDataByName("Seahorse");
        var stageAreaController = GameManager.Instance.StageAreaController;

        if (deck is null || seahorseData is null) return;

        for (var i = 0; i < deck.CardDataInDeck.Count; i++)
        {
            if (deck.CardDataInDeck[i].CardName == "FishEggs")
            {
                deck.CardDataInDeck[i] = seahorseData;
            }
        }

        var playerHand = GameManager.Instance.PlayerHand;

        AudioManager.Instance.PlayTransformAudio();
        
        foreach (var card in playerHand.CardsInHand)
        {
            if (card.Data.CardName == "FishEggs")
            {
                card.TransformCard(seahorseData, card.UI, new SeahorseEffect());
            }
        }

        GameManager.Instance.PlayerHand.TryAddCardToHand(stageAreaController.GetFirstStagedCard());
        GameManager.Instance.PlaceCardInHand(stageAreaController.GetFirstStagedCard(), true);
        
        stageAreaController.CardsStaged.Clear();
    }
}
