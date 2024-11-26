using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Stores all possible cards for creating decks and starting the game
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<Transform> _stagePositions;

    [Header("Areas")]
    [SerializeField] private GameObject _stage;
    [SerializeField] private GameObject _discard;
    [SerializeField] private GameObject _deck;
    [SerializeField] private Transform _whirlpoolCenter; // Added for spiral animation

    // Dynamic Lookup
    private GameObject _hand;
    
    [Header("Play Areas")] 
    [SerializeField] private List<GameObject> _playAreas;
    
    private float _spiralDuration = 1.0f; // Duration of the spiral animation
    private float _spiralRadius = 5.0f;    // Starting radius of the spiral
    private float _spiralDepth = 2.0f;     // Depth the card moves downward
    private float _spiralRotationSpeed = 360f; // Degrees per second

    public GameObject Stage => _stage;
    public GameObject Discard => _discard;
    public GameObject Hand => _hand;
    
    public int NumCardsOnScreen => PlayerHand.NumCardsInHand + StageAreaController.NumCardsStaged;

    private const int MaxCardsOnScreen = 6;
    public int AdditionalCardsDrawn { get; set; }
    public int PermanentHandSizeModifier { get; set; }

    public int HandSize => MaxCardsOnScreen + AdditionalCardsDrawn + PermanentHandSizeModifier;

    public Deck GameDeck { get; set; }
    public Hand PlayerHand { get; private set; }
    
    public StageAreaController StageAreaController { get; private set; }

    private Camera _mainCamera;

    public bool IsDrawingCards { get; set; }
    public bool IsDraggingCard { get; set; }
    public bool IsFlippingCard { get; set; }
    
    public int CurrentScore { get; set; }
    public int CurrentMultiplier { get; set; } = 1;

    private const float DockWidth = 750f;
    private float _initialCardY;

    public int PlaysRemaining { get; set; } = 5;
    public int DiscardsRemaining { get; set; } = 5;
    public int DrawsRemaining { get; set; } = 5;
    public int PlayerMoney { get; set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnPlaysChanged;
    public event Action<int> OnDiscardsChanged;
    public event Action<int> OnDrawsChanged;
    public event Action<int> OnMultiplierChanged;
    public event Action<int> OnHandSizeChanged;
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnCardsRemainingChanged;
    public event Action OnLevelChanged;

    private int _levelIndex = 1;

    // public int LevelIndex
    // {
    //     get => _levelIndex;
    //     set
    //     {
    //         _levelIndex = value;
    //         HandleLevelChanged();
    //     }
    // }

    private const int BaseRequiredScore = 50;
    public int CurrentRequiredScore => BaseRequiredScore * _levelIndex;
    
    public bool RoundIsWon { get; set; }
    public bool RoundIsLost { get; set; }

    #region Helpers

    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);

        var cardSpacing = Mathf.Min(DockWidth / totalCards, 140f); // Dynamic spacing with a max cap
        var startX = -((totalCards - 1) * cardSpacing) / 2f;
        var xPosition = startX + (cardIndex * cardSpacing);

        var handCollider = _hand.GetComponent<BoxCollider>();
        
        return dockCenter + new Vector3(xPosition, handCollider.transform.TransformPoint(handCollider.center).y + cardIndex, 0f); // Straight line with fixed Y
    }

    public void PlaceCardInHand(GameCard gameCard)
    {
        var handCenter = _hand.transform.position;

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
        var dockCenter = _hand.transform.position;
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

        _hand = GameObject.FindGameObjectWithTag("HandArea");
    }

    private void Start()
    {
        _mainCamera = Camera.main;

        StageAreaController = _stage.GetComponent<StageAreaController>();
        
        PlayerHand = new Hand();
        GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);
        
        StartCoroutine(DrawInitialHandCoroutine());
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

    private IEnumerator DrawInitialHandCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        /* For Testing */
        
        var testCards = new[] { "Whaleshark", "Whaleshark", "Kraken", "Kraken", "CookieCutter" };
        foreach (var card in testCards)
        {
            var specificCard = CardLibrary.Instance.GetCardDataByName(card);
            if (specificCard is not null)
            {
                var cardToDraw = GameDeck.DrawSpecificCard(specificCard);
                PlayerHand.TryAddCardToHand(cardToDraw);
                var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand,
                    _hand.transform.position);
        
                StartCoroutine(DealCardCoroutine(cardToDraw, targetPosition));
            }
        }
        
        // StartCoroutine(DrawFullHandCoroutine());
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
            DrawFullHand();
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

    public void DrawFullHand()
    {
        if (DrawsRemaining == 0 || NumCardsOnScreen == HandSize) return;
        
        if (!IsDrawingCards)
        {
            StartCoroutine(DrawFullHandCoroutine());
        }
        
        DrawsRemaining--;
        TriggerDrawsChanged();
        CheckForGameLoss();
    }
    
    private IEnumerator DrawFullHandCoroutine()
    {
        IsDrawingCards = true;

        while (NumCardsOnScreen < HandSize && !GameDeck.IsEmpty)
        {
            var gameCard = GameDeck.DrawCard();
            _initialCardY = _hand.transform.position.y;
            
            if (PlayerHand.TryAddCardToHand(gameCard))
            {
                gameCard.IsInHand = true;
                gameCard.IsStaged = false;
            }

            var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, _hand.transform.position);

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
        
        // StartWaveEffect();
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
        if (PlaysRemaining == 0) return;
        
        switch (StageAreaController.NumCardsStaged)
        {
            case 1:
                if (StageAreaController.GetFirstStagedCard().Data.CardName == "Whaleshark")
                {
                    ScoreSet();
                }
                else
                {
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
        StageAreaController.ClearStageArea();

        if (CurrentMultiplier > 1)
        {
            CurrentMultiplier = 1;
            TriggerMultiplierChanged();
        }

        AudioManager.Instance.PlayScoreSetAudio();
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
        if (dropArea == _hand.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard);
                return true;
            }
        }
        // Stage Card
        if (dropArea == _stage.transform)
        {
            if (StageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                return true;
            }
        }
        // Discard
        if (dropArea == _discard.transform)
        {
            if (DiscardsRemaining == 0) return false;
            
            if (PlayerHand.TryRemoveCardFromHand(gameCard) || StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                StartCoroutine(SpiralDiscardAnimation(gameCard)); // Start spiral animation instead of immediate discard
                return true;
            }
        }
    
        return false;
    }

    private IEnumerator SpiralDiscardAnimation(GameCard gameCard)
    {
        AudioManager.Instance.PlayDiscardAudio();
        gameCard.IsStaged = false;
        gameCard.IsInHand = false;
        DiscardsRemaining--;
        TriggerDiscardsChanged();
        
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
        var isOutOfPlays = PlaysRemaining == 0;
        var isOutOfDiscards = DiscardsRemaining == 0;
        var isOutOfDraws = DrawsRemaining == 0 || GameDeck.IsEmpty;
        var cardsAreMaxed = NumCardsOnScreen >= HandSize;
        var noScoreableSet = !HasScoreableSet();

        if (isOutOfPlays && isOutOfDiscards && isOutOfDraws && (cardsAreMaxed || noScoreableSet))
        {
            HandleLoss();
        }
    }

    private void HandleLoss()
    {
        RoundIsLost = true;
        RoundIsWon = false;
        Debug.Log("you lost");
        UIManager.Instance.ActivateLossPanel();
    }

    private void CheckForGameWin()
    {
        if (CurrentScore < CurrentRequiredScore) return;
        
        HandleWin();
    }

    private void HandleWin()
    {
        RoundIsWon = true;
        RoundIsLost = false;
        Debug.Log("you win");
        UIManager.Instance.ActivateWinPanel();
    }
    
    public void HandleLevelChanged()
    {
        _playAreas[_levelIndex - 1].SetActive(false);
        _levelIndex++;
        _playAreas[_levelIndex - 1].SetActive(true);
        PlayerHand.ClearHandArea();
        StageAreaController.ClearStageArea();

        _hand = null;
        _hand = GameObject.FindGameObjectWithTag("HandArea");

        CurrentScore = 0;
        TriggerScoreChanged();
    }

    #region Invocations

    public void TriggerScoreChanged()
    {
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void TriggerPlaysChanged()
    {
        OnPlaysChanged?.Invoke(PlaysRemaining);
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

    public void TriggerMoneyChanged()
    {
        OnMoneyChanged?.Invoke(PlayerMoney);
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
