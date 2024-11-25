using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleEffect : ICardEffect
{
    public string EffectDescription => "Next turn, draw 2 additional cards. Discards this card.";
    
    public void ActivateEffect()
    {
        GameManager.Instance.AdditionalCardsOnScreen = 2;
        
        GameManager.Instance.StageAreaController.ClearStage();
    }
}
