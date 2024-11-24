using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorayEffect : ICardEffect
{
    public string EffectDescription => "Discards all cards that are unable to form a set (2 or less remaining).";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
