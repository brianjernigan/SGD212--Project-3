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
    
    private readonly float _spiralDuration = 1.0f; // Duration of the spiral animation
    private readonly float _spiralRadius = 5.0f;    // Starting radius of the spiral
    private readonly float _spiralDepth = 2.0f;     // Depth the card moves downward
    private readonly float _spiralRotationSpeed = 360f; // Degrees per second

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
            "Hi! I'm Shelly. I'll be your helper throughout Fresh Catch. Why don't you go ahead and make your first move?");
    }

    private IEnumerator DrawInitialHandCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        /* For Testing */
        // var testCards = new[] { "Whaleshark", "Whaleshark", "Kraken", "Kraken", "CookieCutter" };
        // foreach (var card in testCards)
        // {
        //     var specificCard = CardLibrary.Instance.GetCardDataByName(card);
        //     if (specificCard is not null)
        //     {
        //         var cardToDraw = GameDeck.DrawSpecificCard(specificCard);
        //         PlayerHand.TryAddCardToHand(cardToDraw);
        //         var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand,
        //             _hand.transform.position);
        //
        //         StartCoroutine(DealCardCoroutine(cardToDraw, targetPosition));
        //     }
        // }
        
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
    
    private IEnumerator DrawFullHandCoroutine()
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
                else if (StageAreaController.GetFirstStagedCard().Data.CardName == "Kraken")
                {
                    return;
                }
                else
                {
                    if (PlaysRemaining == 0) return;
                    TriggerCardEffect();
                    PlaysRemaining--;
                    TriggerPlaysChanged();
                }
                break;
            case 2:
                if (StageAreaController.GetFirstStagedCard().Data.CardName == "Whaleshark")
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
        TriggerPlaysChanged();
        CheckForGameWin();
    }
    
    private void OnClickPeekDeckButton()
    {
        UIManager.Instance.ActivatePeekDeckPanel();
    }
    
    private void OnClickCardEffectsButton()
    {
        UIManager.Instance.ActivateCardEffectsPanel();
    }
    
    private void HandleRightMouseClick(GameObject clickedObject)
    {
        if (clickedObject.CompareTag("Card"))
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
    
    public bool TryDropCard(Transform dropArea, GameCard gameCard)
    {
        // Destage
        if (dropArea == HandArea.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard);
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

    private void CheckForGameWin()
    {
        if (CurrentScore < CurrentRequiredScore) return;
        
        HandleWin();
    }

    private void HandleWin()
    {
        UIManager.Instance.ActivateWinPanel();
    }

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
        TriggerPlaysChanged();
        DiscardsRemaining = 5;
        TriggerDiscardsChanged();
        DrawsRemaining = 5;
        TriggerDrawsChanged();
        AdditionalCardsDrawn = 0;
        PermanentHandSizeModifier = 0;
        TriggerHandSizeChanged();

        GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);
        TriggerCardsRemainingChanged();
    }

    #region Invocations

    public void TriggerScoreChanged()
    {
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void TriggerPlaysChanged()
    {
        OnPlaysChanged?.Invoke(PlaysRemaining.ToString());
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
}
