using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClownFishEffect : ICardEffect
{
    public string EffectDescription => "Next played set receives x-Mult for each anemone in deck or hand. Discards this card.";
    
    public void ActivateEffect()
    {
        var countInDeck = GameManager.Instance.GameDeck?.CardDataInDeck.Count(card => card.CardName == "Anemone") ??
                            0;

        var countInHand = GameManager.Instance.PlayerHand?.CardsInHand.Count(card => card.Data.CardName == "Anemone") ??
                          0;

        GameManager.Instance.CurrentMultiplier = countInDeck + countInHand;
        
        GameManager.Instance.StageAreaController.ClearStage();
    }
}
