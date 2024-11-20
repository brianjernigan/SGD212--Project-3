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
    [SerializeField] private GameObject _deck;

    [Header("Texts")] 
    [SerializeField] private TMP_Text _scoreText;

    public GameObject Stage => _stage;
    public GameObject Discard => _discard;
    public GameObject Hand => _hand;
    
    public int CardsOnScreen => _playerHand.NumCardsInHand + _stageAreaController.NumCardsStaged;
    public int MaxCardsOnScreen { get; set; } = 5;

    private Deck _gameDeck;
    private Hand _playerHand;

    private Dictionary<CardData, int> _defaultDeckComposition;
    private Dictionary<string, ICardEffect> _cardEffects;
    
    private StageAreaController _stageAreaController;

    private Camera _mainCamera;

    private bool _isDrawingCards;
    public bool IsDraggingCard { get; set; }
    
    private int _currentScore;
    
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
            { _allPossibleCards[9], 4 },
            { _allPossibleCards[10], 2 },
            { _allPossibleCards[11], 2 },
            { _allPossibleCards[12], 2 },
            { _allPossibleCards[13], 2 },
            { _allPossibleCards[14], 2 },
            { _allPossibleCards[15], 2 }
        };
        
        // Define other decks? 
    }

    private void InitializeCardEffects()
    {
        _cardEffects = new Dictionary<string, ICardEffect>
        {
            {"Plankton", new PlanktonEffect(_playerHand, _gameDeck) },
            {"FishEggs", new FishEggsEffect(_playerHand, _gameDeck) },
            {"Seahorse", new SeahorseEffect(_playerHand, _gameDeck) },
            {"ClownFish", new ClownFishEffect(_playerHand, _gameDeck) },
            {"CookieCutter", new CookieCutterEffect(_playerHand, _gameDeck) },
            {"Turtle", new TurtleEffect(_playerHand, _gameDeck) },
            {"Stingray", new StingrayEffect(_playerHand, _gameDeck) },
            {"Bullshark", new BullsharkEffect(_playerHand, _gameDeck) },
            {"Hammerhead", new HammerheadEffect(_playerHand, _gameDeck) },
            {"Orca", new OrcaEffect(_playerHand, _gameDeck) },
            {"Anemone", new AnemoneEffect(_playerHand, _gameDeck) },
            {"Kraken", new KrakenEffect(_playerHand, _gameDeck) },
            {"Treasure", new TreasureEffect(_playerHand, _gameDeck) },
            {"Moray", new MorayEffect(_playerHand, _gameDeck) },
            {"Net", new NetEffect(_playerHand, _gameDeck) },
            {"Whaleshark", new WhaleSharkEffect(_playerHand, _gameDeck) }
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
            HandleLeftMouseClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightMouseClick();
        }
    }

    private void HandleLeftMouseClick()
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
    
    private void HandleRightMouseClick()
    {
        throw new NotImplementedException();
    }

    public void DrawFullHand()
    {
        if (!_isDrawingCards)
        {
            StartCoroutine(DrawFullHandCoroutine());
        }
    }

    private IEnumerator DrawFullHandCoroutine()
    {
        _isDrawingCards = true;
        
        while (CardsOnScreen < MaxCardsOnScreen && !_gameDeck.IsEmpty)
        {
            var gameCard = _gameDeck.DrawCard();
            if (gameCard is not null)
            {
                _playerHand.TryAddCardToHand(gameCard);

                var targetPosition = _handPositions[_playerHand.NumCardsInHand - 1].position;

                yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition));
            }
        }

        _isDrawingCards = false;
    }

    private IEnumerator DealCardCoroutine(GameCard gameCard, Vector3 targetPosition)
    {
        var cardTransform = gameCard.UI.transform;

        cardTransform.position = _deck.transform.position;

        var duration = 0.4f;
        var elapsed = 0f;

        var startPosition = cardTransform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        cardTransform.position = targetPosition;
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
                DiscardCard(gameCard);
                return true;
            }
        }
    
        return false;
    }

    private void DiscardCard(GameCard gameCard)
    {
        // Add to discard pile?
        Destroy(gameCard.UI.gameObject);
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
            _stageAreaController.CardsStaged[i].UI.transform.position = _stagePositions[i].position;
        }
    }

    public void OnClickPlayButton()
    {
        // Check cards in hand? 

        switch (_stageAreaController.NumCardsStaged)
        {
            case 1:
                TriggerCardEffect();
                break;
            case 3:
            case 4:
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
        if (_stageAreaController.NumCardsStaged == 4)
        {
            // Bonus for set of 4?
        }
        
        _currentScore += _stageAreaController.Score;
        _scoreText.text = $"Score: {_currentScore}";
        _stageAreaController.ClearStage();
    }

    public ICardEffect GetEffectForRank(string name)
    {
        return _cardEffects.GetValueOrDefault(name);
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
