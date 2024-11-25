using System.Collections;
using System.Collections.Generic;
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

    public void UpdateScoreText()
    {
        _scoreText.text = $"Score: {GameManager.Instance.CurrentScore}";
    }

    public void UpdatePlaysText()
    {
        _playsText.text = $"Plays:\n{GameManager.Instance.PlaysRemaining}";
    }

    public void UpdateDiscardsText()
    {
        _discardsText.text = $"Discards:\n{GameManager.Instance.DiscardsRemaining}";
    }

    public void UpdateMultiplierText()
    {
        _multiplierText.text = $"Multiplier: {GameManager.Instance.CurrentMultiplier}x";
    }

    public void UpdateHandSizeText()
    {
        _handSizeText.text = $"Hand Size: {GameManager.Instance.HandSize}";
    }

    public void UpdateMoneyText()
    {
        _moneyText.text = $"${GameManager.Instance.PlayerMoney}";
    }
}
