using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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
    [SerializeField] private Transform _whirlpoolCenter;
    
    private float _spiralDuration = 1.0f;
    private float _spiralRadius = 5.0f;
    private float _spiralDepth = 2.0f;
    private float _spiralRotationSpeed = 360f;

    public GameObject Stage => _stage;
    public GameObject Discard => _discard;
    public GameObject Hand => _hand;

    public bool IsInTutorial { get; set; } = true;

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

    public int LevelOneScore => LevelOneRequiredScore;
    public int LevelTwoScore => LevelTwoRequiredScore;
    public int LevelThreeScore => LevelThreeRequiredScore;

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
        // During tutorial automation, we do not rely on user input.
        // If you wanted to allow manual input after tutorial, you can remove this check or condition it differently.
        if (IsInTutorial) return;

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
        if (clickedObject.CompareTag("Card"))
        {
            SelectCard(clickedObject);
        }
        else if (clickedObject.CompareTag("DrawButton"))
        {
            DrawFullHand();
        }
        else if (clickedObject.CompareTag("PlayButton"))
        {
            OnClickPlayButton();
        }
    }

    private void SelectCard(GameObject card)
    {
        AudioManager.Instance.PlayCardSelectAudio();
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

                var targetPosition = CalculateCardPosition(
                    PlayerHand.NumCardsInHand - 1,
                    PlayerHand.NumCardsInHand,
                    _hand.transform.position
                );

                gameCard.UI.PlayBubbleEffect();
                yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition));
                gameCard.UI.StopBubbleEffect();

                RearrangeHand();
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
        var cardSpacing = Mathf.Min(DockWidth / totalCards, 140f);
        var startX = -((totalCards - 1) * cardSpacing) / 2f;
        var xPosition = startX + (cardIndex * cardSpacing);

        return dockCenter + new Vector3(xPosition, InitialCardY + cardIndex, 0f);
    }

    private IEnumerator DealCardCoroutine(GameCard gameCard, Vector3 targetPosition)
    {
        var cardTransform = gameCard.UI.transform;
        gameCard.IsAnimating = true;

        cardTransform.position = _deck.transform.position;
        AudioManager.Instance.PlayCardDrawAudio();

        var duration = 1.0f;
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

    public bool TryDropCard(Transform dropArea, GameCard gameCard)
    {
        if (dropArea == _hand.transform)
        {
            if (PlayerHand.TryAddCardToHand(gameCard) && StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                PlaceCardInHand(gameCard);
                return true;
            }
        }

        if (dropArea == _stage.transform)
        {
            if (StageAreaController.TryAddCardToStage(gameCard) && PlayerHand.TryRemoveCardFromHand(gameCard))
            {
                PlaceCardInStage(gameCard);
                return true;
            }
        }

        if (dropArea == _discard.transform)
        {
            if (DiscardsRemaining == 0) return false;
            if (PlayerHand.TryRemoveCardFromHand(gameCard) || StageAreaController.TryRemoveCardFromStage(gameCard))
            {
                StartCoroutine(SpiralDiscardAnimation(gameCard));
                return true;
            }
        }
    
        return false;
    }

    private IEnumerator SpiralDiscardAnimation(GameCard gameCard)
    {
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

        DiscardGameCard(gameCard);
    }

    private void DiscardGameCard(GameCard gameCard)
    {
        if (gameCard == null) return;

        gameCard.IsStaged = false;
        gameCard.IsInHand = false;

        if (gameCard.UI != null)
        {
            Destroy(gameCard.UI.gameObject);
        }

        DiscardsRemaining--;
        TriggerDiscardsChanged();
    }

    public void RearrangeHand()
    {
        var dockCenter = _hand.transform.position;
        for (var i = 0; i < PlayerHand.NumCardsInHand; i++)
        {
            var card = PlayerHand.CardsInHand[i];
            var targetPosition = CalculateCardPosition(i, PlayerHand.NumCardsInHand, dockCenter);
            StartCoroutine(AnimateCardToPosition(card.UI?.transform, targetPosition, Quaternion.Euler(90f, 180f, 0f)));
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
        // After effect activates, sets might be cleared or modified. No scoring directly unless WhaleShark or sets formed.
    }

    private void ScoreSet()
    {
        var bonusMultiplier = 1;
        
        if (StageAreaController.NumCardsStaged == 4)
        {
            bonusMultiplier = 2;
        }

        CurrentScore += StageAreaController.CalculateScore() * bonusMultiplier * CurrentMultiplier;
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
        if (cardTransform == null) yield break;

        var startPosition = cardTransform.position;
        var startRotation = cardTransform.rotation;

        var duration = 0.5f;
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

        if (cardTransform != null)
        {
            cardTransform.position = targetPosition;
            cardTransform.rotation = targetRotation;
        }
    }

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

    public void DrawSpecificHand(List<string> cardNames)
    {
        StartCoroutine(DrawSpecificHandCoroutine(cardNames));
    }

    private IEnumerator DrawSpecificHandCoroutine(List<string> cardNames)
    {
        foreach (string cardName in cardNames)
        {
            CardData cardData = CardLibrary.Instance.GetCardDataByName(cardName);
            if (cardData != null)
            {
                GameCard gameCard = GameDeck.DrawSpecificCard(cardData);
                if (gameCard != null)
                {
                    if (PlayerHand.TryAddCardToHand(gameCard))
                    {
                        gameCard.IsInHand = true;
                        gameCard.IsStaged = false;
                    }

                    var targetPosition = CalculateCardPosition(
                        PlayerHand.NumCardsInHand - 1,
                        PlayerHand.NumCardsInHand,
                        _hand.transform.position
                    );

                    gameCard.UI.PlayBubbleEffect();
                    yield return StartCoroutine(DealCardCoroutine(gameCard, targetPosition));
                    gameCard.UI.StopBubbleEffect();
                }
            }
        }

        RearrangeHand();
    }
}
