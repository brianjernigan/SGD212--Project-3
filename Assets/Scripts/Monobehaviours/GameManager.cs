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
        float fixedY = handCollider.transform.TransformPoint(handCollider.center).y; // Fixed Y position

        return dockCenter + new Vector3(xPosition, fixedY, 0f);
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
        if (StageAreaController.NumCardsStaged - 1 < 0 || StageAreaController.NumCardsStaged - 1 >= _stagePositions.Count)
        {
            Debug.LogWarning("[GameManager PlaceCardInStage] Invalid stage position index.");
            return;
        }

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

            // Only animate if the card is not already being animated
            if (!card.IsAnimating)
            {
                StartCoroutine(AnimateCardToPosition(card.UI?.transform, targetPosition, Quaternion.Euler(90f, 180f, 0f)));
            }
        }

        TriggerCardsRemainingChanged();
    }


    public void RearrangeStage()
    {
        for (var i = 0; i < StageAreaController.NumCardsStaged; i++)
        {
            if (i >= _stagePositions.Count)
            {
                Debug.LogWarning($"[GameManager RearrangeStage] Stage position index {i} out of bounds.");
                continue;
            }

            StageAreaController.CardsStaged[i].UI.transform.position = _stagePositions[i].position;
        }

        TriggerCardsRemainingChanged();
    }

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager Awake] Instance created and marked as DontDestroyOnLoad.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[GameManager Awake] Duplicate instance detected and destroyed.");
            return;
        }

        PlayerHand = new Hand();
        GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);

        if (GameDeck == null)
            Debug.LogError("[GameManager Awake] GameDeck is null after BuildDefaultDeck.");
        else
            Debug.Log("[GameManager Awake] GameDeck initialized successfully.");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }



    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        AudioManager.Instance.PlayAmbientAudio();
        _mainCamera = Camera.main;

        if (_stageArea != null)
            StageAreaController = _stageArea.GetComponent<StageAreaController>();
        if (_shelly != null)
            ShellyController = _shelly.GetComponent<ShellyController>();

        if (_handAreas != null && _levelIndex - 1 < _handAreas.Count)
        {
            HandArea = _handAreas[_levelIndex - 1];
        }

        if (!_isTutorialMode)
        {
            ShellyController?.ActivateTextBox(
                "Hi, I'm Shelly! I'll be your helper throughout Fresh Catch. Why don't you go ahead and make your first move?");
        }
    }

    private IEnumerator DrawInitialHandCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DrawFullHandCoroutine(false)); // Starting without a play
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

        // Optional: Hotkeys for testing (F1/F2/F3)
        HandleSceneSwitchingHotkeys();
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
        
        isShowingNormalDialogue = true;
        ShellyController.ActivateTextBox(message);
        StartCoroutine(WaitForDialogueToFinish(() =>
        {
            isShowingNormalDialogue = false;
        }));
    }

    private IEnumerator WaitForDialogueToFinish(Action onFinish)
    {
        yield return new WaitForSeconds(3f); 
        onFinish?.Invoke();
    }

    #endregion

    #region Card Drawing

    public void DrawFullHand(bool isFromPlay)
    {
        if (DrawsRemaining == 0 || NumCardsOnScreen == HandSize) return;

        if (!IsDrawingCards)
        {
            StartCoroutine(DrawFullHandCoroutine(isFromPlay));
        }

        if (!isFromPlay)
        {
            DrawsRemaining--;
            TriggerDrawsChanged();
        }

        CheckForGameLoss();
    }
    
    public IEnumerator DrawFullHandCoroutine(bool isFromPlay)
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

            RearrangeHand();
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

        gameCard.IsAnimating = true;

        // Start from the deck's position
        cardTransform.position = _deck.transform.position;

        AudioManager.Instance.PlayCardDrawAudio();

        var duration = 0.75f;
        var bounceDuration = 0.25f;
        var elapsedTime = 0f;

        var startPosition = cardTransform.position;
        var overshootPosition = targetPosition + Vector3.up * 1.5f;

        // Move to overshoot position
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            t = t * t * (3f - 2f * t); // Smoothstep interpolation

            var arcPosition = Vector3.Lerp(startPosition, overshootPosition, t);
            cardTransform.position = arcPosition;

            yield return null;
        }

        // Bounce back to final position
        elapsedTime = 0f;
        var bounceStartPosition = cardTransform.position;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / bounceDuration);
            t = t * t * (3f - 2f * t); // Smoothstep interpolation

            var bouncePosition = Vector3.Lerp(bounceStartPosition, targetPosition, t);
            cardTransform.position = bouncePosition;

            yield return null;
        }

        // Ensure the card ends exactly at the target position
        cardTransform.position = targetPosition;
        cardTransform.rotation = Quaternion.Euler(90f, 180f, 0f); // Set to desired rotation

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
            string message = string.Empty;

            if (IsTutorialMode)
            {
                message = ShellyController.GetRandomScoreDialog();
            }
            else
            {
                message = "Great job! Keep it up!"; // Or any other non-tutorial-specific message
            }

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
        AudioManager.Instance.PlayCardFlipAudio();
        card.GetComponent<CardUI>().PlayBubbleEffect();

        var startRotation = card.transform.rotation;
        var endRotation = card.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        var duration = 0.5f;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            card.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        card.transform.rotation = endRotation; 
        card.GetComponent<CardUI>().StopBubbleEffect();
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
        if (dropArea == HandArea.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard, false);
                return true;
            }
        }
        if (dropArea == _stageArea.transform)
        {
            if (StageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                return true;
            }
        }
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

        cardTransform.rotation = Quaternion.Euler(0f, 0f, 0f);

        var endPosition = _whirlpoolCenter.position;
        var elapsedTime = 0f;
        var angle = 0f;

        var initialScale = cardTransform.localScale;
        var targetScale = Vector3.zero;

        while (elapsedTime < _spiralDuration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / _spiralDuration);
            var easedT = t * t * (3f - 2f * t);

            var radius = Mathf.Lerp(_spiralRadius, 0, easedT);
            var depth = Mathf.Lerp(0, -_spiralDepth, easedT);

            angle += _spiralRotationSpeed * Time.deltaTime;
            var radian = angle * Mathf.Deg2Rad;

            var offset = new Vector3(Mathf.Cos(radian), depth, Mathf.Sin(radian)) * radius;
            var newPosition = endPosition + offset;

            cardTransform.position = newPosition;
            cardTransform.Rotate(Vector3.up, 720 * Time.deltaTime);
            cardTransform.localScale = Vector3.Lerp(initialScale, targetScale, easedT);

            yield return null;
        }

        cardTransform.position = endPosition;
        cardTransform.localScale = targetScale;
        yield return new WaitForSeconds(0.5f);

        Destroy(gameCard.UI.gameObject);
        CheckForGameLoss();
    }

    private IEnumerator AnimateCardToPosition(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (cardTransform == null) yield break;

        var startPosition = cardTransform.position;
        var startRotation = cardTransform.rotation;

        var duration = 0.35f;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (cardTransform == null) yield break;

            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

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
        if (IsTutorialMode)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.HandleTutorialCompletion();
            }
        }
        else
        {
            UIManager.Instance.ActivateWinPanel();
        }
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
            : DeckBuilder.Instance.BuildNormalLevelDeck(_cardPrefab, 55);

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
        if (GameDeck != null)
        {
            OnCardsRemainingChanged?.Invoke(GameDeck.CardDataInDeck.Count);
        }
    }

    #endregion

    #region Scene Initialization

    private void InitializeForTutorialScene()
    {
        IsTutorialMode = true;
        EnableNormalDialogue = false;

        LinkSceneSpecificObjects();

        PlayerHand = new Hand();
        if (_handAreas != null && _levelIndex - 1 < _handAreas.Count)
        {
            HandArea = _handAreas[_levelIndex - 1];
        }

        GameDeck = DeckBuilder.Instance.BuildTutorialDeck(_cardPrefab);
        if (GameDeck == null) return;

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.InitializeTutorial();
        }
    }


    private void InitializeForGameScene()
    {
        IsTutorialMode = false;
        EnableNormalDialogue = true;

        LinkSceneSpecificObjects();

        PlayerHand = new Hand();

        if (_handAreas != null && _levelIndex - 1 < _handAreas.Count)
        {
            HandArea = _handAreas[_levelIndex - 1];
        }
        
        GameDeck = DeckBuilder.Instance.BuildNormalLevelDeck(_cardPrefab, 55);

        if (GameDeck == null) return;

        StartCoroutine(DrawInitialHandCoroutine());
        ShowNormalDialogue("Welcome to Fresh Catch! Make your first move and show us your skills.");
    }
    
    public void StartDrawFullHandCoroutine(bool isFromPlay)
    {
        if (GameDeck == null) return;
        StartCoroutine(DrawFullHandCoroutine(isFromPlay));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            InitializeForGameScene();
        }
        else if (scene.name == "TutorialScene")
        {
            InitializeForTutorialScene();
        }
    }


    public void DisableTutorialManager()
    {
        if (TutorialManager.Instance != null)
        {
            // Unsubscribe from events in TutorialManager
            TutorialManager.Instance.UnsubscribeFromGameEvents();
        }

        IsTutorialMode = false;
        EnableNormalDialogue = true;

        // Destroy TutorialManager if it exists
        if (TutorialManager.Instance != null)
        {
            Destroy(TutorialManager.Instance.gameObject);
        }
    }

    #endregion

    #region Additional Methods

    private void LinkSceneSpecificObjects()
    {
        _stageArea = GameObject.Find("Stage");
        if (_stageArea != null)
        {
            StageAreaController = _stageArea.GetComponent<StageAreaController>();
        }

        _discardArea = GameObject.Find("Discard");

        _deck = GameObject.Find("Deck");

        _shelly = GameObject.Find("Shelly");
        if (_shelly != null)
        {
            ShellyController = _shelly.GetComponent<ShellyController>();
        }

        // Linking WhirlpoolCenter
        var whirlpoolObj = GameObject.Find("WhirlpoolCenter");
        _whirlpoolCenter = whirlpoolObj != null ? whirlpoolObj.transform : null;

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
        }

        LinkStagePositions();
    }

    private void LinkStagePositions()
    {
        if (_stageArea == null)
        {
            _stagePositions = new List<Transform>();
            return;
        }

        Transform stageLocationsTransform = _stageArea.transform.Find("StageLocations");
        if (stageLocationsTransform == null)
        {
            _stagePositions = new List<Transform>();
            return;
        }

        var positions = stageLocationsTransform.GetComponentsInChildren<Transform>()
                        .Where(t => t != stageLocationsTransform)
                        .ToList();
        

        _stagePositions = positions;
    }

    private void TransitionToScene(string sceneName)
    {
        // Reset tutorial states if needed
        if (sceneName != "TutorialScene" && IsTutorialMode)
        {
            DisableTutorialManager();
        }

        SceneManager.LoadScene(sceneName);
    }

        private void HandleSceneSwitchingHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TransitionToScene("TutorialScene");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TransitionToScene("GameScene");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TransitionToScene("Credits");
        }
    }


    #endregion
}
