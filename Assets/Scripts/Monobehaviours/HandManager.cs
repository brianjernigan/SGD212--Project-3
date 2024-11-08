using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _handArea;
    [SerializeField] private DeckManager _deckManager;
    
    private int _initialHandSize = 5;
    private Hand _playerHand;
}
