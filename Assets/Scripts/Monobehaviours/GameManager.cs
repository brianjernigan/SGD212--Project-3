using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Transform _whirlpoolCenter;

    [SerializeField] private List<GameObject> _levels;
    [SerializeField] private List<GameObject> _handAreas;
    public GameObject StageArea => _stageArea;
    public GameObject DiscardArea => _discardArea;
    public GameObject HandArea { get; private set; }

    private int _levelIndex = 1;

    public int NumCardsOnScreen => PlayerHand.NumCardsInHand + StageAreaController.NumCardsStaged;
    private const int MaxCardsOnScreen = 5;
    public int AdditionalCardsDrawn { get; set; }
    public int PermanentHandSizeModifier { get; set; }
    public int HandSize => MaxCardsOnScreen + AdditionalCardsDrawn + PermanentHandSizeModifier;

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

    private readonly float _spiralDuration = 1.0f;
    private readonly float _spiralRadius = 5.0f;
    private readonly float _spiralDepth = 2.0f;
    private readonly float _spiralRotationSpeed = 360f;

    public int PlaysRemaining { get; set; } = 5;
    public int DiscardsRemaining { get; set; } = 5;
    public int DrawsRemaining { get; set; } = 5;

    public event Action<int> OnScoreChanged;
    public event Action<string> OnPlaysChanged;
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

    private const int BaseRequiredScore = 50;
    public int CurrentRequiredScore => BaseRequiredScore * _levelIndex;

    private bool isShowingNormalDialogue = false;
    private bool isShowingTutorialDialogue = false;

    private string normalDialogue = "Hi! I'm Shelly. I'll be your helper throughout Fresh Catch. Why don't you go ahead and make your first move?";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance created.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[GameManager] Duplicate GameManager instance destroyed.");
        }
    }

    private void Start()
    {
        Debug.Log($"[GameManager Start] IsTutorialMode: {_isTutorialMode}");

        _mainCamera = Camera.main;
        StageAreaController = _stageArea.GetComponent<StageAreaController>();
        ShellyController = _shelly.GetComponent<ShellyController>();
        PlayerHand = new Hand();

        if (_isTutorialMode)
        {
            Debug.Log("[GameManager Start] Building tutorial deck.");
            GameDeck = DeckBuilder.Instance.BuildTutorialDeck(_cardPrefab);
        }
        else
        {
            Debug.Log("[GameManager Start] Building default deck.");
            GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);
        }

        HandArea = _handAreas[_levelIndex - 1];

        Debug.Log("[GameManager Start] Starting initial hand draw...");
        StartCoroutine(DrawInitialHandCoroutine());

        if (!_isTutorialMode)
        {
            ShowNormalDialogue(normalDialogue);
        }

        if (_isTutorialMode && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.InitializeTutorial();
        }
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
            Debug.Log("[GameManager] Left mouse button clicked.");
            HandleMouseClick(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("[GameManager] Right mouse button clicked.");
            HandleMouseClick(false);
        }
    }

    private Transform GetKnownAreaTransform(Transform droppedTransform)
    {
        Transform current = droppedTransform;
        while (current != null)
        {
            if (current == HandArea.transform)
            {
                return HandArea.transform;
            }
            if (current == StageArea.transform)
            {
                return StageArea.transform;
            }
            if (current == DiscardArea.transform)
            {
                return DiscardArea.transform;
            }
            current = current.parent;
        }

        return null;
    }

    #region Dialogue Methods

    public void ShowNormalDialogue(string message)
    {
        Debug.Log("[GameManager] Attempting to show normal dialogue: " + message);

        if (_isTutorialMode || !enableNormalDialogue || isShowingTutorialDialogue || isShowingNormalDialogue)
        {
            Debug.Log("[GameManager] Conditions not met for showing normal dialogue.");
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

    #region Card Placement & Arrangement

    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);
        var cardSpacing = Mathf.Min(DockWidth / totalCards, 140f);
        var startX = -((totalCards - 1) * cardSpacing) / 2f;
        var xPosition = startX + (cardIndex * cardSpacing);

        var handCollider = HandArea.GetComponent<BoxCollider>();
        return dockCenter + new Vector3(xPosition, handCollider.transform.TransformPoint(handCollider.center).y + cardIndex, 0f);
    }

    public void PlaceCardInHand(GameCard gameCard)
    {
        var handCenter = HandArea.transform.position;
        var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, handCenter);

        gameCard.UI.transform.position = targetPosition;
        AudioManager.Instance.PlayBackToHandAudio();

        gameCard.IsInHand = true;
        gameCard.IsStaged = false;

        RearrangeHand();
    }

    private void PlaceCardInStage(GameCard gameCard)
    {
        gameCard.UI.transform.position = _stagePositions[StageAreaController.NumCardsStaged - 1].position;
        var pos = gameCard.UI.transform.position;
        pos.y = _initialCardY;
        gameCard.UI.transform.position = pos;

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

            RearrangeHand();
        }

        if (AdditionalCardsDrawn > 0)
        {
            AdditionalCardsDrawn = 0;
            TriggerHandSizeChanged();
        }

        IsDrawingCards = false;

        TriggerOnMouseOverForCurrentCard();
    }

    private IEnumerator DealCardCoroutine(GameCard gameCard, Vector3 targetPosition)
    {
        var cardUI = gameCard.UI;
        var cardTransform = cardUI.transform;

        gameCard.IsAnimating = true;
        cardTransform.position = _deck.transform.position;
        AudioManager.Instance.PlayCardDrawAudio();

        var duration = 0.75f; 
        var bounceDuration = 0.25f; 
        var elapsedTime = 0f;

        var startPosition = cardTransform.position;
        var overshootPosition = targetPosition + Vector3.up * 1.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            var arcPosition = Vector3.Lerp(startPosition, overshootPosition, t);
            cardTransform.position = arcPosition;
            yield return null;
        }

        elapsedTime = 0f;
        var bounceStartPosition = cardTransform.position;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / bounceDuration;
            t = t * t * (3f - 2f * t);

            var bouncePosition = Vector3.Lerp(bounceStartPosition, targetPosition, t);
            cardTransform.position = bouncePosition;
            yield return null;
        }

        cardTransform.position = targetPosition;
        gameCard.IsAnimating = false;
    }

    #endregion

    #region Button Handlers

    public void OnClickPlayButton()
    {
        Debug.Log("[GameManager] OnClickPlayButton called.");

        if (StageAreaController.NumCardsStaged == 0)
        {
            Debug.Log("[GameManager] No cards staged. Exiting Play button handler.");
            return;
        }

        Debug.Log($"[GameManager] Number of cards staged: {StageAreaController.NumCardsStaged}");

        switch (StageAreaController.NumCardsStaged)
        {
            case 1:
                var singleCard = StageAreaController.GetFirstStagedCard();
                Debug.Log($"[GameManager] Single staged card: {singleCard.Data.CardName}");

                if (singleCard.Data.CardName == "Whaleshark")
                {
                    Debug.Log("[GameManager] Single Whaleshark staged. Scoring set.");
                    ScoreSet();
                }
                else if (singleCard.Data.CardName == "Kraken")
                {
                    Debug.Log("[GameManager] Single Kraken staged. No action taken.");
                    return;
                }
                else
                {
                    if (PlaysRemaining == 0)
                    {
                        Debug.Log("[GameManager] No plays remaining. Cannot trigger card effect.");
                        return;
                    }
                    TriggerCardEffect();
                    PlaysRemaining--;
                    TriggerPlaysChanged();
                    Debug.Log($"[GameManager] Plays remaining after decrement: {PlaysRemaining}");
                }
                break;
            case 2:
                var firstCard = StageAreaController.GetFirstStagedCard();
                var secondCard = StageAreaController.CardsStaged.Count > 1 ? StageAreaController.CardsStaged[1] : null;
                if (secondCard == null) return;
                Debug.Log($"[GameManager] Two staged cards: {firstCard.Data.CardName}, {secondCard.Data.CardName}");

                if (firstCard.Data.CardName == "Whaleshark")
                {
                    Debug.Log("[GameManager] First staged card is Whaleshark. Scoring set.");
                    ScoreSet();
                }
                else if (firstCard.Data.CardName == "ClownFish" && secondCard.Data.CardName == "ClownFish")
                {
                    Debug.Log("[GameManager] Two ClownFish staged. Scoring set.");
                    ScoreSet();
                }
                else
                {
                    Debug.Log("[GameManager] Invalid set for two cards. No action taken.");
                }
                break;
            case 3:
            case 4:
                Debug.Log($"[GameManager] {StageAreaController.NumCardsStaged} cards staged. Scoring set.");
                ScoreSet();
                break;
            default:
                Debug.Log("[GameManager] Number of staged cards does not match any case. No action taken.");
                return;
        }

        CheckForGameLoss();
    }

    private void TriggerCardEffect()
    {
        var firstStagedCard = StageAreaController.GetFirstStagedCard();
        Debug.Log($"[GameManager] Triggering effect for card: {firstStagedCard.Data.CardName}");
        firstStagedCard?.ActivateEffect();
    }

    private void ScoreSet()
    {
        Debug.Log("[GameManager] Scoring set.");
        int scoreToAdd = StageAreaController.CalculateScore();
        CurrentScore += scoreToAdd;
        Debug.Log($"[GameManager] Added {scoreToAdd} points. Total score: {CurrentScore}");
        TriggerScoreChanged();
        StageAreaController.ClearStageArea(true);

        if (CurrentMultiplier > 1)
        {
            CurrentMultiplier = 1;
            TriggerMultiplierChanged();
            Debug.Log("[GameManager] Multiplier reset to 1 after scoring.");
        }

        AudioManager.Instance.PlayScoreSetAudio();
        TriggerPlaysChanged();
        Debug.Log("[GameManager] Plays updated after scoring.");
        CheckForGameWin();
    }

    private void OnClickPeekDeckButton()
    {
        Debug.Log("[GameManager] OnClickPeekDeckButton called.");
        UIManager.Instance.ActivatePeekDeckPanel();
    }

    private void OnClickCardEffectsButton()
    {
        Debug.Log("[GameManager] OnClickCardEffectsButton called.");
        UIManager.Instance.ActivateCardEffectsPanel();
    }

    #endregion

    #region Mouse Input Handling

    private void HandleMouseClick(bool isLeftClick)
    {
        var ray = _mainCamera?.ScreenPointToRay(Input.mousePosition);
        if (ray == null || !Physics.Raycast(ray.Value, out var hit)) return;

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
            Debug.Log("[GameManager] DrawButton clicked.");
            DrawFullHand(false);
        }

        if (clickedObject.CompareTag("PlayButton") && !IsDrawingCards)
        {
            Debug.Log("[GameManager] PlayButton clicked.");
            OnClickPlayButton();
        }

        if (clickedObject.CompareTag("PeekDeckButton") && !IsDrawingCards)
        {
            Debug.Log("[GameManager] PeekDeckButton clicked.");
            OnClickPeekDeckButton();
        }

        if (clickedObject.CompareTag("CardEffectsButton") && !IsDrawingCards)
        {
            Debug.Log("[GameManager] CardEffectsButton clicked.");
            OnClickCardEffectsButton();
        }
    }

    private void HandleRightMouseClick(GameObject clickedObject)
    {
        if (clickedObject.CompareTag("Card"))
        {
            Debug.Log("[GameManager] Card right-clicked.");
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
        Debug.Log($"[GameManager] Starting flip coroutine for card: {card.name}");
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
            var t = elapsedTime / duration;
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
        Debug.Log("[GameManager] Attempting to drop card " + gameCard.Data.CardName + " into " + dropArea.name + ".");

        Transform recognizedArea = GetKnownAreaTransform(dropArea);

        if (recognizedArea == HandArea.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard);
                return true;
            }
        }
        else if (recognizedArea == StageArea.transform)
        {
            if (StageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                return true;
            }
        }
        else if (recognizedArea == DiscardArea.transform)
        {
            if (DiscardsRemaining == 0) return false;

            if (PlayerHand.TryRemoveCardFromHand(gameCard) || StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                FullDiscard(gameCard, false);
                return true;
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Attempted to drop card " + gameCard.Data.CardName + " into an unknown area.");
        }

        return false;
    }

    public void FullDiscard(GameCard gameCard, bool isFromPlay)
    {
        Debug.Log($"[GameManager] Performing full discard for card: {gameCard.Data.CardName}, isFromPlay: {isFromPlay}");
        StartCoroutine(SpiralDiscardAnimation(gameCard, isFromPlay));
    }

    private IEnumerator SpiralDiscardAnimation(GameCard gameCard, bool isFromPlay)
    {
        Debug.Log($"[GameManager] Starting spiral discard animation for card: {gameCard.Data.CardName}");
        AudioManager.Instance.PlayDiscardAudio();
        gameCard.IsStaged = false;
        gameCard.IsInHand = false;

        if (!isFromPlay)
        {
            DiscardsRemaining--;
            TriggerDiscardsChanged();
            Debug.Log($"[GameManager] Discards remaining after decrement: {DiscardsRemaining}");
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

        Debug.Log($"[GameManager] Discarded card {gameCard.Data.CardName} successfully.");
        Destroy(gameCard.UI.gameObject);

        CheckForGameLoss();
    }

    private IEnumerator AnimateCardToPosition(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (cardTransform == null) 
        {
            Debug.LogWarning("[GameManager] Attempted to animate a null card transform.");
            yield break;
        }

        var startPosition = cardTransform.position;
        var startRotation = cardTransform.rotation;

        var duration = 0.35f; 
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (cardTransform == null) 
            {
                Debug.LogWarning("[GameManager] Card transform became null during animation.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cardTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        if (cardTransform == null) 
        {
            Debug.LogWarning("[GameManager] Card transform became null at the end of animation.");
            yield break;
        }

        cardTransform.position = targetPosition;
        cardTransform.rotation = targetRotation;
    }

    #endregion

    #region Game Over Conditions

    private void CheckForGameLoss()
    {
        Debug.Log("[GameManager] Checking for game loss conditions.");
        if (HasScoreableSet()) 
        {
            Debug.Log("[GameManager] Scoreable set exists. No game loss.");
            return;
        }

        var canDiscard = DiscardsRemaining > 0;
        var canPlay = PlaysRemaining > 0;

        Debug.Log($"[GameManager] CanDiscard: {canDiscard}, CanPlay: {canPlay}, CanDrawCards: {CanDrawCards()}");

        if (!CanDrawCards() && !canDiscard && !canPlay)
        {
            Debug.Log("[GameManager] No possible moves left. Game lost.");
            HandleLoss();
        }
    }

    private bool CanDrawCards()
    {
        if (DrawsRemaining == 0 || GameDeck.IsEmpty)
        {
            Debug.Log("[GameManager] Cannot draw cards: Either no draws remaining or deck is empty.");
            return false;
        }

        var canDraw = NumCardsOnScreen < HandSize;
        Debug.Log($"[GameManager] CanDrawCards: {canDraw}");
        return canDraw;
    }

    private bool HasScoreableSet()
    {
        var hasWhaleShark = PlayerHand.CardsInHand.Exists(card => card.Data.CardName == "Whaleshark");
        var hasKraken = PlayerHand.CardsInHand.Exists(card => card.Data.CardName == "Kraken");

        if (hasWhaleShark) 
        {
            Debug.Log("[GameManager] Player has a Whaleshark in hand. Scoreable set exists.");
            return true;
        }

        var groups = PlayerHand.CardsInHand.GroupBy(card => card.Data.CardName);
        foreach (var group in groups)
        {
            if (group.Count() == 3)
            {
                Debug.Log($"[GameManager] Player has three of {group.Key}. Scoreable set exists.");
                return true;
            }
            if (group.Count() == 2 && group.Key != "Kraken" && hasKraken)
            {
                Debug.Log($"[GameManager] Player has two of {group.Key} and a Kraken. Scoreable set exists.");
                return true;
            }
        }

        Debug.Log("[GameManager] No scoreable sets found in player's hand.");
        return false;
    }

    private void HandleLoss()
    {
        Debug.Log("[GameManager] Activating Loss Panel.");
        UIManager.Instance.ActivateLossPanel();
    }

    #endregion

    #region Game Win Conditions

    private void CheckForGameWin()
    {
        if (CurrentScore < CurrentRequiredScore) return;

        if (IsTutorialMode)
        {
            Debug.Log("[GameManager] Tutorial complete. Shelly will conclude the tutorial.");
            TutorialManager.Instance.HandleTutorialCompletion();
            return;
        }

        HandleWin();
    }


    private void HandleWin()
    {
        Debug.Log("[GameManager] Activating Win Panel.");
        UIManager.Instance.ActivateWinPanel();
    }

    #endregion

    #region Level & Restart

    public void RestartCurrentLevel()
    {
        Debug.Log("[GameManager] Restarting current level.");
        ClearHandAndStage();
        ResetStats();
    }
    
    public void HandleLevelChanged()
    {
        Debug.Log("[GameManager] Handling level change.");
        InitializeNewLevel();
        ResetStats();
        StartCoroutine(DrawInitialHandCoroutine());
    }

    private void InitializeNewLevel()
    {
        if (_levelIndex < 3)
        {
            Debug.Log($"[GameManager] Deactivating level {_levelIndex}.");
            _levels[_levelIndex - 1].SetActive(false);
            _levelIndex++;
            Debug.Log($"[GameManager] Activating level {_levelIndex}.");
            _levels[_levelIndex - 1].SetActive(true);
        
            ClearHandAndStage();
            HandArea = _handAreas[_levelIndex - 1];
        }
        else
        {
            Debug.Log("[GameManager] All levels completed. Loading Credits scene.");
            SceneManager.LoadScene("Credits");
        }
    }

    private void ClearHandAndStage()
    {
        Debug.Log("[GameManager] Clearing player's hand and stage area.");
        PlayerHand.ClearHandArea();
        StageAreaController.ClearStageArea(true);
    }

    private void ResetStats()
    {
        Debug.Log("[GameManager] Resetting game statistics.");
        CurrentScore = 0;
        TriggerScoreChanged();
        CurrentMultiplier = 1;
        TriggerMultiplierChanged();
        PlaysRemaining = 5;
        TriggerPlaysChanged();
        DiscardsRemaining = 5;
        TriggerDiscardsChanged();
        DrawsRemaining = 5;
        TriggerDrawsChanged();
        AdditionalCardsDrawn = 0;
        PermanentHandSizeModifier = 0;
        TriggerHandSizeChanged();

        if (_isTutorialMode)
        {
            Debug.Log("[GameManager ResetStats] Rebuilding tutorial deck.");
            GameDeck = DeckBuilder.Instance.BuildTutorialDeck(_cardPrefab);
        }
        else
        {
            Debug.Log("[GameManager ResetStats] Rebuilding default deck.");
            GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);
        }

        TriggerCardsRemainingChanged();
    }

    #endregion

    #region Event Triggers

    public void TriggerScoreChanged()
    {
        Debug.Log($"[GameManager] Triggering OnScoreChanged with value: {CurrentScore}");
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void TriggerPlaysChanged()
    {
        Debug.Log($"[GameManager] Triggering OnPlaysChanged with value: {PlaysRemaining}");
        OnPlaysChanged?.Invoke(PlaysRemaining.ToString());
    }

    public void TriggerDiscardsChanged()
    {
        Debug.Log($"[GameManager] Triggering OnDiscardsChanged with value: {DiscardsRemaining}");
        OnDiscardsChanged?.Invoke(DiscardsRemaining);
    }

    public void TriggerDrawsChanged()
    {
        Debug.Log($"[GameManager] Triggering OnDrawsChanged with value: {DrawsRemaining}");
        OnDrawsChanged?.Invoke(DrawsRemaining);
    }

    public void TriggerMultiplierChanged()
    {
        Debug.Log($"[GameManager] Triggering OnMultiplierChanged with value: {CurrentMultiplier}");
        OnMultiplierChanged?.Invoke(CurrentMultiplier);
    }

    public void TriggerHandSizeChanged()
    {
        Debug.Log($"[GameManager] Triggering OnHandSizeChanged with value: {HandSize}");
        OnHandSizeChanged?.Invoke(HandSize);
    }

    public void TriggerCardsRemainingChanged()
    {
        if (GameDeck != null)
        {
            Debug.Log($"[GameManager] Triggering OnCardsRemainingChanged with value: {GameDeck.CardDataInDeck.Count}");
            OnCardsRemainingChanged?.Invoke(GameDeck.CardDataInDeck.Count);
        }
    }

    #endregion
}
