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
    
    [SerializeField] private List<Transform> _stagePositions;

    [Header("Areas")]
    [SerializeField] private GameObject _stage;
    [SerializeField] private GameObject _discard;
    [SerializeField] private GameObject _hand;
    [SerializeField] private GameObject _deck;

    [Header("Texts")] 
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _playText;
    [SerializeField] private TMP_Text _discardText;
    [SerializeField] private TMP_Text _multiplierText;

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

    private const float DockWidth = 750f;
    // If we want curved hand layout
    private const float CurveStrength = -0.001f;
    
    public int PlaysRemaining { get; set; }
    public int DiscardsRemaining { get; set; }
    public int PlayerMoney { get; set; }
    
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
        
        _gameDeck = new Deck(_defaultDeckComposition, _cardPrefab);
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
        if (_mainCamera is null) return;

        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit))
        {
            var clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Card"))
            {
                FlipCard(clickedObject);
            }
        }
    }

    private void FlipCard(GameObject card)
    {
        var rb = card.GetComponent<Rigidbody>();

        if (rb is not null)
        {
            Debug.Log("howdy");
        }
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

                var dockCenter = _hand.transform.position;
                var targetPosition = CalculateCardPosition(
                    _playerHand.NumCardsInHand - 1,
                    _playerHand.NumCardsInHand,
                    dockCenter
                );
                
                // var targetPosition = _handPositions[_playerHand.NumCardsInHand - 1].position;

                yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition));
            }
        }

        _isDrawingCards = false;
    }

    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);

        var totalSpacing = Mathf.Max(DockWidth, 0.1f);
        var startX = -totalSpacing / 2f;
        var xPosition = startX + (cardIndex * (DockWidth / Mathf.Max(1, totalCards - 1)));
        
        var zPosition = Mathf.Pow(xPosition / Mathf.Max(totalSpacing, 1f), 2) * CurveStrength;

        return dockCenter + new Vector3(xPosition, 25 + cardIndex, zPosition);
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
                PlaceCardInHand(gameCard);
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
        var dockCenter = _hand.transform.position;

        for (var i = 0; i < _playerHand.NumCardsInHand; i++)
        {
            var card = _playerHand.CardsInHand[i];
            var targetPosition = CalculateCardPosition(i, _playerHand.NumCardsInHand, dockCenter);

            card.UI.transform.position = targetPosition;
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
        if (_stageAreaController.NumCardsStaged == 0) return;

        switch (_stageAreaController.NumCardsStaged)
        {
            case 1:
                if (_stageAreaController.GetFirstStagedCard().Data.CardName != "Kraken")
                {
                    TriggerCardEffect();
                }
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

        _currentScore += _stageAreaController.CalculateScore();
        _scoreText.text = $"Score: {_currentScore}";
        _stageAreaController.ClearStage();
    }

    public ICardEffect GetEffectForRank(string cardName)
    {
        return _cardEffects.GetValueOrDefault(cardName);
    }

    public void PlaceCardInHand(GameCard gameCard)
    {
        var dockCenter = _hand.transform.position;

        var targetPosition = CalculateCardPosition(_playerHand.NumCardsInHand - 1, _playerHand.NumCardsInHand, dockCenter);

        gameCard.UI.transform.position = targetPosition;
        
        RearrangeHand();
    }

    private void PlaceCardInStage(GameCard gameCard)
    {
        gameCard.UI.transform.position = _stagePositions[_stageAreaController.NumCardsStaged - 1].transform.position;
    }

    public void AddCardToDeck(CardData data, int count = 1)
    {
        _gameDeck?.AddCard(data, count);
    }
}
