using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAreaController : MonoBehaviour
{
    private List<CardData> _cardsInPlay = new();

    public void AddCardToPlayArea(CardData cardInPlay)
    {
        _cardsInPlay.Add(cardInPlay);
    }
}
