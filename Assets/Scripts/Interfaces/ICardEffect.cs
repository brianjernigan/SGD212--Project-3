using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardEffect
{
    Hand PlayerHand { get; }
    Deck GameDeck { get; }
    string EffectDescription { get; }
    void ActivateEffect();
}
