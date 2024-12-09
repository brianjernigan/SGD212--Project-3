using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Stores all possible cards for creating decks and starting the game
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<Transform> _stagePositions;
    [SerializeField] private GameObject _shelly;

    [Header("Areas")]
    [SerializeField] private GameObject _stageArea;
    [SerializeField] private GameObject _discardArea;
    [SerializeField] private GameObject _deck;
    [SerializeField] private Transform _whirlpoolCenter; // Added for spiral animation
    
    [SerializeField] private List<GameObject> _levels;
    [SerializeField] private List<GameObject> _handAreas;
    public GameObject StageArea => _stageArea;
    public GameObject DiscardArea => _discardArea;
    public GameObject HandArea { get; private set; }
    private int _levelIndex = 1;
    
    public int NumCardsOnScreen => PlayerHand.NumCardsInHand + StageAreaController.NumCardsStaged;
    public int DefaultHandSize => 5;
    public int AdditionalCardsDrawn { get; set; }
    public int PermanentHandSizeModifier { get; set; }
    public int HandSize => DefaultHandSize + AdditionalCardsDrawn + PermanentHandSizeModifier;

    public Deck GameDeck { get; set; }
    public Hand PlayerHand { get; private set; }
    
    public StageAreaController StageAreaController { get; private set; }
    public ShellyController ShellyController { get; private set; }

    private Camera _mainCamera;

    public bool IsDrawingCards { get; set; }
    public bool IsDraggingCard { get; set; }
    public bool IsFlippingCard { get; set; }
    
    public int CurrentScore { get; set; }
    public int CurrentMultiplier { get; set; } = 1;

    private const float DockWidth = 750f;
    private float _initialCardY;
    
    private readonly float _spiralDuration = 1.0f; // Duration of the spiral animation
    private readonly float _spiralRadius = 5.0f;    // Starting radius of the spiral
    private readonly float _spiralDepth = 2.0f;     // Depth the card moves downward
    private readonly float _spiralRotationSpeed = 360f; // Degrees per second

    public int PlaysRemaining { get; set; } = 5;
    public int DiscardsRemaining { get; set; } = 5;
    public int DrawsRemaining { get; set; } = 5;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnDiscardsChanged;
    public event Action<int> OnDrawsChanged;
    public event Action<int> OnMultiplierChanged;
    public event Action<int> OnHandSizeChanged;
    public event Action<int> OnCardsRemainingChanged;

    [Header("Dialogue Settings")]
    [Tooltip("Enable or disable normal dialogues.")]
    [SerializeField] private bool enableNormalDialogue = true;
    public bool EnableNormalDialogue
    {
        get => enableNormalDialogue;
        set => enableNormalDialogue = value;
    }

    [Header("Game Mode Settings")]
    [SerializeField] private bool _isTutorialMode = true; 
    public bool IsTutorialMode
    {
        get => _isTutorialMode;
        set => _isTutorialMode = value;
    }

    [Header("Dialogue State")]
    [SerializeField] private bool isShowingTutorialDialogue = false;
    [SerializeField] private bool isShowingNormalDialogue = false;

    private const int BaseRequiredScore = 50;
    public int CurrentRequiredScore => BaseRequiredScore * _levelIndex;

    #region Helpers

    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);

        var cardSpacing = Mathf.Min(DockWidth / totalCards, 140f); // Dynamic spacing with a max cap
        var startX = -((totalCards - 1) * cardSpacing) / 2f;
        var xPosition = startX + (cardIndex * cardSpacing);

        var handCollider = HandArea.GetComponent<BoxCollider>();
        
        return dockCenter + new Vector3(xPosition, handCollider.transform.TransformPoint(handCollider.center).y + cardIndex, 0f); // Straight line with fixed Y
    }

    public void PlaceCardInHand(GameCard gameCard, bool isFromCard)
    {
        var handCenter = HandArea.transform.position;

        var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, handCenter);
        
        gameCard.UI.transform.position = targetPosition;

        if (!isFromCard)
        {
            AudioManager.Instance.PlayBackToHandAudio();
        }
     
        gameCard.IsInHand = true;
        gameCard.IsStaged = false;
        
        RearrangeHand();
    }

    private void PlaceCardInStage(GameCard gameCard)
    {
        gameCard.UI.transform.position = _stagePositions[StageAreaController.NumCardsStaged - 1].transform.position;
        var vector3 = gameCard.UI.transform.position;
        vector3.y = _initialCardY;
        gameCard.UI.transform.position = vector3;
        
        AudioManager.Instance.PlayStageCardAudio();
        
        gameCard.IsStaged = true;
        gameCard.IsInHand = false;
        
        RearrangeStage();
    }
    
    public void RearrangeHand()
    {
        var dockCenter = HandArea.transform.position;
        for (var i = 0; i < PlayerHand.NumCardsInHand; i++)
        {
            var card = PlayerHand.CardsInHand[i];
            var targetPosition = CalculateCardPosition(i, PlayerHand.NumCardsInHand, dockCenter);
            card.UI.YPositionInHand = targetPosition.y;
            StartCoroutine(AnimateCardToPosition(card.UI?.transform, targetPosition, Quaternion.Euler(90f, 180f, 0f)));
        }
        
        TriggerCardsRemainingChanged();
    }
    
    public void RearrangeStage()
    {
        for (var i = 0; i < StageAreaController.NumCardsStaged; i++)
        {
            StageAreaController.CardsStaged[i].UI.transform.position = _stagePositions[i].position;
        }
        
        TriggerCardsRemainingChanged();
    }

    #endregion


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
        AudioManager.Instance.PlayAmbientAudio();
        
        _mainCamera = Camera.main;

        StageAreaController = _stageArea.GetComponent<StageAreaController>();
        ShellyController = _shelly.GetComponent<ShellyController>();
        
        PlayerHand = new Hand();
        GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);

        HandArea = _handAreas[_levelIndex - 1];
        
        StartCoroutine(DrawInitialHandCoroutine());
        ShellyController.ActivateTextBox(
            "Hi, I'm Shelly! I'll be your helper throughout Fresh Catch. Why don't you go ahead and make your first move?");
    }

    private IEnumerator DrawInitialHandCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        
        StartCoroutine(DrawFullHandCoroutine());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleMouseClick(false);
        }
    }

    private void HandleMouseClick(bool isLeftClick)
    {
        var ray = _mainCamera?.ScreenPointToRay(Input.mousePosition);
        if (ray is null || !Physics.Raycast(ray.Value, out var hit)) return;

        var clickedObject = hit.collider.gameObject;

        if (isLeftClick)
        {
            HandleLeftMouseClick(clickedObject);
        }
        else
        {
            HandleRightMouseClick(clickedObject);
        }
    }

    private void HandleLeftMouseClick(GameObject clickedObject)
    {
        if (clickedObject.CompareTag("DrawButton") && !IsDrawingCards)
        {
            DrawFullHand(false);
        }
        
        if (clickedObject.CompareTag("PlayButton") && !IsDrawingCards)
        {
            OnClickPlayButton();
        }

        if (clickedObject.CompareTag("PeekDeckButton") && !IsDrawingCards)
        {
            OnClickPeekDeckButton();
        }

        if (clickedObject.CompareTag("CardEffectsButton") && !IsDrawingCards)
        {
            OnClickCardEffectsButton();
        }
    }

    #region Dialogue Methods

    public void ShowNormalDialogue(string message)
    {
        if (_isTutorialMode || !enableNormalDialogue || isShowingTutorialDialogue || isShowingNormalDialogue)
        {
            return;
        }

        Debug.Log("[GameManager] Showing normal dialogue now.");
        isShowingNormalDialogue = true;
        ShellyController.ActivateTextBox(message);
        StartCoroutine(WaitForDialogueToFinish(() =>
        {
            Debug.Log("[GameManager] Normal dialogue finished.");
            isShowingNormalDialogue = false;
        }));
    }

    private IEnumerator WaitForDialogueToFinish(Action onFinish)
    {
        Debug.Log("[GameManager] Waiting for dialogue to finish...");
        yield return new WaitForSeconds(3f); 
        Debug.Log("[GameManager] Dialogue wait finished.");
        onFinish?.Invoke();
    }

    #endregion

    #region Card Drawing

    public void DrawFullHand(bool isFromPlay)
    {
        if (DrawsRemaining == 0 || NumCardsOnScreen == HandSize) return;
        
        if (!IsDrawingCards)
        {
            StartCoroutine(DrawFullHandCoroutine());
        }

        if (!isFromPlay)
        {
            DrawsRemaining--;
            TriggerDrawsChanged();
        }
        
        CheckForGameLoss();
    }
    
    public IEnumerator DrawFullHandCoroutine()
    {
        IsDrawingCards = true;

        while (NumCardsOnScreen < HandSize && !GameDeck.IsEmpty)
        {
            var gameCard = GameDeck.DrawCard();
            _initialCardY = HandArea.transform.position.y;
            
            if (PlayerHand.TryAddCardToHand(gameCard))
            {
                gameCard.IsInHand = true;
                gameCard.IsStaged = false;
            }

            var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, HandArea.transform.position);

            gameCard.UI.PlayBubbleEffect();

            yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition));

            gameCard.UI.StopBubbleEffect();

            RearrangeHand(); // Smoothly adjust positions after each card is added
        }

        if (AdditionalCardsDrawn > 0)
        {
            AdditionalCardsDrawn = 0;
            TriggerHandSizeChanged();
        }

        IsDrawingCards = false;
        
        TriggerOnMouseOverForCurrentCard();
        CheckForGameLoss();
    }
    
    private IEnumerator DealCardCoroutine(GameCard gameCard, Vector3 targetPosition)
    {
        var cardUI = gameCard.UI;
        var cardTransform = cardUI.transform;

        // Lock animation
        gameCard.IsAnimating = true;

        cardTransform.position = _deck.transform.position;

        AudioManager.Instance.PlayCardDrawAudio();

        var duration = 0.75f; // Animation duration
        var bounceDuration = 0.25f; // Bounce-back duration
        var elapsedTime = 0f;

        var startPosition = cardTransform.position;
        var overshootPosition = targetPosition + Vector3.up * 1.5f; // Slight overshoot above final position

        // Move to overshoot position
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Smooth step easing
            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            var arcPosition = Vector3.Lerp(startPosition, overshootPosition, t);
            cardTransform.position = arcPosition;

            yield return null;
        }

        // Bounce back to final position
        elapsedTime = 0f; // Reset elapsed time for bounce
        var bounceStartPosition = cardTransform.position;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;

            // Smoothstep easing
            var t = elapsedTime / bounceDuration;
            t = t * t * (3f - 2f * t);

            var bouncePosition = Vector3.Lerp(bounceStartPosition, targetPosition, t);
            cardTransform.position = bouncePosition;

            yield return null;
        }

        cardTransform.position = targetPosition; // Ensure final position

        // Unlock animation
        gameCard.IsAnimating = false;
    }

    #endregion

    #region Button Handlers

    public void OnClickPlayButton()
    {
        if (StageAreaController.NumCardsStaged == 0) return;
        
        switch (StageAreaController.NumCardsStaged)
        {
            case 1:
                if (StageAreaController.GetFirstStagedCard().Data.CardName == "Whaleshark")
                {
                    ScoreSet();
                }
                else
                {
                    if (PlaysRemaining == 0) return;
                    if (StageAreaController.GetFirstStagedCard().Data.CardName == "Kraken")
                    {
                        TriggerCardEffect();
                        return;
                    }
                    TriggerCardEffect();
                    PlaysRemaining--;
                }
                break;
            case 2:
                if (StageAreaController.StageContainsWhaleShark())
                {
                    ScoreSet();
                }
                break;
            case 3:
            case 4:
                ScoreSet();
                break;
            default:
                return;
        }
        
        CheckForGameLoss();
    }

    private void TriggerCardEffect()
    {
        var firstStagedCard = StageAreaController.GetFirstStagedCard();

        firstStagedCard?.ActivateEffect();
    }

    private void ScoreSet()
    {
        CurrentScore += StageAreaController.CalculateScore();
        TriggerScoreChanged();
        StageAreaController.ClearStageArea(true);

        if (CurrentMultiplier > 1)
        {
            CurrentMultiplier = 1;
            TriggerMultiplierChanged();
        }

        AudioManager.Instance.PlayScoreSetAudio();

        if (!CheckForGameWin())
        {
            var message = ShellyController.GetRandomScoreDialog();
            ShellyController.ActivateTextBox(message);
        }
    }
    
    private void OnClickPeekDeckButton()
    {
        UIManager.Instance.ActivatePeekDeckPanel();
    }
    
    private void OnClickCardEffectsButton()
    {
        UIManager.Instance.ActivateCardEffectsPanel();
    }

    #endregion

    #region Mouse Input Handling

    private void HandleRightMouseClick(GameObject clickedObject)
    {
        if (clickedObject.CompareTag("Card") && !IsFlippingCard)
        {
            FlipCard(clickedObject);
        }
    }

    private void FlipCard(GameObject detectionCollider)
    {
        var cardObject = detectionCollider.transform.parent.gameObject;
        StartCoroutine(FlipCardCoroutine(cardObject));
    }

    private IEnumerator FlipCardCoroutine(GameObject card)
    {
        IsFlippingCard = true;
        
        // Play the flip audio
        AudioManager.Instance.PlayCardFlipAudio();

        card.GetComponent<CardUI>().PlayBubbleEffect();

        var startRotation = card.transform.rotation;
        var endRotation = card.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        var duration = 0.5f; // Duration for smoother animation
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep interpolation

            card.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        card.transform.rotation = endRotation; // Ensure it ends exactly at the target rotation

        card.GetComponent<CardUI>().StopBubbleEffect(); // Stop bubbles after flipping

        IsFlippingCard = false;

        if (!card.GetComponent<CardUI>().IsMouseOver)
        {
            card.GetComponent<CardUI>().OnMouseExit();
        }

        TriggerOnMouseOverForCurrentCard();
    }

    private void TriggerOnMouseOverForCurrentCard()
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            var hoveredCardUI = hit.transform.GetComponent<CardUI>();
            hoveredCardUI?.OnMouseEnter();
        }
    }
    
    #endregion

    #region Card Dropping & Discarding

    public bool TryDropCard(Transform dropArea, GameCard gameCard)
    {
        // Destage
        if (dropArea == HandArea.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard, false);
                return true;
            }
        }
        // Stage Card
        if (dropArea == _stageArea.transform)
        {
            if (StageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                return true;
            }
        }
        // Discard
        if (dropArea == _discardArea.transform)
        {
            if (DiscardsRemaining == 0) return false;
            
            if (PlayerHand.TryRemoveCardFromHand(gameCard) || StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                FullDiscard(gameCard, false);
                return true;
            }
        }
    
        return false;
    }

    public void FullDiscard(GameCard gameCard, bool isFromPlay)
    {
        StartCoroutine(SpiralDiscardAnimation(gameCard, isFromPlay));
    }

    private IEnumerator SpiralDiscardAnimation(GameCard gameCard, bool isFromPlay)
    {
        AudioManager.Instance.PlayDiscardAudio();
        gameCard.IsStaged = false;
        gameCard.IsInHand = false;

        if (!isFromPlay)
        {
            DiscardsRemaining--;
            TriggerDiscardsChanged();
        }
        
        var cardTransform = gameCard.UI.transform;

        // Reset rotation to upright
        cardTransform.rotation = Quaternion.Euler(0f, 0f, 0f); // Adjusted to lay flat before animation

        var endPosition = _whirlpoolCenter.position;

        var elapsedTime = 0f;
        var angle = 0f;

        // Capture the initial scale of the card
        var initialScale = cardTransform.localScale;
        var targetScale = Vector3.zero; // Scale down to zero

        while (elapsedTime < _spiralDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate progress
            var t = Mathf.Clamp01(elapsedTime / _spiralDuration);

            // Apply easing (optional for smoother scaling)
            var easedT = t * t * (3f - 2f * t); // Smoothstep interpolation

            // Reduce radius over time
            var radius = Mathf.Lerp(_spiralRadius, 0, easedT);

            // Move downward into the whirlpool
            var depth = Mathf.Lerp(0, -_spiralDepth, easedT);

            // Rotate around the whirlpool center
            angle += _spiralRotationSpeed * Time.deltaTime; // Degrees per second
            var radian = angle * Mathf.Deg2Rad;

            var offset = new Vector3(Mathf.Cos(radian), depth, Mathf.Sin(radian)) * radius;
            var newPosition = endPosition + offset;

            // Update card position
            cardTransform.position = newPosition;

            // Rotate the card around its Y-axis for a spinning effect
            cardTransform.Rotate(Vector3.up, 720 * Time.deltaTime); // 720 degrees per second

            // Scale the card down over time
            cardTransform.localScale = Vector3.Lerp(initialScale, targetScale, easedT);

            yield return null;
        }

        // Ensure the card ends at the center of the whirlpool with zero scale
        cardTransform.position = endPosition;
        cardTransform.localScale = targetScale;

        // Optional: Add a slight delay before destruction to ensure the last frame is visible
        yield return new WaitForSeconds(0.5f);

        Destroy(gameCard.UI.gameObject);
        
        CheckForGameLoss();
    }

    private IEnumerator AnimateCardToPosition(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (cardTransform == null) yield break;

        var startPosition = cardTransform.position;
        var startRotation = cardTransform.rotation;

        var duration = 0.35f; // Animation duration
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (cardTransform == null) yield break;
            
            elapsedTime += Time.deltaTime;

            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cardTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        if (cardTransform == null) yield break;
        cardTransform.position = targetPosition;
        cardTransform.rotation = targetRotation;
    }

    #endregion

    #region Game Over Conditions

    private void CheckForGameLoss()
    {
        if (HasScoreableSet()) return;

        var canDiscard = DiscardsRemaining > 0;
        var canPlay = PlaysRemaining > 0;

        if (!CanDrawCards() && !canDiscard && !canPlay)
        {
            HandleLoss();
        }
    }

    private bool CanDrawCards()
    {
        if (DrawsRemaining == 0 || GameDeck.IsEmpty)
        {
            return false;
        }

        return NumCardsOnScreen < HandSize;
    }
    
    private bool HasScoreableSet()
    {
        var hasWhaleShark = PlayerHand.CardsInHand.Exists(card => card.Data.CardName == "Whaleshark");
        var hasKraken = PlayerHand.CardsInHand.Exists(card => card.Data.CardName == "Kraken");

        if (hasWhaleShark) return true;
        
        var groups = PlayerHand.CardsInHand.GroupBy(card => card.Data.CardName);

        foreach (var group in groups)
        {
            if (group.Count() == 3)
            {
                return true;
            }
            
            if (group.Count() == 2 && group.Key != "Kraken" && hasKraken)
            {
                return true;
            }
        }
        
        return false;
    }

    private void HandleLoss()
    {
        UIManager.Instance.ActivateLossPanel();
    }

    #endregion

    #region Game Win Conditions

    private bool CheckForGameWin()
    {
        if (CurrentScore < CurrentRequiredScore) return false;
        
        HandleWin();
        return true;
    }

    private void HandleWin()
    {
        UIManager.Instance.ActivateWinPanel();
    }

    #endregion

    #region Level & Restart

    public void RestartCurrentLevel()
    {
        ClearHandAndStage();
        ResetStats();
    }
    
    public void HandleLevelChanged()
    {
        InitializeNewLevel();
        ResetStats();

        StartCoroutine(DrawInitialHandCoroutine());
    }

    private void InitializeNewLevel()
    {
        if (_levelIndex < 3)
        {
            _levels[_levelIndex - 1].SetActive(false);
            _levelIndex++;
            _levels[_levelIndex - 1].SetActive(true);
        
            ClearHandAndStage();

            HandArea = null;
            HandArea = _handAreas[_levelIndex - 1];
        }
        else
        {
            SceneManager.LoadScene("Credits");
        }
    }

    private void ClearHandAndStage()
    {
        PlayerHand.ClearHandArea();
        StageAreaController.ClearStageArea(true);
    }

    private void ResetStats()
    {
        CurrentScore = 0;
        TriggerScoreChanged();
        CurrentMultiplier = 1;
        TriggerMultiplierChanged();
        PlaysRemaining = 5;
        DiscardsRemaining = 5;
        TriggerDiscardsChanged();
        DrawsRemaining = 5;
        TriggerDrawsChanged();
        AdditionalCardsDrawn = 0;
        PermanentHandSizeModifier = 0;
        TriggerHandSizeChanged();

        GameDeck = IsTutorialMode 
            ? DeckBuilder.Instance.BuildTutorialDeck(_cardPrefab)
            : DeckBuilder.Instance.BuildNormalLevelDeck(_cardPrefab, GetDeckSizeForLevel(_levelIndex));
        TriggerCardsRemainingChanged();
    }

    #endregion

    #region Event Triggers

    public void TriggerScoreChanged()
    {
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void TriggerDiscardsChanged()
    {
        OnDiscardsChanged?.Invoke(DiscardsRemaining);
    }

    public void TriggerDrawsChanged()
    {
        OnDrawsChanged?.Invoke(DrawsRemaining);
    }

    public void TriggerMultiplierChanged()
    {
        OnMultiplierChanged?.Invoke(CurrentMultiplier);
    }

    public void TriggerHandSizeChanged()
    {
        OnHandSizeChanged?.Invoke(HandSize);
    }

    public void TriggerCardsRemainingChanged()
    {
        if (GameDeck is not null)
        {
            OnCardsRemainingChanged?.Invoke(GameDeck.CardDataInDeck.Count);
        }
    }

    #endregion

    #region Scene Initialization

    /// <summary>
    /// Initializes GameManager for the TutorialScene.
    /// </summary>
    private void InitializeForTutorialScene()
    {
        // Enable tutorial mode and disable normal dialogue
        IsTutorialMode = true;
        EnableNormalDialogue = false;

        // Link scene-specific objects
        LinkSceneSpecificObjects();

        // Setup player hand and deck
        PlayerHand = new Hand();
        if (_handAreas != null && _levelIndex - 1 < _handAreas.Count)
        {
            HandArea = _handAreas[_levelIndex - 1];
        }
        else
        {
            Debug.LogWarning("[GameManager InitializeForTutorialScene] HandAreas not set or insufficient for the current level.");
        }

        // Build a tutorial deck
        GameDeck = DeckBuilder.Instance.BuildTutorialDeck(_cardPrefab);

        // Draw initial hand
        StartCoroutine(DrawInitialHandCoroutine());

        // Initialize the tutorial
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.InitializeTutorial();
        }
        else
        {
            Debug.LogError("[GameManager InitializeForTutorialScene] TutorialManager instance not found.");
        }
    }

    /// <summary>
    /// Initializes GameManager for the L1 Scene (Main Gameplay).
    /// </summary>
    private void InitializeForGameScene()
    {
        // Disable tutorial mode and enable normal dialogue
        IsTutorialMode = false;
        EnableNormalDialogue = true;

        // Link scene-specific objects
        LinkSceneSpecificObjects();

        // Setup player hand and deck
        PlayerHand = new Hand();
        if (_handAreas != null && _levelIndex - 1 < _handAreas.Count)
        {
            HandArea = _handAreas[_levelIndex - 1];
        }
        else
        {
            Debug.LogWarning("[GameManager InitializeForGameScene] HandAreas not set or insufficient for the current level.");
        }

        // Build a deck for normal mode based on level
        int deckSize = GetDeckSizeForLevel(_levelIndex);
        GameDeck = DeckBuilder.Instance.BuildNormalLevelDeck(_cardPrefab, deckSize);

        StartCoroutine(DrawInitialHandCoroutine());

        // Show initial dialogue for the normal game
        ShowNormalDialogue("Welcome to Fresh Catch! Make your first move and show us your skills.");
    }

    /// <summary>
    /// Sets up the tutorial deck with specific cards.
    /// </summary>
    private void SetupTutorialDeck()
    {
        Debug.Log("[GameManager SetupTutorialDeck] Clearing existing deck and hand for tutorial setup.");

        // Clear existing deck and hand
        GameDeck.CardDataInDeck.Clear();
        PlayerHand.ClearHandArea();

        // Add specific tutorial cards
        AddTutorialCard("ClownFish", 4); // Example counts
        AddTutorialCard("Anemone", 2);
        AddTutorialCard("Kraken", 1);

        // Shuffle deck and draw initial hand
        GameDeck.ShuffleDeck();
        Debug.Log("[GameManager SetupTutorialDeck] Deck shuffled. Drawing initial hand.");
        StartCoroutine(DrawFullHandCoroutine());
    }

    /// <summary>
    /// Adds a specific number of cards to the deck.
    /// </summary>
    /// <param name="cardName">Name of the card to add.</param>
    /// <param name="count">Number of cards to add.</param>
    private void AddTutorialCard(string cardName, int count)
    {
        // Ensure CardLibrary is available
        if (CardLibrary.Instance == null)
        {
            Debug.LogError("[GameManager AddTutorialCard] CardLibrary.Instance is null. Cannot retrieve card data.");
            return;
        }

        var cardData = CardLibrary.Instance.GetCardDataByName(cardName);
        if (cardData != null)
        {
            GameDeck.AddCard(cardData, count);
            Debug.Log($"[GameManager AddTutorialCard] Added {count}x {cardName} to the deck.");
        }
        else
        {
            Debug.LogWarning($"[GameManager AddTutorialCard] Card '{cardName}' not found in CardLibrary.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager OnSceneLoaded] Scene '{scene.name}' loaded.");
        if (scene.name == "L1")
        {
            LinkSceneSpecificObjects();
            InitializeForGameScene();
        }
        else if (scene.name == "TutorialScene")
        {
            LinkSceneSpecificObjects();
            InitializeForTutorialScene();
        }
        else
        {
            Debug.Log($"[GameManager OnSceneLoaded] No specific initialization for scene: {scene.name}.");
        }
    }

    #endregion

    #region Additional Methods

    /// <summary>
    /// Links scene-specific objects for the current scene.
    /// </summary>
    private void LinkSceneSpecificObjects()
    {
        _stageArea = GameObject.Find("Stage");
        if (_stageArea != null)
        {
            StageAreaController = _stageArea.GetComponent<StageAreaController>();
        }
        else
        {
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] StageArea not found.");
        }

        _discardArea = GameObject.Find("Discard");
        if (_discardArea != null)
        {
            // Any specific initialization for DiscardArea
        }
        else
        {
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] DiscardArea not found.");
        }

        _deck = GameObject.Find("Deck");
        if (_deck != null)
        {
            // Any specific initialization for Deck
        }
        else
        {
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] Deck not found.");
        }

        _shelly = GameObject.Find("Shelly");
        if (_shelly != null)
        {
            ShellyController = _shelly.GetComponent<ShellyController>();
        }
        else
        {
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] Shelly not found.");
        }

        // Linking WhirlpoolCenter
        var whirlpoolObj = GameObject.Find("WhirlpoolCenter");
        if (whirlpoolObj != null)
        {
            _whirlpoolCenter = whirlpoolObj.transform;
        }
        else
        {
            _whirlpoolCenter = null;
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] WhirlpoolCenter not found.");
        }

        // Linking Levels
        var levelsParent = GameObject.Find("LevelsParent");
        if (levelsParent != null)
        {
            _levels = new List<GameObject>();
            foreach (Transform child in levelsParent.transform)
            {
                _levels.Add(child.gameObject);
            }
        }
        else
        {
            _levels = new List<GameObject>();
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] LevelsParent not found.");
        }

        // Linking HandAreas
        var handAreasParent = GameObject.Find("HandAreasParent");
        if (handAreasParent != null)
        {
            _handAreas = new List<GameObject>();
            foreach (Transform child in handAreasParent.transform)
            {
                _handAreas.Add(child.gameObject);
            }
        }
        else
        {
            _handAreas = new List<GameObject>();
            Debug.LogWarning("[GameManager LinkSceneSpecificObjects] HandAreasParent not found.");
        }

        // Linking Stage Positions
        LinkStagePositions();
    }

    /// <summary>
    /// Dynamically links the stage positions by finding child transforms under StageLocations.
    /// </summary>
    private void LinkStagePositions()
    {
        if (_stageArea == null)
        {
            Debug.LogWarning("[GameManager LinkStagePositions] StageArea is not assigned. Cannot link stage positions.");
            _stagePositions = new List<Transform>();
            return;
        }

        // Find the StageLocations child under StageArea
        Transform stageLocationsTransform = _stageArea.transform.Find("StageLocations");
        if (stageLocationsTransform == null)
        {
            Debug.LogWarning("[GameManager LinkStagePositions] StageLocations object not found under StageArea.");
            _stagePositions = new List<Transform>();
            return;
        }

        // Retrieve all child transforms under StageLocations
        var positions = stageLocationsTransform.GetComponentsInChildren<Transform>()
                            .Where(t => t != stageLocationsTransform) // Exclude the StageLocations itself
                            .ToList();

        if (positions.Count == 0)
        {
            Debug.LogWarning("[GameManager LinkStagePositions] No stage positions found under StageLocations.");
        }
        else
        {
            _stagePositions = positions;
            foreach (var pos in _stagePositions)
            {
                Debug.Log($"[GameManager LinkStagePositions] Found Stage Position: {pos.name}");
            }
            Debug.Log($"[GameManager LinkStagePositions] Linked {_stagePositions.Count} stage positions.");
        }
    }

    /// <summary>
    /// Determines the deck size based on the current level for normal mode.
    /// </summary>
    private int GetDeckSizeForLevel(int level)
    {
        return level switch
        {
            1 => 55, // Level 1 deck size
            2 => 60, // Level 2 deck size
            3 => 70, // Level 3 deck size
            _ => 55, // Default to 55 if unknown level
        };
    }

    #endregion
}
