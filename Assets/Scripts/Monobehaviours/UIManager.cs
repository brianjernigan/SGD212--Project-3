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
    [SerializeField] private TMP_Text _multiplierText;
    [SerializeField] private TMP_Text _handSizeText;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _cardsRemainingText;

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
        GameManager.Instance.OnMultiplierChanged += UpdateMultiplierText;
        GameManager.Instance.OnHandSizeChanged += UpdateHandSizeText;
        GameManager.Instance.OnMoneyChanged += UpdateMoneyText;
        GameManager.Instance.OnCardsRemainingChanged += UpdateCardsRemainingText;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScoreText;
        GameManager.Instance.OnPlaysChanged -= UpdatePlaysText;
        GameManager.Instance.OnDiscardsChanged -= UpdateDiscardsText;
        GameManager.Instance.OnMultiplierChanged -= UpdateMultiplierText;
        GameManager.Instance.OnHandSizeChanged -= UpdateHandSizeText;
        GameManager.Instance.OnMoneyChanged -= UpdateMoneyText;
        GameManager.Instance.OnCardsRemainingChanged -= UpdateCardsRemainingText;
    }

    private void Start()
    {
        var gameDeck = GameManager.Instance.GameDeck;
        var gameDeckSize = gameDeck.CardDataInDeck.Count;
        
        UpdateScoreText(GameManager.Instance.CurrentScore);
        UpdatePlaysText(GameManager.Instance.PlaysRemaining);
        UpdateDiscardsText(GameManager.Instance.DiscardsRemaining);
        UpdateMultiplierText(GameManager.Instance.CurrentMultiplier);
        UpdateHandSizeText(GameManager.Instance.HandSize);
        UpdateMoneyText(GameManager.Instance.PlayerMoney);
        UpdateCardsRemainingText(GameManager.Instance.GameDeck is not null
            ? GameManager.Instance.GameDeck.CardDataInDeck.Count
            : 52);
    }

    public void UpdateScoreText(int score)
    {
        _scoreText.text = $"Score: {score}";
    }

    public void UpdatePlaysText(int plays)
    {
        _playsText.text = $"Plays:\n{plays}";
    }

    public void UpdateDiscardsText(int discards)
    {
        _discardsText.text = $"Discards:\n{discards}";
    }

    public void UpdateMultiplierText(int multiplier)
    {
        _multiplierText.text = $"Multiplier: {multiplier}x";
    }

    public void UpdateHandSizeText(int size)
    {
        _handSizeText.text = $"Hand Size: {size}";
    }

    public void UpdateMoneyText(int cash)
    {
        _moneyText.text = $"${cash}";
    }

    public void UpdateCardsRemainingText(int cardsRemaining)
    {
        _cardsRemainingText.text = $"Cards Remaining: {cardsRemaining}";
    }
}
