using System;
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
    
    [Header("Card Positions")]
    [SerializeField] private List<Transform> _handPositions;
    [SerializeField] private List<Transform> _stagePositions;

    [Header("Areas")]
    [SerializeField] private GameObject _stage;
    [SerializeField] private GameObject _discard;
    [SerializeField] private GameObject _hand;

    public GameObject Stage => _stage;
    public GameObject Discard => _discard;
    public GameObject Hand => _hand;
    
    public int CardsOnScreen => _playerHand.NumCardsInHand + _stageAreaController.NumCardsStaged;
    public int MaxCardsOnScreen { get; set; } = 5;

    private Deck _gameDeck;
    private Hand _playerHand;

    private Dictionary<CardData, int> _defaultDeckComposition;
    private Dictionary<int, ICardEffect> _cardEffects;
    
    private StageAreaController _stageAreaController;

    private Camera _mainCamera;
    
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

    private void Start()
    {
        _mainCamera = Camera.main;
        
        InitializeDeckComposition();

        _stageAreaController = _stage.GetComponent<StageAreaController>();
        
        _gameDeck = new Deck(_defaultDeckComposition, _cardPrefab, _handPositions);
        _playerHand = new Hand();
        
        InitializeCardEffects();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        if (_mainCamera is null) return;

        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit))
        {
            var clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("DrawButton"))
            {
                DrawFullHand();
            }

            if (clickedObject.CompareTag("PlayButton"))
            {
                OnClickPlayButton();
            }
        }
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

    public bool TryDropCard(Transform dropArea, GameCard gameCard)
    {
        // Destage
        if (dropArea == _hand.transform)
        {
            if (_playerHand.TryAddCardToHand(gameCard) && _stageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard, false);
                return true;
            }
        }
        // Stage Card
        if (dropArea == _stage.transform)
        {
            if (_stageAreaController.TryAddCardToStage(gameCard) && _playerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                return true;
            }
        }
        // Discard
        if (dropArea == _discard.transform)
        {
            if (_playerHand.TryRemoveCardFromHand(gameCard) || _stageAreaController.TryRemoveCardFromStage(gameCard))
            {
                Destroy(gameCard.UI.gameObject);
                return true;
            }
        }
    
        return false;
    }

    public void RearrangeHand()
    {
        for (var i = 0; i < _playerHand.NumCardsInHand; i++)
        {
            _playerHand.CardsInHand[i].UI.transform.position = _handPositions[i].position;
        }
    }

    public void RearrangeStage()
    {
        for (var i = 0; i < _stageAreaController.NumCardsStaged; i++)
        {
            _stageAreaController.CardStaged[i].UI.transform.position = _stagePositions[i].position;
        }
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
        
        _stageAreaController.ClearStage();

        firstStagedCard.ActivateEffect();
    }

    private void ScoreSet()
    {
        var score = _stageAreaController.Score;
        _stageAreaController.ClearStage();
    }

    public ICardEffect GetEffectForRank(int rank)
    {
        return _cardEffects.GetValueOrDefault(rank);
    }

    public void PlaceCardInHand(GameCard gameCard, bool isDrawing)
    {
        // Drawing creates card before placing in hand -> 0 index
        // Dragging adds card to hand then places -> -1 index
        var index = isDrawing ? _playerHand.NumCardsInHand : _playerHand.NumCardsInHand - 1;
        gameCard.UI.transform.position = _handPositions[index].transform.position;
    }

    private void PlaceCardInStage(GameCard gameCard)
    {
        gameCard.UI.transform.position = _stagePositions[_stageAreaController.NumCardsStaged - 1].transform.position;
    }
}
