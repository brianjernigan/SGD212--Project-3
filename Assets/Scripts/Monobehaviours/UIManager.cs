using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Texts")] 
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _playsText;
    [SerializeField] private TMP_Text _discardsText;
    [SerializeField] private TMP_Text _drawsText;
    [SerializeField] private TMP_Text _multiplierText;
    [SerializeField] private TMP_Text _handSizeText;
    [SerializeField] private TMP_Text _cardsRemainingText;

    [Header("Panels")] 
    [SerializeField] private GameObject _peekDeckPanel;
    [SerializeField] private GameObject _cardEffectsPanel;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _lossPanel;

    [SerializeField] private List<TMP_Text> _cardCountTexts;
    [SerializeField] private List<TMP_Text> _cardEffectsTexts;

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

    private void OnEnable()
    {
        GameManager.Instance.OnScoreChanged += UpdateScoreText;
        GameManager.Instance.OnPlaysChanged += UpdatePlaysText;
        GameManager.Instance.OnDiscardsChanged += UpdateDiscardsText;
        GameManager.Instance.OnDrawsChanged += UpdateDrawsText;
        GameManager.Instance.OnMultiplierChanged += UpdateMultiplierText;
        GameManager.Instance.OnHandSizeChanged += UpdateHandSizeText;
        GameManager.Instance.OnCardsRemainingChanged += UpdateCardsRemainingText;
        GameManager.Instance.OnCardsRemainingChanged += UpdateCardCountsText;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScoreText;
        GameManager.Instance.OnPlaysChanged -= UpdatePlaysText;
        GameManager.Instance.OnDiscardsChanged -= UpdateDiscardsText;
        GameManager.Instance.OnDrawsChanged -= UpdateDrawsText;
        GameManager.Instance.OnMultiplierChanged -= UpdateMultiplierText;
        GameManager.Instance.OnHandSizeChanged -= UpdateHandSizeText;
        GameManager.Instance.OnCardsRemainingChanged -= UpdateCardsRemainingText;
        GameManager.Instance.OnCardsRemainingChanged -= UpdateCardCountsText;
    }

    private void Start()
    {
        UpdateScoreText(GameManager.Instance.CurrentScore);
        UpdatePlaysText(GameManager.Instance.PlaysRemaining.ToString());
        UpdateDiscardsText(GameManager.Instance.DiscardsRemaining);
        UpdateDrawsText(GameManager.Instance.DrawsRemaining);
        UpdateMultiplierText(GameManager.Instance.CurrentMultiplier);
        UpdateHandSizeText(GameManager.Instance.HandSize);
        UpdateCardsRemainingText(GameManager.Instance.GameDeck.CardDataInDeck.Count);

        SetCardEffectTexts();
    }

    private void SetCardEffectTexts()
    {
        for (var i = 0; i < CardLibrary.Instance.AllPossibleCards.Count; i++)
        {
            var cardName = CardLibrary.Instance.AllPossibleCards[i].CardName;
            var description = CardLibrary.Instance.GetCardEffectByName(cardName).EffectDescription;
            _cardEffectsTexts[i].text = $"{cardName}: {description}";
        }
    }

    private void UpdateScoreText(int score)
    {
        // USE "I" AS A REPLACEMENT FOR "/"
        _scoreText.text = $"Score: {score} / {GameManager.Instance.CurrentRequiredScore}";
    }

    public void UpdatePlaysText(string plays)
    {
        _playsText.text = plays;
    }

    private void UpdateDiscardsText(int discards)
    {
        _discardsText.text = $"Discards: {discards}";
    }
    
    private void UpdateDrawsText(int draws)
    {
        _drawsText.text = $"{draws}";
    }

    private void UpdateMultiplierText(int multiplier)
    {
        _multiplierText.text = $"Multiplier: {multiplier}x";
    }

    private void UpdateHandSizeText(int size)
    {
        _handSizeText.text = $"Hand Size: {size}";
    }

    private void UpdateCardsRemainingText(int cardsRemaining)
    {
        _cardsRemainingText.text = $"{cardsRemaining}";
    }
    
    private void UpdateCardCountsText(int obj)
    {
        var gameDeck = GameManager.Instance.GameDeck;
        var playerHand = GameManager.Instance.PlayerHand;

        var cardCountsInHand = new int[_cardCountTexts.Count];
        
        foreach (var card in playerHand.CardsInHand)
        {
            var index = CardLibrary.Instance.AllPossibleCards.IndexOf(card.Data);
            if (index >= 0 && index < cardCountsInHand.Length)
            {
                cardCountsInHand[index]++;
            }
        }

        var cardCountsInDeck = new int[_cardCountTexts.Count];

        foreach (var card in gameDeck.CardDataInDeck)
        {
            var index = CardLibrary.Instance.AllPossibleCards.IndexOf(card);
            if (index >= 0 && index < cardCountsInDeck.Length)
            {
                cardCountsInDeck[index]++;
            }
        }

        for (var i = 0; i < _cardCountTexts.Count; i++)
        {
            _cardCountTexts[i].text =
                $"{CardLibrary.Instance.AllPossibleCards[i].CardName}: {cardCountsInDeck[i].ToString()} ({cardCountsInHand[i].ToString()});";
        }
    }

    public void ActivatePeekDeckPanel()
    {
        _peekDeckPanel.SetActive(true);
        _winPanel.SetActive(false);
        _lossPanel.SetActive(false);
        _cardEffectsPanel.SetActive(false);
    }

    public void ActivateCardEffectsPanel()
    {
        _cardEffectsPanel.SetActive(true);
        _winPanel.SetActive(false);
        _lossPanel.SetActive(false);
        _peekDeckPanel.SetActive(false);
    }

    public void ActivateWinPanel()
    {
        _winPanel.SetActive(true);
        _peekDeckPanel.SetActive(false);
        _lossPanel.SetActive(false);
        _cardEffectsPanel.SetActive(false);
    }

    public void ActivateLossPanel()
    {
        _lossPanel.SetActive(true);
        _peekDeckPanel.SetActive(false);
        _winPanel.SetActive(false);
        _cardEffectsPanel.SetActive(false);
    }

    public void OnClickPeekBackButton()
    {
        _peekDeckPanel.SetActive(false);
    }

    public void OnClickEffectsBackButton()
    {
        _cardEffectsPanel.SetActive(false);
    }

    public void OnClickLossRestartButton()
    {
        _lossPanel.SetActive(false);
        GameManager.Instance.RestartCurrentLevel();
    }

    public void OnClickWinNextLevelButton()
    {
        GameManager.Instance.HandleLevelChanged();
        _winPanel.SetActive(false);
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
