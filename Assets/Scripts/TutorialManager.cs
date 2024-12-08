// TutorialManager.cs (New)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    private enum TutorialStep
    {
        Intro,
        ExplainCards,
        WaitForPlayerToPlaySet,
        ExplainMultiplier,
        WaitForMultiplierPlay,
        Conclusion
    }

    private TutorialStep _currentStep = TutorialStep.Intro;
    private bool _tutorialComplete = false;
    public bool IsTutorialMode { get; set; } = false;


    [Header("Tutorial Deck Setup")]
    public int ClownfishCount = 2;
    public int AnemoneCount = 2;
    public int KrakenCount = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeTutorial()
    {
        if (!GameManager.Instance.IsTutorialMode)
        {
            return;
        }

        SetupTutorialDeck();
        StartCoroutine(BeginTutorialSequence());
        SubscribeToGameEvents();
    }

    private void SetupTutorialDeck()
    {
        GameManager.Instance.GameDeck.CardDataInDeck.Clear();
        GameManager.Instance.PlayerHand.ClearHandArea();

        var clownfishData = CardLibrary.Instance.GetCardDataByName("ClownFish");
        var anemoneData = CardLibrary.Instance.GetCardDataByName("Anemone");
        var krakenData = CardLibrary.Instance.GetCardDataByName("Kraken");

        if (clownfishData != null)
        {
            GameManager.Instance.GameDeck.AddCard(clownfishData, ClownfishCount);
        }
        if (anemoneData != null)
        {
            GameManager.Instance.GameDeck.AddCard(anemoneData, AnemoneCount);
        }
        if (krakenData != null)
        {
            GameManager.Instance.GameDeck.AddCard(krakenData, KrakenCount);
        }

        GameManager.Instance.GameDeck.ShuffleDeck();

        GameManager.Instance.StartCoroutine(GameManager.Instance.DrawFullHandCoroutine());
    }

    private void SubscribeToGameEvents()
    {
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged += HandleMultiplierChanged;
    }

    private void UnsubscribeFromGameEvents()
    {
        GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged -= HandleMultiplierChanged;
    }

    private IEnumerator BeginTutorialSequence()
    {
        yield return new WaitForSeconds(1f);
        ShowIntroDialogue();
    }

    private void ShowIntroDialogue()
    {
        DialogueManager.Instance.StartDialogue(new string[]
        {
            "Hi, I'm Shelly! Welcome to the Fresh Catch tutorial!",
            "We'll start simple. Your goal: Earn 50 points.",
            "You have special cards: Clownfish and Anemone. Clownfish score points. Anemone boosts your multiplier!",
            "First, drag both Clownfish cards from your hand to the stage area to form a set, then press 'Play' to score."
        }, OnIntroDialogueComplete);
    }

    private void OnIntroDialogueComplete()
    {
        _currentStep = TutorialStep.ExplainCards;
    }

    private void HandleScoreChanged(int newScore)
    {
        if (_currentStep == TutorialStep.ExplainCards && newScore > 0)
        {
            DialogueManager.Instance.StartDialogue(new string[]
            {
                "Great job! You scored points from the Clownfish set.",
                "Now let's talk about the multiplier. Anemones add extra multipliers to future sets!",
                "Drag an Anemone from your hand to the stage area. Then play another Clownfish set to see the multiplier!",
                "Try to reach 50 points!"
            }, OnMultiplierExplanationComplete);
        }
        else if (_currentStep == TutorialStep.WaitForMultiplierPlay && newScore >= 50)
        {
            _currentStep = TutorialStep.Conclusion;
            ShowConclusion();
        }
    }

    private void HandleMultiplierChanged(int newMultiplier)
    {
        if (_currentStep == TutorialStep.ExplainMultiplier && newMultiplier > 1)
        {
            DialogueManager.Instance.StartDialogue(new string[]
            {
                "Awesome! You've activated the multiplier.",
                "Play another Clownfish set to see how it boosts your score!"
            }, OnMultiplierUsageComplete);
        }
    }

    private void OnMultiplierExplanationComplete()
    {
        _currentStep = TutorialStep.ExplainMultiplier;
    }

    private void OnMultiplierUsageComplete()
    {
        _currentStep = TutorialStep.WaitForMultiplierPlay;
    }

    private void ShowConclusion()
    {
        if (!_tutorialComplete)
        {
            _tutorialComplete = true;
            DialogueManager.Instance.StartDialogue(new string[]
            {
                "Fantastic! You reached 50 points!",
                "You now understand how to play cards, form sets, and use the Anemone to boost multipliers!",
                "Let's return to the main menu and start the real game."
            }, OnConclusionComplete);
        }
    }

    private void OnConclusionComplete()
    {
        StartCoroutine(ReturnToMenuCoroutine());
    }

    private IEnumerator ReturnToMenuCoroutine()
    {
        yield return new WaitForSeconds(2f);
        UnsubscribeFromGameEvents();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameEvents();
    }
}
