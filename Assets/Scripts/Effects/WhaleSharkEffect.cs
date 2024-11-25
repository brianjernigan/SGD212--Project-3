using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhaleSharkEffect : ICardEffect
{
    public string EffectDescription =>
        "This card can always be scored. Value is multiplied by the number of plankton remaining in deck.";
    
    public void ActivateEffect()
    {
        var planktonCount = GameManager.Instance.GameDeck.CardDataInDeck.Count(card => card.CardName == "Plankton");

        GameManager.Instance.CurrentMultiplier += planktonCount;
    }
}
