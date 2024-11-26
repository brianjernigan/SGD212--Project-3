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
    
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<Transform> _stagePositions;

    [Header("Areas")]
    [SerializeField] private GameObject _stage;
    [SerializeField] private GameObject _discard;
    [SerializeField] private GameObject _hand;
    [SerializeField] private GameObject _deck;
    [SerializeField] private Transform _whirlpoolCenter; // Added for spiral animation

    [Header("Spiral Parameters")]
    [SerializeField] private float spiralDuration = 2.0f; // Duration of the spiral animation
    [SerializeField] private float spiralRadius = 5.0f;    // Starting radius of the spiral
    [SerializeField] private float spiralDepth = 2.0f;     // Depth the card moves downward
    [SerializeField] private float spiralRotationSpeed = 360f; // Degrees per second

    public GameObject Stage => _stage;
    public GameObject Discard => _discard;
    public GameObject Hand => _hand;
    
    public int CardsOnScreen => PlayerHand.NumCardsInHand + StageAreaController.NumCardsStaged;

    private const int MaxCardsOnScreen = 5;
    public int AdditionalCardsOnScreen { get; set; }
    public int HandSizeModifier { get; set; }

    public int HandSize => MaxCardsOnScreen + AdditionalCardsOnScreen + HandSizeModifier;

    public Deck GameDeck { get; set; }
    public Hand PlayerHand { get; private set; }
    
    public StageAreaController StageAreaController { get; private set; }

    private Camera _mainCamera;

    private bool _isDrawingCards;
    public bool IsDraggingCard { get; set; }
    
    public int CurrentScore { get; set; }
    public int CurrentMultiplier { get; set; } = 1;

    private const float DockWidth = 750f;
    private const float InitialCardY = 25f;

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

    private const int LevelOneRequiredScore = 50;
    private const int LevelTwoRequiredScore = 100;
    private const int LevelThreeRequiredScore = 150;
    
    public bool GameIsLost { get; set; }
    public bool GameIsWon { get; set; }
    
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
        _mainCamera = Camera.main;

        StageAreaController = _stage.GetComponent<StageAreaController>();
        
        PlayerHand = new Hand();
        GameDeck = DeckBuilder.Instance.BuildDefaultDeck(_cardPrefab);
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

        // Visualize the whirlpool center in the Scene view
        Debug.DrawLine(_whirlpoolCenter.position - Vector3.up * 1f, _whirlpoolCenter.position + Vector3.up * 1f, Color.red);
        Debug.DrawLine(_whirlpoolCenter.position - Vector3.right * 1f, _whirlpoolCenter.position + Vector3.right * 1f, Color.red);
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
        if (clickedObject.CompareTag("DrawButton"))
        {
            DrawFullHand();
        }
        else if (clickedObject.CompareTag("PlayButton"))
        {
            OnClickPlayButton();
        }
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
        PlayBubbleEffect(card); // Start bubbles when flipping

        var startRotation = card.transform.rotation;
        var endRotation = card.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        var duration = 0.75f; // Duration for smoother animation
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

        StopBubbleEffect(card); // Stop bubbles after flipping
    }
    
    public void DrawFullHand()
    {
        if (DrawsRemaining == 0) return;
        
        if (!_isDrawingCards)
        {
            StartCoroutine(DrawFullHandCoroutine());
        }
        
        DrawsRemaining--;
        TriggerDrawsChanged();
    }

    private IEnumerator DrawFullHandCoroutine()
    {
        _isDrawingCards = true;

        while (CardsOnScreen < HandSize && !GameDeck.IsEmpty)
        {
            var gameCard = GameDeck.DrawCard();
            if (gameCard != null)
            {
                if (PlayerHand.TryAddCardToHand(gameCard))
                {
                    gameCard.IsInHand = true;
                    gameCard.IsStaged = false;
                }

                var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, _hand.transform.position);

                PlayBubbleEffect(gameCard.UI.gameObject); // Start bubbles when the card is drawn

                yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition));

                StopBubbleEffect(gameCard.UI.gameObject); // Stop bubbles after placing the card

                RearrangeHand(); // Smoothly adjust positions after each card is added
            }
        }

        if (AdditionalCardsOnScreen > 0)
        {
            AdditionalCardsOnScreen = 0;
            TriggerHandSizeChanged();
        }

        _isDrawingCards = false;
    }

    private Vector3 CalculateCardPosition(int cardIndex, int totalCards, Vector3 dockCenter)
    {
        totalCards = Mathf.Max(totalCards, 1);

        var cardSpacing = Mathf.Min(DockWidth / totalCards, 120f); // Dynamic spacing with a max cap
        var startX = -((totalCards - 1) * cardSpacing) / 2f;
        var xPosition = startX + (cardIndex * cardSpacing);

        return dockCenter + new Vector3(xPosition, InitialCardY + cardIndex, 0f); // Straight line with fixed Y
    }

    private IEnumerator DealCardCoroutine(GameCard gameCard, Vector3 targetPosition)
    {
        var cardTransform = gameCard.UI.transform;

        // Lock animation
        gameCard.IsAnimating = true;

        cardTransform.position = _deck.transform.position;

        AudioManager.Instance.PlayCardDrawAudio();

        var duration = 1.0f; // Animation duration
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
        Debug.Log($"Starting spiral discard animation for card: {gameCard.UI.name}");
        Transform cardTransform = gameCard.UI.transform;

        // Reset rotation to upright
        cardTransform.rotation = Quaternion.Euler(0f, 0f, 0f); // Adjusted to lay flat before animation
        Debug.Log($"Card rotation reset to upright for spiral animation.");

        Vector3 endPosition = _whirlpoolCenter.position;

        float elapsedTime = 0f;
        float angle = 0f;

        // Capture the initial scale of the card
        Vector3 initialScale = cardTransform.localScale;
        Vector3 targetScale = Vector3.zero; // Scale down to zero

        while (elapsedTime < spiralDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate progress
            float t = Mathf.Clamp01(elapsedTime / spiralDuration);

            // Apply easing (optional for smoother scaling)
            float easedT = t * t * (3f - 2f * t); // Smoothstep interpolation

            // Reduce radius over time
            float radius = Mathf.Lerp(spiralRadius, 0, easedT);

            // Move downward into the whirlpool
            float depth = Mathf.Lerp(0, -spiralDepth, easedT);

            // Rotate around the whirlpool center
            angle += spiralRotationSpeed * Time.deltaTime; // Degrees per second
            float radian = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(radian), depth, Mathf.Sin(radian)) * radius;
            Vector3 newPosition = endPosition + offset;

            // Update card position
            cardTransform.position = newPosition;

            // Rotate the card around its Y-axis for a spinning effect
            cardTransform.Rotate(Vector3.up, 720 * Time.deltaTime); // 720 degrees per second

            // Scale the card down over time
            cardTransform.localScale = Vector3.Lerp(initialScale, targetScale, easedT);

            // Debug the current position and scale each frame
            Debug.Log($"Spiral Animation - Frame {elapsedTime:F2}s: Position {newPosition}, Scale {cardTransform.localScale}");

            yield return null;
        }

        // Ensure the card ends at the center of the whirlpool with zero scale
        cardTransform.position = endPosition;
        cardTransform.localScale = targetScale;
        Debug.Log($"Spiral discard animation completed for card: {gameCard.UI.name}");

        // Optional: Add a slight delay before destruction to ensure the last frame is visible
        yield return new WaitForSeconds(0.5f);

        DiscardGameCard(gameCard);
    }

    private void DiscardGameCard(GameCard gameCard)
    {
        // Additional logic for adding to discard pile can be added here if needed
        gameCard.IsStaged = false;
        gameCard.IsInHand = false;
        Destroy(gameCard.UI.gameObject);
        DiscardsRemaining--;
        TriggerDiscardsChanged();
        Debug.Log($"Discarding card: {gameCard.UI.name}");
        Debug.Log($"Discards Remaining: {DiscardsRemaining}");
    }

    public void RearrangeHand()
    {
        var dockCenter = _hand.transform.position;
        Debug.Log("Rearranging hand.");

        for (var i = 0; i < PlayerHand.NumCardsInHand; i++)
        {
            var card = PlayerHand.CardsInHand[i];
            var targetPosition = CalculateCardPosition(i, PlayerHand.NumCardsInHand, dockCenter);

            StartCoroutine(AnimateCardToPosition(card.UI?.transform, targetPosition, Quaternion.Euler(90f, 180f, 0f)));
            Debug.Log($"Animating card {card.UI.name} to position {targetPosition}");
        }
    }

    public void RearrangeStage()
    {
        for (var i = 0; i < StageAreaController.NumCardsStaged; i++)
        {
            StageAreaController.CardsStaged[i].UI.transform.position = _stagePositions[i].position;
        }
    }

    public void OnClickPlayButton()
    {
        if (StageAreaController.NumCardsStaged is 0) return;
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
                }
                PlaysRemaining--;
                TriggerPlaysChanged();
                break;
            case 2:
                if (StageAreaController.GetFirstStagedCard().Data.CardName == "Whaleshark")
                {
                    ScoreSet();
                    PlaysRemaining--;
                    TriggerPlaysChanged();
                }
                break;
            case 3:
            case 4:
                ScoreSet();
                PlaysRemaining--;
                TriggerPlaysChanged();
                break;
            default:
                return;
        }
    }

    private void TriggerCardEffect()
    {
        var firstStagedCard = StageAreaController.GetFirstStagedCard();

        firstStagedCard?.ActivateEffect();
    }

    private void ScoreSet()
    {
        var bonusMultiplier = 1;
        
        if (StageAreaController.NumCardsStaged == 4)
        {
            bonusMultiplier = 2;
        }

        CurrentScore += StageAreaController.CalculateScore() * bonusMultiplier;
        TriggerScoreChanged();
        StageAreaController.ClearStageArea();

        if (CurrentMultiplier > 1)
        {
            CurrentMultiplier = 1;
            TriggerMultiplierChanged();
        }
    }

    public void PlaceCardInHand(GameCard gameCard)
    {
        var dockCenter = _hand.transform.position;

        var targetPosition = CalculateCardPosition(PlayerHand.NumCardsInHand - 1, PlayerHand.NumCardsInHand, dockCenter);

        gameCard.UI.transform.position = targetPosition;
     
        gameCard.IsInHand = true;
        gameCard.IsStaged = false;
        
        RearrangeHand();
    }

    private void PlaceCardInStage(GameCard gameCard)
    {
        gameCard.UI.transform.position = _stagePositions[StageAreaController.NumCardsStaged - 1].transform.position;
        
        gameCard.IsStaged = true;
        gameCard.IsInHand = false;
    }

    private IEnumerator AnimateCardToPosition(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (cardTransform == null) yield break; // Exit if the card is no longer valid

        var startPosition = cardTransform.position;
        var startRotation = cardTransform.rotation;

        var duration = 0.5f; // Animation duration
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            var t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cardTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        cardTransform.position = targetPosition;
        cardTransform.rotation = targetRotation;
        Debug.Log($"Finished animating card to position {targetPosition}");
    }

    private void CheckForGameLoss()
    {
        var outOfPlays = PlaysRemaining == 0;
        var outOfDiscards = DiscardsRemaining == 0;
        var outOfDraws = DrawsRemaining == 0;

        if (!outOfPlays || !outOfDiscards || !outOfDraws) return;
        GameIsLost = true;
        GameIsWon = false;
        Debug.Log("Game is lost.");
    }

    private void CheckForGameWin(int levelNumber)
    {
        var requiredScore = levelNumber switch
        {
            1 => LevelOneRequiredScore,
            2 => LevelTwoRequiredScore,
            3 => LevelThreeRequiredScore,
            _ => 0
        };

        if (CurrentScore < requiredScore) return;
        GameIsWon = true;
        GameIsLost = false;
        Debug.Log("Game is won!");
    }

    public void TriggerScoreChanged()
    {
        OnScoreChanged?.Invoke(CurrentScore);
        Debug.Log($"Score Changed Event Triggered. New Score: {CurrentScore}");
    }

    public void TriggerPlaysChanged()
    {
        OnPlaysChanged?.Invoke(PlaysRemaining);
        Debug.Log($"Plays Changed Event Triggered. Plays Remaining: {PlaysRemaining}");
    }

    public void TriggerDiscardsChanged()
    {
        OnDiscardsChanged?.Invoke(DiscardsRemaining);
        Debug.Log($"Discards Changed Event Triggered. Discards Remaining: {DiscardsRemaining}");
    }

    public void TriggerDrawsChanged()
    {
        OnDrawsChanged?.Invoke(DrawsRemaining);
        Debug.Log($"Draws Changed Event Triggered. Draws Remaining: {DrawsRemaining}");
    }

    public void TriggerMultiplierChanged()
    {
        OnMultiplierChanged?.Invoke(CurrentMultiplier);
        Debug.Log($"Multiplier Changed Event Triggered. Current Multiplier: {CurrentMultiplier}");
    }

    public void TriggerHandSizeChanged()
    {
        OnHandSizeChanged?.Invoke(HandSize);
        Debug.Log($"Hand Size Changed Event Triggered. New Hand Size: {HandSize}");
    }

    public void TriggerMoneyChanged()
    {
        OnMoneyChanged?.Invoke(PlayerMoney);
        Debug.Log($"Money Changed Event Triggered. Player Money: {PlayerMoney}");
    }

    public void TriggerCardsRemainingChanged()
    {
        if (GameDeck is not null)
        {
            OnCardsRemainingChanged?.Invoke(GameDeck.CardDataInDeck.Count);
            Debug.Log($"Cards Remaining Changed Event Triggered. Cards Remaining: {GameDeck.CardDataInDeck.Count}");
        }
    }

    // **Added Methods for Bubble Particle Effects**

    /// <summary>
    /// Plays the bubble particle effect attached to the specified card.
    /// </summary>
    /// <param name="card">The GameObject representing the card.</param>
    private void PlayBubbleEffect(GameObject card)
    {
        var bubbleEffect = card.transform.Find("BubbleEffect")?.GetComponent<ParticleSystem>();
        if (bubbleEffect != null)
        {
            bubbleEffect.Play();
            Debug.Log($"Bubble effect started for card: {card.name}");
        }
        else
        {
            Debug.LogWarning($"BubbleEffect not found on card: {card.name}");
        }
    }

    /// <summary>
    /// Stops the bubble particle effect attached to the specified card.
    /// </summary>
    /// <param name="card">The GameObject representing the card.</param>
    private void StopBubbleEffect(GameObject card)
    {
        var bubbleEffect = card.transform.Find("BubbleEffect")?.GetComponent<ParticleSystem>();
        if (bubbleEffect != null)
        {
            bubbleEffect.Stop();
            Debug.Log($"Bubble effect stopped for card: {card.name}");
        }
        else
        {
            Debug.LogWarning($"BubbleEffect not found on card: {card.name}");
        }
    }


}
