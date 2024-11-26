using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnemoneEffect : ICardEffect
{
    public string EffectDescription => "Adds 2 clownfish to your deck. Discards this card.";
    
    public void ActivateEffect()
    {
        var clownFishData = CardLibrary.Instance.GetCardDataByName("Clownfish");
        if (clownFishData is null) return;

        GameManager.Instance.GameDeck?.AddCard(clownFishData, 2);
        GameManager.Instance.StageAreaController.ClearStageArea();
    }
}
