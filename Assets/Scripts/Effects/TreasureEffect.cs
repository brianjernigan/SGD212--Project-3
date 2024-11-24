using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureEffect : ICardEffect
{
    public string EffectDescription => "Gain $5.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
