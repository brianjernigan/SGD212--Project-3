using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClownFishEffect : ICardEffect
{
    public string EffectDescription => "Next played set receives x-Mult for each anemone in deck or hand.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
