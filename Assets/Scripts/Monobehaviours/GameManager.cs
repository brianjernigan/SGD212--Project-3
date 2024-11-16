using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Stores all possible cards for creating decks and starting the game
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<CardData> _allPossibleCards;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<Transform> _cardPositions;

    [Header("Areas")]
    [SerializeField] private GameObject _stage;
    [SerializeField] private GameObject _deck;
    [SerializeField] private GameObject _discard;
    [SerializeField] private GameObject _hand;
    
    public List<Transform> DropZones { get; set; }
    
    public int CardsOnScreen => _playerHand.NumCardsInHand + _stageAreaController.NumCardsStaged;
    public int MaxCardsOnScreen { get; set; } = 5;

    private Deck _gameDeck;
    private Hand _playerHand;

    private Dictionary<CardData, int> _defaultDeckComposition;
    private Dictionary<int, ICardEffect> _cardEffects;
    
    private StageAreaController _stageAreaController;
    
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

    private void InitializeDeckComposition()
    {
        _defaultDeckComposition = new Dictionary<CardData, int>
        {
            { _allPossibleCards[0], 4 },
            { _allPossibleCards[1], 4 },
            { _allPossibleCards[2], 4 },
            { _allPossibleCards[3], 4 },
            { _allPossibleCards[4], 4 },
            { _allPossibleCards[5], 4 },
            { _allPossibleCards[6], 4 },
            { _allPossibleCards[7], 4 },
            { _allPossibleCards[8], 4 },
            { _allPossibleCards[9], 4 }
        };
        
        // Define other decks? 
    }

    private void InitializeCardEffects()
    {
        _cardEffects = new Dictionary<int, ICardEffect>
        {
            {1, new SwapAndDiscard(_playerHand, _gameDeck) },
            {2, new SwapAndDiscard(_playerHand, _gameDeck) },
            {3, new SwapAndDiscard(_playerHand, _gameDeck) },
            {4, new SwapAndDiscard(_playerHand, _gameDeck) },
            {5, new SwapAndDiscard(_playerHand, _gameDeck) },
            {6, new SwapAndDiscard(_playerHand, _gameDeck) },
            {7, new SwapAndDiscard(_playerHand, _gameDeck) },
            {8, new SwapAndDiscard(_playerHand, _gameDeck) },
            {9, new SwapAndDiscard(_playerHand, _gameDeck) },
            {10, new SwapAndDiscard(_playerHand, _gameDeck) }
        };
    }

    private void InitializeDropZones()
    {
        DropZones = new List<Transform>()
        {
            _stage.transform,
            _discard.transform,
            _hand.transform
        };
    }

    private void Start()
    {
        InitializeDropZones();
        InitializeDeckComposition();

        _stageAreaController = _stage.GetComponent<StageAreaController>();
        
        _gameDeck = new Deck(_defaultDeckComposition, _cardPrefab, _cardPositions);
        _playerHand = new Hand();
        
        InitializeCardEffects();
        
        DrawFullHand();
    }

    public void DrawFullHand()
    {
        if (_gameDeck.IsEmpty) return;
        
        while (CardsOnScreen < MaxCardsOnScreen && !_gameDeck.IsEmpty)
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

    public bool OnCardDropped(Transform dropArea, GameCard gameCard)
    {
        // Destage
        if (dropArea == _hand.transform)
        {
            return _playerHand.TryAddCardToHand(gameCard) && _stageAreaController.TryRemoveCardFromStageArea(gameCard);
        }
        // Discard
        if (dropArea == _discard.transform)
        {
            if (!_playerHand.TryRemoveCardFromHand(gameCard) &&
                !_stageAreaController.TryRemoveCardFromStageArea(gameCard)) return false;
            Destroy(gameCard.UI.gameObject);
            return true;
        }
        // Stage Card
        if (dropArea == _stage.transform)
        {
            return _stageAreaController.TryAddCardToStageArea(gameCard) && _playerHand.TryRemoveCardFromHand(gameCard);
        }
    
        return false;
    }

    public void OnClickPlayButton()
    {
        var numCardsStaged = _stageAreaController.NumCardsStaged;
        
        // Check cards in hand? 

        switch (numCardsStaged)
        {
            case 1:
                TriggerCardEffect();
                break;
            case 3:
                ScoreSet();
                break;
            default:
                return;
        }
    }

    private void TriggerCardEffect()
    {
        var firstStagedCard = _stageAreaController.GetFirstStagedCard();
        if (firstStagedCard is null) return;
        
        _stageAreaController.ClearStageArea();

        firstStagedCard.ActivateEffect();
    }

    private void ScoreSet()
    {
        var score = _stageAreaController.Score;
        _stageAreaController.ClearStageArea();
    }

    public ICardEffect GetEffectForRank(int rank)
    {
        return _cardEffects.GetValueOrDefault(rank);
    }
}
