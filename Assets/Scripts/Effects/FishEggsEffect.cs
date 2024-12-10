using Random = UnityEngine.Random;

public class FishEggsEffect : ICardEffect
{
    public string EffectDescription => "Transforms this card into a random card from your hand. Returns the transformed card to your hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;
        if (playerHand.CardsInHand.Count == 0) return;

        GameCard randomCardInHand;

        do
        {
            randomCardInHand = playerHand.CardsInHand[Random.Range(0, playerHand.NumCardsInHand)];
        } while (randomCardInHand.Data.CardName == "FishEggs");
        
        var stagedCard = GameManager.Instance.StageAreaController.GetFirstStagedCard();

        var handCardData = randomCardInHand.Data;
        var handCardEffect = randomCardInHand.CardEffect;

        AudioManager.Instance.PlayTransformAudio();
        stagedCard.TransformCard(handCardData, stagedCard.UI, handCardEffect);
        playerHand.TryAddCardToHand(stagedCard);
        GameManager.Instance.PlaceCardInHand(stagedCard, true);
        
        GameManager.Instance.StageAreaController.CardsStaged.Clear();

        if (GameManager.Instance.IsTutorialMode &&
            TutorialManager.Instance.CurrentStep == TutorialStep.ActivateFishEggs)
        {
            TutorialManager.Instance.SetTutorialStep(TutorialStep.ScoreWhaleShark);
        }
    }
}
