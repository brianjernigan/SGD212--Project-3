using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private float _dialogueWaitTime = 3f; // Time in seconds to wait between lines
    private bool isShowingTutorialDialogue = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[TutorialManager Awake] Instance created.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.IsTutorialMode = true;
                GameManager.Instance.EnableNormalDialogue = false; // Disable normal dialogues
                IsTutorialMode = true;
                Debug.Log("[TutorialManager Awake] Set GameManager.IsTutorialMode to true and disabled normal dialogues.");
            }
            else
            {
                Debug.LogError("[TutorialManager Awake] GameManager.Instance is null. Cannot set tutorial flags.");
            }
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[TutorialManager Awake] Duplicate instance detected and destroyed.");
        }
    }

    public void InitializeTutorial()
    {
        Debug.Log("[TutorialManager] Initializing tutorial...");
        if (!GameManager.Instance.IsTutorialMode)
        {
            Debug.Log("[TutorialManager] Tutorial mode not active. Exiting initialization.");
            return;
        }

        Debug.Log("[TutorialManager] Tutorial mode is active. Setting up deck...");
        SetupTutorialDeck();
        StartCoroutine(BeginTutorialSequence());
        SubscribeToGameEvents();
    }

    private void SetupTutorialDeck()
    {
        Debug.Log("[TutorialManager] Clearing existing deck and hand for tutorial setup.");
        GameManager.Instance.GameDeck.CardDataInDeck.Clear();
        GameManager.Instance.PlayerHand.ClearHandArea();

        AddTutorialCard("ClownFish", ClownfishCount);
        AddTutorialCard("Anemone", AnemoneCount);
        AddTutorialCard("Kraken", KrakenCount);

        GameManager.Instance.GameDeck.ShuffleDeck();
        Debug.Log("[TutorialManager] Deck shuffled. Drawing initial hand...");
        GameManager.Instance.StartCoroutine(GameManager.Instance.DrawFullHandCoroutine());
    }

    private void AddTutorialCard(string cardName, int count)
    {
        var cardData = CardLibrary.Instance.GetCardDataByName(cardName);
        if (cardData != null)
        {
            GameManager.Instance.GameDeck.AddCard(cardData, count);
            Debug.Log($"[TutorialManager] Added {count} {cardName} cards to the deck.");
        }
        else
        {
            Debug.LogWarning($"[TutorialManager] Card '{cardName}' not found in CardLibrary.");
        }
    }

    private void SubscribeToGameEvents()
    {
        Debug.Log("[TutorialManager] Subscribing to GameManager events.");
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged += HandleMultiplierChanged;
    }

    private void UnsubscribeFromGameEvents()
    {
        Debug.Log("[TutorialManager] Unsubscribing from GameManager events.");
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnMultiplierChanged -= HandleMultiplierChanged;
        }
    }

    private IEnumerator BeginTutorialSequence()
    {
        Debug.Log("[TutorialManager] Beginning tutorial sequence...");
        yield return new WaitForSeconds(1f);
        ShowIntroDialogue();
    }

    private void ShowIntroDialogue()
    {
        Debug.Log("[TutorialManager] Showing introduction dialogue.");
        StartCoroutine(ShowDialogueLines(new string[]
        {
            "Hi, I'm Shelly! Welcome to the Fresh Catch tutorial!",
            "We'll start simple. Your goal: Earn 50 points.",
            "You have special cards: Clownfish and Anemone. Clownfish score points. Anemone boosts your multiplier!",
            "First, drag both Clownfish cards from your hand to the stage area to form a set, then press 'Play' to score."
        }, OnIntroDialogueComplete));
    }

    private void OnIntroDialogueComplete()
    {
        Debug.Log("[TutorialManager] Intro dialogue complete. Moving to ExplainCards step.");
        _currentStep = TutorialStep.ExplainCards;
    }

    private void HandleScoreChanged(int newScore)
    {
        Debug.Log($"[TutorialManager] Score changed to {newScore}. Current Step: {_currentStep}");
        if (!IsTutorialMode) return;

        if (_currentStep == TutorialStep.ExplainCards && newScore > 0)
        {
            Debug.Log("[TutorialManager] Player scored during ExplainCards step. Showing multiplier explanation.");
            StartCoroutine(ShowDialogueLines(new string[]
            {
                "Great job! You scored points from the Clownfish set.",
                "Now let's talk about the multiplier. Anemones add extra multipliers to future sets!",
                "Drag an Anemone from your hand to the stage area. Then play another Clownfish set to see the multiplier!",
                "Try to reach 50 points!"
            }, OnMultiplierExplanationComplete));
        }
        else if (_currentStep == TutorialStep.WaitForMultiplierPlay && newScore >= 50)
        {
            Debug.Log("[TutorialManager] Score reached 50 during WaitForMultiplierPlay step. Showing conclusion.");
            _currentStep = TutorialStep.Conclusion;
            ShowConclusion();
        }
    }

    private void HandleMultiplierChanged(int newMultiplier)
    {
        Debug.Log($"[TutorialManager] Multiplier changed to {newMultiplier}. Current Step: {_currentStep}");
        if (!IsTutorialMode) return;

        if (_currentStep == TutorialStep.ExplainMultiplier && newMultiplier > 1)
        {
            Debug.Log("[TutorialManager] Multiplier activated. Showing multiplier usage instructions.");
            StartCoroutine(ShowDialogueLines(new string[]
            {
                "Awesome! You've activated the multiplier.",
                "Play another set to see how it boosts your score!"
            }, OnMultiplierUsageComplete));
        }
    }

    private void OnMultiplierExplanationComplete()
    {
        Debug.Log("[TutorialManager] Multiplier explanation dialogue complete.");
        _currentStep = TutorialStep.ExplainMultiplier;
    }

    private void OnMultiplierUsageComplete()
    {
        Debug.Log("[TutorialManager] Multiplier usage dialogue complete.");
        _currentStep = TutorialStep.WaitForMultiplierPlay;
    }

    private void ShowConclusion()
    {
        if (!_tutorialComplete)
        {
            Debug.Log("[TutorialManager] Showing conclusion dialogue.");
            _tutorialComplete = true;
            StartCoroutine(ShowDialogueLines(new string[]
            {
                "Fantastic! You reached 50 points!",
                "You now understand how to play cards, form sets, and use the Anemone to boost multipliers!",
                "Let's return to the main menu and start the real game."
            }, OnConclusionComplete));
        }
    }

    private void OnConclusionComplete()
    {
        Debug.Log("[TutorialManager] Conclusion dialogue complete. Returning to main menu...");
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
        Debug.Log("[TutorialManager] OnDestroy called. Unsubscribing from events.");
        UnsubscribeFromGameEvents();
    }

    private IEnumerator ShowDialogueLines(string[] lines, Action onComplete = null)
    {
        var shelly = GameManager.Instance.ShellyController;
        Debug.Log($"[TutorialManager] Showing {lines.Length} dialogue lines.");
        foreach (var line in lines)
        {
            Debug.Log($"[TutorialManager] Showing line: {line}");
            shelly.ActivateTextBox(line);
            yield return new WaitForSeconds(_dialogueWaitTime);
        }
        Debug.Log("[TutorialManager] All lines shown.");
        onComplete?.Invoke();
    }
}
