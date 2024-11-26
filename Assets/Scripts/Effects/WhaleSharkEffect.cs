using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhaleSharkEffect : ICardEffect
{
    public string EffectDescription =>
        "This card can be scored as a set of 1, 2, 3, or 4. Value is multiplied by the number of remaining plankton.";
    
    public void ActivateEffect()
    {
        var planktonCountInDeck = GameManager.Instance.GameDeck.CardDataInDeck.Count(card => card.CardName == "Plankton");
        var planktonCountInHand =
            GameManager.Instance.PlayerHand.CardsInHand.Count(card => card.Data.CardName == "Plankton");

        GameManager.Instance.CurrentMultiplier += planktonCountInDeck + planktonCountInHand;
        GameManager.Instance.TriggerMultiplierChanged();
    }
}
