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
    [SerializeField] private GameObject _cardUIPrefab;
    
    [Header("Drop Areas")]
    [SerializeField] private RectTransform _handArea;
    [SerializeField] private RectTransform _stageArea;
    [SerializeField] private RectTransform _discardArea;

    [Header("Canvas")]
    [SerializeField] private Canvas _gameCanvas;

    [Header("Texts")] 
    [SerializeField] private TMP_Text _scoreText;
    
    public RectTransform HandArea => _handArea;
    public RectTransform StageArea => _stageArea;
    public RectTransform DiscardArea => _discardArea;

    public int CardsOnScreen => _playerHand.NumCardsInHand + _stageAreaController.NumCardsStaged;
    public int MaxCardsOnScreen { get; set; } = 5;
    
    public Canvas GameCanvas => _gameCanvas;

    private Deck _gameDeck;
    private Hand _playerHand;

    private Dictionary<CardData, int> _defaultDeckComposition;
    private StageAreaController _stageAreaController;
    
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

    private void SetDeckCompositions()
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
            { _allPossibleCards[9], 4 }
        };
        
        // Define other decks? 
    }

    private void Start()
    {
        SetDeckCompositions();
        
        _gameDeck = new Deck(_defaultDeckComposition, _cardUIPrefab);
        _playerHand = new Hand();

        _stageAreaController = _stageArea.gameObject.GetComponent<StageAreaController>();
    }

    public void DrawFullHand()
    {
        while (CardsOnScreen < MaxCardsOnScreen)
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {
        var gameCard = _gameDeck.DrawCard();
        if (gameCard is not null)
        {
            _playerHand.TryAddCardToHand(gameCard);
        }
    }

    public bool OnCardDropped(RectTransform dropArea, GameCard gameCard)
    {
        // Destage
        if (dropArea == HandArea)
        {
            if (_playerHand.TryAddCardToHand(gameCard) && _stageAreaController.TryRemoveCardFromStageArea(gameCard))
            {
                return true;
            }
        }
        
        // Discard
        else if (dropArea == DiscardArea)
        {
            if (!_playerHand.TryRemoveCardFromHand(gameCard) &&
                !_stageAreaController.TryRemoveCardFromStageArea(gameCard)) return false;
            Destroy(gameCard.UI.gameObject);
            return true;

        }
        // Stage Card
        else if (dropArea == StageArea)
        {
            if (!_stageAreaController.TryAddCardToStageArea(gameCard)) return false;
            _playerHand.TryRemoveCardFromHand(gameCard);
            return true;
        }

        return false;
    }

    public void OnClickPlayButton()
    {
        var action = _stageAreaController.CheckStagedCards();

        switch (action)
        {
            case 2:
                return;
            case 1:
                TriggerCardEffect();
                break;
            case 3:
                ScoreSet();
                break;
        }
    }

    private void TriggerCardEffect()
    {
        Debug.Log("Triggering");
        _stageAreaController.ClearStagedCards();
    }

    private void ScoreSet()
    {
        var score = _stageAreaController.Score;
        UpdateScoreText(score);
        _stageAreaController.ClearStagedCards();
    }

    private void UpdateScoreText(int score)
    {
        _scoreText.text = $"Score: {score}";
    }
}
