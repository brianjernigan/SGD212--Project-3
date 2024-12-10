public class AnemoneEffect : ICardEffect
{
    public string EffectDescription => "Adds 2 clownfish to your hand. Discards this card.";
    
    public void ActivateEffect()
    {
        var clownFishData = CardLibrary.Instance.GetCardDataByName("ClownFish");
        if (clownFishData is null) return;

        for (var i = 0; i < 2; i++)
        {
            var clownFishCard = GameManager.Instance.GameDeck.CreateGameCard(clownFishData);
            GameManager.Instance.PlayerHand.TryAddCardToHand(clownFishCard);
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
        
        if (GameManager.Instance.IsTutorialMode)
        {
            TutorialManager.Instance.SetTutorialStep(TutorialStep.ScoreThreeSet);   
        }
    }
}
