using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Stores all possible cards for creating decks and starting the game
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<CardData> _allPossibleCards;
    [SerializeField] private GameObject _cardUIPrefab;
    
    [Header("Drop Areas")]
    [SerializeField] private RectTransform _handArea;
    [SerializeField] private RectTransform _stageArea;
    [SerializeField] private RectTransform _discardArea;

    [SerializeField] private Canvas _gameCanvas;
    
    public RectTransform HandArea => _handArea;
    public RectTransform StageArea => _stageArea;
    public RectTransform DiscardArea => _discardArea;

    public Canvas GameCanvas => _gameCanvas;

    private Deck _gameDeck;
    private Hand _playerHand;
    
    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _gameDeck = new Deck(_allPossibleCards, _cardUIPrefab);
        _playerHand = new Hand();
    }

    public void DrawInitialHand()
    {
        while (!_playerHand.IsFull)
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {
        var gameCard = _gameDeck.DrawCard();
        if (gameCard is not null)
        {
            _playerHand.TryAddCardToHand(gameCard);
        }
    }

    public void OnCardDropped(RectTransform dropArea, CardData cardData)
    {
        if (dropArea == HandArea)
        {
            Debug.Log("Hand");
        }
        else if (dropArea == DiscardArea)
        {
            Debug.Log("Discard");
        }
        else if (dropArea == StageArea)
        {
            Debug.Log("Stage");
        }
    }
}
