public class OrcaEffect : ICardEffect
{
    public string EffectDescription => "Discards this card and your entire hand. Redraws a full hand.";
    
    public void ActivateEffect()
    {
        var playerHand = GameManager.Instance.PlayerHand;

        for (var i = playerHand.NumCardsInHand - 1; i >= 0; i--)
        {
            playerHand.TryDiscardCardFromHand(playerHand.CardsInHand[i]);
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);

        GameManager.Instance.DrawFullHand(true);
        GameManager.Instance.TriggerCardsRemainingChanged();
    }
}
