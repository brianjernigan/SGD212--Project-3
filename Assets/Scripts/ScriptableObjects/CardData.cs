using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    [SerializeField] private string _cardName;
    [SerializeField] private Image _cardImage;
    [SerializeField] private int _cardCost;
    [SerializeField] private int _cardRank;
    [SerializeField] private CardEffect _cardEffect;
    [SerializeField] private FishType _fishType;

    public string CardName => _cardName;
    public Image CardImage => _cardImage;
    public int CardCost => _cardCost;
    public int CardRank => _cardRank;
    public CardEffect CardEffect => _cardEffect;
    public FishType FishType => _fishType;

    public void ApplyEffect(CardData targetCard)
    {
        throw new NotImplementedException();
    }
}
