public class TurtleEffect : ICardEffect
{
    public string EffectDescription => "The next draw draws 2 additional cards. Discards this card.";
    
    public void ActivateEffect()
    {
        GameManager.Instance.AdditionalCardsDrawn += 2;
        GameManager.Instance.TriggerHandSizeChanged();
        
        GameManager.Instance.StageAreaController.ClearStageArea(true);
    }
}
