using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StingrayEffect : ICardEffect
{
    public string EffectDescription => "Shuffle your hand back into the deck. Redraw your hand.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
