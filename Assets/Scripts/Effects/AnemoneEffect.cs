using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnemoneEffect : ICardEffect
{
    public string EffectDescription => "Add 2 clownfish to your deck.";
    
    public void ActivateEffect()
    {
        Debug.Log(EffectDescription);
    }
}
