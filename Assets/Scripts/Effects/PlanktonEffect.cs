public class PlanktonEffect : ICardEffect
{
    public string EffectDescription => "Draws 1 card to your hand and adds 1 Plankton to the deck. Discards this card.";
    
    public void ActivateEffect()
    {
        var planktonCard = CardLibrary.Instance.GetCardDataByName("Plankton");

        var drawnCard = GameManager.Instance.GameDeck?.DrawCard();
        if (drawnCard is not null)
        {
            GameManager.Instance.PlayerHand.TryAddCardToHand(drawnCard);
        }

        if (planktonCard is not null)
        {
            GameManager.Instance.GameDeck?.AddCard(planktonCard);
        }
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
    }
}
