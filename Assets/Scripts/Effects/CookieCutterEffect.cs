using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieCutterEffect : ICardEffect
{
    public string EffectDescription =>
        "Discards all higher ranked cards in hand. Draws 1 card for each card discarded.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
