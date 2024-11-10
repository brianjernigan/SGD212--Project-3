using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Card Areas")]
    [SerializeField] private RectTransform _playArea;
    [SerializeField] private RectTransform _discardArea;
    [SerializeField] private RectTransform _handArea;
    [SerializeField] private RectTransform _deckArea;

    [SerializeField] private Canvas _gameCanvas;
}
