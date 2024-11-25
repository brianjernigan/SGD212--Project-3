using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureEffect : ICardEffect
{
    public string EffectDescription => "Gain $5. Discards this card.";
    
    public void ActivateEffect()
    {
        GameManager.Instance.PlayerMoney += 5;
        GameManager.Instance.StageAreaController.ClearStageArea();
    }
}
