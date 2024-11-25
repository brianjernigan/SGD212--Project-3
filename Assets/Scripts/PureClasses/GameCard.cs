using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCard
{
    public CardData Data { get; private set; }
    public CardUI UI { get; private set; }
    public ICardEffect CardEffect { get; private set; }

    // Flag to indicate animation
    public bool IsAnimating { get; set; } 

    public GameCard(CardData data, CardUI ui, ICardEffect effect)
    {
        Data = data;
        UI = ui;
        CardEffect = effect;
    }

    public string Description => CardEffect?.EffectDescription ?? "No effect";

    public void ActivateEffect()
    {
        CardEffect?.ActivateEffect();
    }

    public void TransformCard(CardData newCardData, CardUI ui, ICardEffect newEffect)
    {
        Data = newCardData;
        CardEffect = newEffect;
        UI = ui;
        UI.InitializeCard(newCardData, this);
    }
}
