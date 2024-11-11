using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles all static data from card
[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    [SerializeField] private string _cardName;
    [SerializeField] private Sprite _cardSprite;
    [SerializeField] private Sprite _backOfCardSprite;
    [SerializeField] private int _cardCost;
    [SerializeField] private int _cardRank;

    public string CardName => _cardName;
    public Sprite CardSprite => _cardSprite;
    public Sprite BackOfCardSprite => _backOfCardSprite;
    public int CardCost => _cardCost;
    public int CardRank => _cardRank;
}
