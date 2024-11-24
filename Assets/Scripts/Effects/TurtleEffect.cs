using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleEffect : ICardEffect
{
    public string EffectDescription => "Next turn, draw 2 additional cards.";
    
    public void ActivateEffect()
    {
        throw new System.NotImplementedException();
    }
}
