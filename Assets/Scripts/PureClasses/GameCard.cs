using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCard
{
    public CardData Data { get; private set; }
    public CardUI UI { get; private set; }
    private ICardEffect _cardEffect;

    // Flag to indicate animation
    public bool IsAnimating { get; set; } 

    public GameCard(CardData data, CardUI ui, ICardEffect effect)
    {
        Data = data;
        UI = ui;
        _cardEffect = effect;
    }

    public string Description => _cardEffect?.EffectDescription ?? "No effect";

    public void ActivateEffect()
    {
        _cardEffect?.ActivateEffect();
    }

    public void TransformCard(CardData newCardData, ICardEffect newEffect)
    {
        Data = newCardData;
        _cardEffect = newEffect;
        UI.InitializeCard(newCardData, this);
    }
}
