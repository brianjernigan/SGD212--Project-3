using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeahorseEffect : ICardEffect
{
    public string EffectDescription => "Transform all Fish Eggs remaining into Seahorses.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
