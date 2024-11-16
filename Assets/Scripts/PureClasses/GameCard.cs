using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCard
{
    public CardData Data { get; private set; }
    public CardUI UI { get; private set; }
    private readonly ICardEffect _cardEffect;

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
}
