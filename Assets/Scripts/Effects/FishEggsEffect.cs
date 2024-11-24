using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEggsEffect : ICardEffect
{
    public string EffectDescription => "Transform this card into a random card in your hand. Add it back to your hand.";
    
    public void ActivateEffect()
    {
        throw new NotImplementedException();
    }
}
