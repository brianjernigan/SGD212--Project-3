using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private TMP_Text _cardRankText;
    [SerializeField] private TMP_Text _cardCostText;

    public void InitializeCard(CardData data)
    {
        _cardImage = data.CardImage;
        _cardRankText.text = data.CardRank.ToString();
        _cardCostText.text = data.CardCost.ToString();
    }
}
