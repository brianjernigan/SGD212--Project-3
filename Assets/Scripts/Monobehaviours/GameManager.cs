using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the core game mechanics, including drawing cards, handling interactions,
/// managing the deck and player's hand, and integrating visual and audio effects.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance { get; private set; }

    [Header("Card Setup")]
    [SerializeField] private List<CardData> _allPossibleCards;      // All available card types
    [SerializeField] private GameObject _cardPrefab;                // Prefab for creating card instances

    [Header("Stage Positions")]
    [SerializeField] private List<Transform> _stagePositions;       // Positions where staged cards will be placed

    [Header("Areas")]
    [SerializeField] private GameObject _stage;                     // Stage area GameObject
    [SerializeField] private GameObject _discard;                   // Discard pile GameObject
    [SerializeField] private GameObject _hand;                      // Player's hand GameObject
    [SerializeField] private GameObject _deck;                      // Deck area GameObject

    [Header("UI Texts")]
    [SerializeField] private TMP_Text _scoreText;                   // UI Text for displaying score
    [SerializeField] private TMP_Text _moneyText;                   // UI Text for displaying money
    [SerializeField] private TMP_Text _playText;                    // UI Text for play button
    [SerializeField] private TMP_Text _discardText;                 // UI Text for discard button
    [SerializeField] private TMP_Text _multiplierText;              // UI Text for multiplier

    [Header("Audio")]
    [SerializeField] private AudioClip cardDrawSound;               // Sound played when a card is drawn
    [SerializeField] private AudioClip cardClickSound;              // Sound played when a card is clicked
    [SerializeField] private AudioClip cardFlipSound;               // Sound played when a card is flipped
    [SerializeField] private AudioSource audioSource;               // AudioSource component for playing sounds

    [Header("Visual Effects")]
    [SerializeField] private GameObject cardDrawParticlesPrefab;    // Particle effect prefab for card draw
    [SerializeField] private Material glowMaterial;                 // Material with glow effect (optional)
    [SerializeField] private Material originalMaterial;             // Original material of the card

    // Properties for accessing areas
    public GameObject Stage => _stage;
    public GameObject Discard => _discard;
    public GameObject Hand => _hand;

    // Hand and Deck Management
    public int CardsOnScreen => PlayerHand.NumCardsInHand + _stageAreaController.NumCardsStaged;
    public int MaxCardsOnScreen { get; set; } = 5;

    public Deck GameDeck { get; set; }
    public Hand PlayerHand { get; private set; }

    // Internal Data Structures
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
        // Implement Singleton Pattern
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

    /// <summary>
    /// Initializes the default composition of the deck based on available cards.
    /// </summary>
    private void InitializeDeckComposition()
    {
        _defaultDeckComposition = new Dictionary<CardData, int>();
        foreach (var card in _allPossibleCards)
        {
            // Assuming each card has a predefined count. Modify as needed.
            _defaultDeckComposition[card] = 4; // Example: 4 of each card
        }
    }

    /// <summary>
    /// Initializes the effects associated with each card.
    /// </summary>
    private void InitializeCardEffects()
    {
        _cardEffects = new Dictionary<string, ICardEffect>
        {
            {"Plankton", new PlanktonEffect(PlayerHand, GameDeck) },
            {"FishEggs", new FishEggsEffect(PlayerHand, GameDeck) },
            {"Seahorse", new SeahorseEffect(PlayerHand, GameDeck) },
            {"ClownFish", new ClownFishEffect(PlayerHand, GameDeck) },
            {"CookieCutter", new CookieCutterEffect(PlayerHand, GameDeck) },
            {"Turtle", new TurtleEffect(PlayerHand, GameDeck) },
            {"Stingray", new StingrayEffect(PlayerHand, GameDeck) },
            {"Bullshark", new BullsharkEffect(PlayerHand, GameDeck) },
            {"Hammerhead", new HammerheadEffect(PlayerHand, GameDeck) },
            {"Orca", new OrcaEffect(PlayerHand, GameDeck) },
            {"Anemone", new AnemoneEffect(PlayerHand, GameDeck) },
            {"Kraken", new KrakenEffect(PlayerHand, GameDeck) },
            {"Treasure", new TreasureEffect(PlayerHand, GameDeck) },
            {"Moray", new MorayEffect(PlayerHand, GameDeck) },
            {"Net", new NetEffect(PlayerHand, GameDeck) },
            {"Whaleshark", new WhaleSharkEffect(PlayerHand, GameDeck) }
        };
    }

    private void Start()
    {
        _mainCamera = Camera.main;

        InitializeDeckComposition();

        _stageAreaController = _stage.GetComponent<StageAreaController>();

        PlayerHand = new Hand();
        GameDeck = new Deck(_defaultDeckComposition, _cardPrefab);

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

    /// <summary>
    /// Handles left mouse button clicks for various interactions.
    /// </summary>
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
            else if (clickedObject.CompareTag("PlayButton"))
            {
                OnClickPlayButton();
            }
            else if (clickedObject.CompareTag("Card"))
            {
                // Play card click sound via AudioSource
                if (audioSource != null && cardClickSound != null)
                {
                    audioSource.PlayOneShot(cardClickSound);
                }

                // Optional: Additional logic for left-clicking a card can be added here
            }
        }
    }

    /// <summary>
    /// Handles right mouse button clicks, typically used for flipping cards.
    /// </summary>
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

    /// <summary>
    /// Initiates the card flip animation and manages related effects.
    /// </summary>
    /// <param name="detectionCollider">The collider that detected the flip.</param>
    private void FlipCard(GameObject detectionCollider)
    {
        var parentObject = detectionCollider.transform.parent.gameObject;

        // Disable idle animation during the flip
        var idleAnimation = parentObject.GetComponent<CardIdleAnimation>();
        if (idleAnimation != null)
        {
            idleAnimation.DisableIdleAnimation();
        }

        StartCoroutine(FlipCardCoroutine(parentObject, idleAnimation));
    }

    /// <summary>
    /// Coroutine to smoothly flip a card over a specified duration.
    /// </summary>
    /// <param name="card">The card GameObject to flip.</param>
    /// <param name="idleAnimation">The CardIdleAnimation component of the card.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator FlipCardCoroutine(GameObject card, CardIdleAnimation idleAnimation)
    {
        var startRotation = card.transform.rotation;
        var endRotation = card.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        var duration = 0.75f; // Duration of the flip
        var elapsedTime = 0f;

        // Play flip sound via AudioSource
        if (audioSource != null && cardFlipSound != null)
        {
            audioSource.PlayOneShot(cardFlipSound);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smoothstep interpolation for smooth easing
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep formula

            card.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        card.transform.rotation = endRotation; // Ensure exact final rotation

        // Re-enable idle animation after the flip
        if (idleAnimation != null)
        {
            idleAnimation.EnableIdleAnimation();
        }
    }

    /// <summary>
    /// Initiates the process of drawing cards to fill the player's hand.
    /// </summary>
    public void DrawFullHand()
    {
        if (!_isDrawingCards)
        {
            StartCoroutine(DrawFullHandCoroutine());
        }
    }

    /// <summary>
    /// Coroutine to handle the asynchronous drawing of multiple cards.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator DrawFullHandCoroutine()
    {
        _isDrawingCards = true;
        _isUpdatingHand = true;

        int cardsToDraw = MaxCardsOnScreen - CardsOnScreen;
        int startingHandSize = PlayerHand.NumCardsInHand;

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
            var card = PlayerHand.CardsInHand[i];
            var targetPosition = finalPositions[i];
            StartMoveCardToPosition(card.UI.transform, targetPosition);
        }

        // Deal new cards directly to their final positions
        for (int i = 0; i < cardsToDraw && !GameDeck.IsEmpty; i++)
        {
            var gameCard = GameDeck.DrawCard();
            if (gameCard != null)
            {
                PlayerHand.TryAddCardToHand(gameCard);

                var targetPosition = finalPositions[startingHandSize + i];

                yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition, i));
            }
        }

        _isUpdatingHand = false;
        _isDrawingCards = false;
    }

    /// <summary>
    /// Calculates the position of a card in the player's hand based on its index.
    /// </summary>
    /// <param name="cardIndex">Index of the card in hand.</param>
    /// <param name="totalCards">Total number of cards in hand.</param>
    /// <param name="dockCenter">Center position of the hand dock.</param>
    /// <returns>Calculated Vector3 position.</returns>
    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);

        var totalSpacing = Mathf.Max(DockWidth, 0.1f);
        var startX = -totalSpacing / 2f;
        // Reverse the order so the first card is on the right
        var xPosition = startX + ((totalCards - 1 - cardIndex) * (DockWidth / Mathf.Max(1, totalCards - 1)));

        var zPosition = Mathf.Pow(xPosition / Mathf.Max(totalSpacing, 1f), 2) * CurveStrength;

        // Adjusted Y position by adding 10 units
        return dockCenter + new Vector3(xPosition, 10f, zPosition);
    }

    /// <summary>
    /// Coroutine to handle the animation and effects when dealing a card.
    /// </summary>
    /// <param name="gameCard">The GameCard being dealt.</param>
    /// <param name="targetPosition">The final position of the card.</param>
    /// <param name="cardIndex">Index of the card in the draw sequence.</param>
    /// <returns>IEnumerator for coroutine.</returns>
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

        // Play card draw sound via AudioSource
        if (audioSource != null && cardDrawSound != null)
        {
            audioSource.PlayOneShot(cardDrawSound);
        }

        // Animation parameters
        var duration = 1.0f; // Duration of the arc movement
        var bounceDuration = 0.25f; // Duration of the bounce back
        var elapsedTime = 0f;

        var startPosition = cardTransform.position;

        // Adjust starting position based on cardIndex to prevent overlap
        float offsetX = cardIndex * 100f; // Adjust the value as needed for spacing
        startPosition += new Vector3(offsetX, 0f, 0f);

        // Adjust overshoot position's Y-coordinate for a slight arc
        var overshootPosition = targetPosition + new Vector3(0f, 1.5f, 0f); // Slight overshoot above final position

        // Move to overshoot position (arc movement)
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smoothstep easing for smooth motion
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep formula

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

            // Maintain rotation
            cardTransform.rotation = Quaternion.Euler(90f, 0f, 180f);

            yield return null;
        }

        // Ensure final position and rotation
        cardTransform.position = targetPosition;
        cardTransform.rotation = Quaternion.Euler(90f, 0f, 180f); // Final rotation

        // Instantiate particle effect at the card's position
        if (cardDrawParticlesPrefab != null)
        {
            GameObject particles = Instantiate(cardDrawParticlesPrefab, targetPosition, Quaternion.identity, cardTransform);
            Destroy(particles, particles.GetComponent<ParticleSystem>().main.duration);
        }

        // OPTIONAL: Apply glow effect to the newly drawn card
        /*
        Renderer cardRenderer = cardTransform.GetComponent<Renderer>();
        if (cardRenderer != null && glowMaterial != null)
        {
            cardRenderer.material = glowMaterial;
            StartCoroutine(RevertMaterial(cardRenderer, originalMaterial, 0.5f));
        }
        */

        // Unlock animation
        gameCard.IsAnimating = false;
    }

    /// <summary>
    /// Coroutine to revert the card's material back to the original after applying a glow.
    /// </summary>
    /// <param name="renderer">Renderer of the card.</param>
    /// <param name="originalMat">Original material to revert to.</param>
    /// <param name="delay">Delay before reverting.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator RevertMaterial(Renderer renderer, Material originalMat, float delay)
    {
        yield return new WaitForSeconds(delay);
        renderer.material = originalMat;
    }

    /// <summary>
    /// Initiates the movement of a card to a specified position with smooth animation.
    /// </summary>
    /// <param name="cardTransform">Transform of the card to move.</param>
    /// <param name="targetPosition">Target position for the card.</param>
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

    /// <summary>
    /// Coroutine to smoothly move a card to its target position.
    /// </summary>
    /// <param name="cardTransform">Transform of the card to move.</param>
    /// <param name="targetPosition">Destination position for the card.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator MoveCardToPositionCoroutine(Transform cardTransform, Vector3 targetPosition)
    {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 startPosition = cardTransform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Smoothstep easing for smooth motion
            t = t * t * (3f - 2f * t);

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cardTransform.position = targetPosition;

        // Remove coroutine from tracking dictionary
        _moveCardCoroutines.Remove(cardTransform);
    }

    /// <summary>
    /// Attempts to drop a card into a specified area (hand, stage, or discard).
    /// </summary>
    /// <param name="dropArea">The area where the card is being dropped.</param>
    /// <param name="gameCard">The card being dropped.</param>
    /// <returns>True if the drop was successful; otherwise, false.</returns>
    public bool TryDropCard(Transform dropArea, GameCard gameCard)
    {
        if (_isUpdatingHand)
            return false;

        // Destage: Move card back to hand
        if (dropArea == _hand.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && _stageAreaController.TryRemoveCardFromStage(gameCard))
            {
                RearrangeHand(); // Rearrange hand when cards are manually added
                return true;
            }
        }
        // Stage Card: Move card to stage area
        else if (dropArea == _stage.transform)
        {
            if (_stageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                RearrangeHand(); // Rearrange hand when cards are manually removed
                return true;
            }
        }
        // Discard: Move card to discard pile
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

    /// <summary>
    /// Handles the discarding of a card by destroying its GameObject.
    /// </summary>
    /// <param name="gameCard">The card to discard.</param>
    private void DiscardCard(GameCard gameCard)
    {
        // Add to discard pile logic can be implemented here
        Destroy(gameCard.UI.gameObject);
    }

    /// <summary>
    /// Rearranges the player's hand by repositioning all cards smoothly.
    /// </summary>
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

    /// <summary>
    /// Rearranges the stage area by repositioning all staged cards smoothly.
    /// </summary>
    public void RearrangeStage()
    {
        for (var i = 0; i < _stageAreaController.NumCardsStaged; i++)
        {
            // Adjusted Y position by adding 10 units and rotated 180 degrees on Z-axis
            _stageAreaController.CardsStaged[i].UI.transform.position = _stagePositions[i].position + new Vector3(0f, 10f, 0f);
            _stageAreaController.CardsStaged[i].UI.transform.rotation = Quaternion.Euler(90f, 0f, 180f);
        }
    }

    /// <summary>
    /// Handles the logic when the play button is clicked.
    /// </summary>
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

    /// <summary>
    /// Triggers the effect associated with the first staged card.
    /// </summary>
    private void TriggerCardEffect()
    {
        var firstStagedCard = _stageAreaController.GetFirstStagedCard();
        if (firstStagedCard is null) return;

        _stageAreaController.ClearStage();

        firstStagedCard.ActivateEffect();
    }

    /// <summary>
    /// Calculates and updates the player's score based on the staged cards.
    /// </summary>
    private void ScoreSet()
    {
        if (_stageAreaController.NumCardsStaged == 4)
        {
            // Implement bonus for a set of 4 if desired
        }

        _currentScore += _stageAreaController.CalculateScore();
        _scoreText.text = $"Score: {_currentScore}";
        _stageAreaController.ClearStage();
    }

    /// <summary>
    /// Retrieves the card effect associated with a given card name.
    /// </summary>
    /// <param name="cardName">Name of the card.</param>
    /// <returns>The corresponding ICardEffect.</returns>
    public ICardEffect GetEffectForRank(string cardName)
    {
        return _cardEffects.GetValueOrDefault(cardName);
    }

    /// <summary>
    /// Places a card in the player's hand and initiates rearrangement.
    /// </summary>
    /// <param name="gameCard">The card to place in hand.</param>
    public void PlaceCardInHand(GameCard gameCard)
    {
        var dockCenter = _hand.transform.position;
        var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, dockCenter);

        gameCard.UI.transform.position = targetPosition;

        RearrangeHand();
    }

    /// <summary>
    /// Places a card in the stage area and sets its position and rotation.
    /// </summary>
    /// <param name="gameCard">The card to place on stage.</param>
    private void PlaceCardInStage(GameCard gameCard)
    {
        // Adjusted Y position by adding 10 units and rotated 180 degrees on Z-axis
        gameCard.UI.transform.position = _stagePositions[_stageAreaController.NumCardsStaged - 1].transform.position + new Vector3(0f, 10f, 0f);
        gameCard.UI.transform.rotation = Quaternion.Euler(90f, 0f, 180f);
    }

    /// <summary>
    /// Adds a specified number of cards of a particular type to the deck.
    /// </summary>
    /// <param name="data">The CardData representing the type of card.</param>
    /// <param name="count">Number of cards to add.</param>
    public void AddCardToDeck(CardData data, int count = 1)
    {
        GameDeck?.AddCard(data, count);
    }
}
