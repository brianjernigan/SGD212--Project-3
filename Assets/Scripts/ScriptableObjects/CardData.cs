using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    [SerializeField] private string _cardName;
    [SerializeField] private Sprite _cardSprite;
    [SerializeField] private int _cardCost;

    public string CardName => _cardName;
    public Sprite CardSprite => _cardSprite;
    public int CardCost => _cardCost;
}
