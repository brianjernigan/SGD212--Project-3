using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetEffect : ICardEffect
{
    public string EffectDescription =>
        "Draw a random pair to your hand (Will add cards to deck if no pairs remaining).";
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
