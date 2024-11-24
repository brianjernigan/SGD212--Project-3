using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    public int CardsOnScreen => PlayerHand.NumCardsInHand + _stageAreaController.NumCardsStaged;
    public int MaxCardsOnScreen { get; set; } = 5;

    public Deck GameDeck { get; set; }
    public Hand PlayerHand { get; private set; }

    private Dictionary<CardData, int> _defaultDeckComposition;
    private Dictionary<string, ICardEffect> _cardEffects;

    private StageAreaController _stageAreaController;

    private Camera _mainCamera;

    private bool _isDrawingCards;
    private bool _isUpdatingHand; // Flag to indicate if the hand is being updated
    public bool IsDraggingCard { get; set; }

    private int _currentScore;

    private const float DockWidth = 750f;
    private const float CurveStrength = -0.001f;

    public int PlaysRemaining { get; set; }
    public int DiscardsRemaining { get; set; }
    public int PlayerMoney { get; set; }

    // Dictionary to track ongoing movement coroutines for each card
    private Dictionary<Transform, Coroutine> _moveCardCoroutines = new Dictionary<Transform, Coroutine>();

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
        
        PlayerHand = new Hand();
        GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);
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

    private void FlipCard(GameObject detectionCollider)
    {
        var parentObject = detectionCollider.transform.parent.gameObject;
        StartCoroutine(FlipCardCoroutine(parentObject));
    }

    private IEnumerator FlipCardCoroutine(GameObject card)
    {
        var startRotation = card.transform.rotation;
        var endRotation = card.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        var duration = 0.75f; // Increased duration for smoother animation
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smooth step interpolation (ease-in-out effect)
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep easing formula

            card.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        card.transform.rotation = endRotation; // Ensure it ends exactly at the target rotation
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
        _isUpdatingHand = true;

        int cardsToDraw = MaxCardsOnScreen - CardsOnScreen;
        int startingHandSize = _playerHand.NumCardsInHand;

        // Compute final positions for all cards in hand
        var dockCenter = _hand.transform.position;
        List<Vector3> finalPositions = new List<Vector3>();
        int totalCardsInHand = startingHandSize + cardsToDraw;

        for (int i = 0; i < totalCardsInHand; i++)
        {
            var position = CalculateCardPosition(i, totalCardsInHand, dockCenter);
            finalPositions.Add(position);
        }

        // Move existing cards to their final positions
        for (int i = 0; i < startingHandSize; i++)
        {
            var card = _playerHand.CardsInHand[i];
            var targetPosition = finalPositions[i];
            StartMoveCardToPosition(card.UI.transform, targetPosition);
        }

        // Deal new cards directly to their final positions
        for (int i = 0; i < cardsToDraw && !_gameDeck.IsEmpty; i++)
        {
            var gameCard = GameDeck.DrawCard();
            if (gameCard is not null)
            {
                PlayerHand.TryAddCardToHand(gameCard);
                
                var targetPosition = finalPositions[startingHandSize + i];

                yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition, i));
            }
        }

        _isUpdatingHand = false;
        _isDrawingCards = false;
    }

    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);

        var totalSpacing = Mathf.Max(DockWidth, 0.1f);
        var startX = -totalSpacing / 2f;
        // Reverse the order so first card goes to the furthest right
        var xPosition = startX + ((totalCards - 1 - cardIndex) * (DockWidth / Mathf.Max(1, totalCards - 1)));

        var zPosition = Mathf.Pow(xPosition / Mathf.Max(totalSpacing, 1f), 2) * CurveStrength;

        // Adjusted Y position by adding 10 units
        return dockCenter + new Vector3(xPosition, 10f, zPosition);
    }

    private IEnumerator DealCardCoroutine(GameCard gameCard, Vector3 targetPosition, int cardIndex)
    {
        var cardTransform = gameCard.UI.transform;

        // Lock animation
        gameCard.IsAnimating = true;

        // Set initial position and rotation
        cardTransform.position = _deck.transform.position;
        cardTransform.rotation = Quaternion.Euler(90f, 0f, 180f); // Ensure the card starts flat and rotated 180 degrees on Z

        // Adjust initial position's Y-coordinate
        cardTransform.position += new Vector3(0f, 10f, 0f);

        AudioManager.Instance.PlayCardDrawAudio();

        // Animation parameters
        var duration = 1.0f; // Animation duration
        var bounceDuration = 0.25f; // Bounce-back duration
        var elapsedTime = 0f;

        var startPosition = cardTransform.position;

        // Adjust starting position based on cardIndex
        float offsetX = cardIndex * 100f; // Adjust the value as needed
        startPosition += new Vector3(offsetX, 0f, 0f);

        // Adjust overshoot position's Y-coordinate
        var overshootPosition = targetPosition + new Vector3(0f, 1.5f, 0f); // Slight overshoot above final position

        // Move to overshoot position
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smoothstep easing
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            Vector3 arcPosition = Vector3.Lerp(startPosition, overshootPosition, t);
            cardTransform.position = arcPosition;

            // Maintain rotation to keep the card flat and rotated 180 degrees on Z
            cardTransform.rotation = Quaternion.Euler(90f, 0f, 180f);

            yield return null;
        }

        // Bounce back to final position
        elapsedTime = 0f; // Reset elapsed time for bounce
        Vector3 bounceStartPosition = cardTransform.position;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;

            // Smoothstep easing
            float t = elapsedTime / bounceDuration;
            t = t * t * (3f - 2f * t);

            Vector3 bouncePosition = Vector3.Lerp(bounceStartPosition, targetPosition, t);
            cardTransform.position = bouncePosition;

            // Maintain rotation to keep the card flat and rotated 180 degrees on Z
            cardTransform.rotation = Quaternion.Euler(90f, 0f, 180f);

            yield return null;
        }

        // Ensure final position and rotation
        cardTransform.position = targetPosition;
        cardTransform.rotation = Quaternion.Euler(90f, 0f, 180f); // Final rotation

        // Unlock animation
        gameCard.IsAnimating = false;
    }

    private void StartMoveCardToPosition(Transform cardTransform, Vector3 targetPosition)
    {
        // Stop any existing coroutine for this card
        if (_moveCardCoroutines.TryGetValue(cardTransform, out Coroutine existingCoroutine))
        {
            StopCoroutine(existingCoroutine);
            _moveCardCoroutines.Remove(cardTransform);
        }

        // Start new coroutine
        Coroutine newCoroutine = StartCoroutine(MoveCardToPositionCoroutine(cardTransform, targetPosition));
        _moveCardCoroutines.Add(cardTransform, newCoroutine);
    }

    private IEnumerator MoveCardToPositionCoroutine(Transform cardTransform, Vector3 targetPosition)
    {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 startPosition = cardTransform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Smoothstep easing
            t = t * t * (3f - 2f * t);

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cardTransform.position = targetPosition;

        // Remove coroutine from tracking dictionary
        _moveCardCoroutines.Remove(cardTransform);
    }

    public bool TryDropCard(Transform dropArea, GameCard gameCard)
    {
        if (_isUpdatingHand)
            return false;

        // Destage
        if (dropArea == _hand.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && _stageAreaController.TryRemoveCardFromStage(gameCard))
            {
                RearrangeHand(); // Rearrange hand when cards are manually added
                return true;
            }
        }
        // Stage Card
        else if (dropArea == _stage.transform)
        {
            if (_stageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                RearrangeHand(); // Rearrange hand when cards are manually removed
                return true;
            }
        }
        // Discard
        else if (dropArea == _discard.transform)
        {
            if (DiscardsRemaining == 0) return false;
            
            if (PlayerHand.TryRemoveCardFromHand(gameCard) || _stageAreaController.TryRemoveCardFromStage(gameCard))
            {
                DiscardCard(gameCard);
                RearrangeHand(); // Rearrange hand when a card is discarded
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
        if (_isUpdatingHand)
            return;

        var dockCenter = _hand.transform.position;

        for (var i = 0; i < PlayerHand.NumCardsInHand; i++)
        {
            var card = PlayerHand.CardsInHand[i];
            var targetPosition = CalculateCardPosition(i, PlayerHand.NumCardsInHand, dockCenter);

            StartMoveCardToPosition(card.UI.transform, targetPosition);
        }
    }

    public void RearrangeStage()
    {
        for (var i = 0; i < _stageAreaController.NumCardsStaged; i++)
        {
            // Adjusted Y position by adding 10 units and rotated 180 degrees on Z-axis
            _stageAreaController.CardsStaged[i].UI.transform.position = _stagePositions[i].position + new Vector3(0f, 10f, 0f);
            _stageAreaController.CardsStaged[i].UI.transform.rotation = Quaternion.Euler(90f, 0f, 180f);
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

    public void PlaceCardInHand(GameCard gameCard)
    {
        var dockCenter = _hand.transform.position;
        var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, dockCenter);
        
        gameCard.UI.transform.position = targetPosition;
        
        RearrangeHand();
    }

    private void PlaceCardInStage(GameCard gameCard)
    {
        // Adjusted Y position by adding 10 units and rotated 180 degrees on Z-axis
        gameCard.UI.transform.position = _stagePositions[_stageAreaController.NumCardsStaged - 1].transform.position + new Vector3(0f, 10f, 0f);
        gameCard.UI.transform.rotation = Quaternion.Euler(90f, 0f, 180f);
    }

    public void AddCardToDeck(CardData data, int count = 1)
    {
        GameDeck?.AddCard(data, count);
    }
}
