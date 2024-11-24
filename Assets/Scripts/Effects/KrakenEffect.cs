using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrakenEffect : ICardEffect
{
    public string EffectDescription => "No effect. This card can be played as any rank.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
