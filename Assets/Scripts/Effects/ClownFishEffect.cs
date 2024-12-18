using System.Linq;

public class ClownFishEffect : ICardEffect
{
    public string EffectDescription => "The next played set receives x-Mult for each anemone in deck or hand. Discards this card.";
    
    public void ActivateEffect()
    {
        var countInDeck = GameManager.Instance.GameDeck?.CardDataInDeck.Count(card => card.CardName == "Anemone") ??
                            0;

        var countInHand = GameManager.Instance.PlayerHand?.CardsInHand.Count(card => card.Data.CardName == "Anemone") ??
                          0;

        GameManager.Instance.CurrentMultiplier += countInDeck + countInHand;
        GameManager.Instance.TriggerMultiplierChanged();
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
    }
}
