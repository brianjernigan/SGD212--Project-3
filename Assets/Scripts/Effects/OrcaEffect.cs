using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcaEffect : ICardEffect
{
    public string EffectDescription => "Discard your entire hand. Redraw hand.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
