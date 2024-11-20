using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles all static data from card
[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    [SerializeField] private CardType _type;
    [SerializeField] private string _cardName;
    [SerializeField] private Material _cardMat;
    [SerializeField] private Material _backOfCardMat;
    [SerializeField] private Sprite _textBoxSprite;
    [SerializeField] private int _cardRank;

    public CardType Type => _type;
    public string CardName => _cardName;
    public Material CardMat => _cardMat;
    public Material BackOfCardMat => _backOfCardMat;
    public Sprite TextBoxSprite => _textBoxSprite;
    public int CardRank => _cardRank;
}
