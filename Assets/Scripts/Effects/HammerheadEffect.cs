using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerheadEffect : ICardEffect
{
    public string EffectDescription =>
        "Discard all remaining Stingrays. The next played set receives x-Mult for each Stingray discarded.";
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
