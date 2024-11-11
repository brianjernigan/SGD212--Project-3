using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardEffect
{
    string Description { get; }
    void ActivateEffect();
}
