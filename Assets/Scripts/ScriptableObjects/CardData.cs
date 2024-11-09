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
    [SerializeField] private Image _cardImage;
    [SerializeField] private Image _backOfCardImage;
    [SerializeField] private int _cardCost;
    [SerializeField] private int _cardRank;
    [SerializeField] private bool _isFlipped;

    public string CardName => _cardName;
    public Image CardImage => _cardImage;
    public Image BackOfCardImage => _backOfCardImage;
    public int CardCost => _cardCost;
    public int CardRank => _cardRank;
    public bool IsFlipped => _isFlipped;
}
