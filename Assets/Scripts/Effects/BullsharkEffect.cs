using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullsharkEffect : ICardEffect
{
    public string EffectDescription => "Discard a random bullshark. Increase your hand size by 1 this round.";
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
