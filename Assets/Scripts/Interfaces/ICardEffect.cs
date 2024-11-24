using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardEffect
{
    string EffectDescription { get; }
    void ActivateEffect();
}