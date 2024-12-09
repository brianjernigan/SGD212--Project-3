using System;
using System.Collections;
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
    public bool IsTutorialMode { get; private set; } = false;

    [Header("Tutorial Deck Setup")]
    [SerializeField] private int ClownfishCount = 4; // Sufficient for reaching 50 points with multipliers
    [SerializeField] private int AnemoneCount = 2;
    [SerializeField] private int KrakenCount = 1;

    [Header("Dialogue Settings")]
    [SerializeField] private float DialogueWaitTime = 3f; // Time in seconds to wait between lines
    private bool _isDialogueInProgress = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[TutorialManager Awake] Instance created and marked as DontDestroyOnLoad.");

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a new scene is loaded.
    /// Reinitializes references if necessary.
    /// </summary>
    /// <param name="scene">The scene that was loaded.</param>
    /// <param name="mode">The mode in which the scene was loaded.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[TutorialManager OnSceneLoaded] Scene '{scene.name}' loaded.");
        if (scene.name == "GameScene") // Replace with your actual game scene name
        {
            // Re-link any scene-specific references if necessary
            // For example, if TutorialManager needs to interact with objects in the new scene
            Debug.Log("[TutorialManager OnSceneLoaded] Initializing for GameScene.");
            // Add any initialization code here if needed
        }
    }

    /// <summary>
    /// Initializes the tutorial by setting up the deck and starting the sequence.
    /// </summary>
    public void InitializeTutorial()
    {
        Debug.Log("[TutorialManager InitializeTutorial] Initializing tutorial.");
        if (!GameManager.Instance.IsTutorialMode)
        {
            Debug.Log("[TutorialManager InitializeTutorial] Tutorial mode not active. Exiting initialization.");
            return;
        }

        SetupTutorialDeck();
        StartCoroutine(BeginTutorialSequence());
        SubscribeToGameEvents();
    }

    /// <summary>
    /// Sets up the tutorial deck with specific cards.
    /// </summary>
    private void SetupTutorialDeck()
    {
        Debug.Log("[TutorialManager SetupTutorialDeck] Clearing existing deck and hand for tutorial setup.");
        GameManager.Instance.GameDeck.CardDataInDeck.Clear();
        GameManager.Instance.PlayerHand.ClearHandArea();

        AddTutorialCard("ClownFish", ClownfishCount);
        AddTutorialCard("Anemone", AnemoneCount);
        AddTutorialCard("Kraken", KrakenCount);

        GameManager.Instance.GameDeck.ShuffleDeck();
        Debug.Log("[TutorialManager SetupTutorialDeck] Deck shuffled. Drawing initial hand.");
        GameManager.Instance.StartCoroutine(GameManager.Instance.DrawFullHandCoroutine());
    }

    /// <summary>
    /// Adds a specific number of cards to the deck.
    /// </summary>
    /// <param name="cardName">Name of the card to add.</param>
    /// <param name="count">Number of cards to add.</param>
    private void AddTutorialCard(string cardName, int count)
    {
        var cardData = CardLibrary.Instance.GetCardDataByName(cardName);
        if (cardData != null)
        {
            GameManager.Instance.GameDeck.AddCard(cardData, count);
            Debug.Log($"[TutorialManager AddTutorialCard] Added {count}x {cardName} to the deck.");
        }
        else
        {
            Debug.LogWarning($"[TutorialManager AddTutorialCard] Card '{cardName}' not found in CardLibrary.");
        }
    }

    /// <summary>
    /// Subscribes to game events for handling tutorial steps.
    /// </summary>
    private void SubscribeToGameEvents()
    {
        Debug.Log("[TutorialManager SubscribeToGameEvents] Subscribing to GameManager events.");
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged += HandleMultiplierChanged;
    }

    /// <summary>
    /// Unsubscribes from game events to prevent memory leaks.
    /// </summary>
    private void UnsubscribeFromGameEvents()
    {
        Debug.Log("[TutorialManager UnsubscribeFromGameEvents] Unsubscribing from GameManager events.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnMultiplierChanged -= HandleMultiplierChanged;
        }
    }

    /// <summary>
    /// Begins the tutorial sequence.
    /// </summary>
    /// <returns></returns>
    private IEnumerator BeginTutorialSequence()
    {
        Debug.Log("[TutorialManager BeginTutorialSequence] Beginning tutorial sequence.");
        yield return new WaitForSeconds(1f);
        ShowIntroDialogue();
    }

    /// <summary>
    /// Displays the introduction dialogue.
    /// </summary>
    private void ShowIntroDialogue()
    {
        Debug.Log("[TutorialManager ShowIntroDialogue] Showing introduction dialogue.");
        StartCoroutine(ShowDialogueLines(new string[]
        {
            "Hi, I'm Shelly! Welcome to the Fresh Catch tutorial!",
            "We'll start simple. Your goal: Earn 50 points.",
            "You have special cards: Clownfish and Anemone. Clownfish score points. Anemone boosts your multiplier!",
            "First, drag both Clownfish cards from your hand to the stage area to form a set, then press 'Play' to score."
        }, OnIntroDialogueComplete));
    }

    /// <summary>
    /// Callback after the intro dialogue completes.
    /// </summary>
    private void OnIntroDialogueComplete()
    {
        Debug.Log("[TutorialManager OnIntroDialogueComplete] Intro dialogue complete. Moving to ExplainCards step.");
        _currentStep = TutorialStep.ExplainCards;
    }

    /// <summary>
    /// Handles score changes to progress the tutorial.
    /// </summary>
    /// <param name="newScore">The new score value.</param>
    private void HandleScoreChanged(int newScore)
    {
        Debug.Log($"[TutorialManager HandleScoreChanged] Score changed to {newScore}. Current Step: {_currentStep}");
        if (!IsTutorialMode) return;

        if (_currentStep == TutorialStep.ExplainCards && newScore > 0)
        {
            Debug.Log("[TutorialManager HandleScoreChanged] Player scored during ExplainCards step. Showing multiplier explanation.");
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
            Debug.Log("[TutorialManager HandleScoreChanged] Score reached 50 during WaitForMultiplierPlay step. Showing conclusion.");
            _currentStep = TutorialStep.Conclusion;
            ShowConclusion();
        }
    }

    /// <summary>
    /// Handles multiplier changes to progress the tutorial.
    /// </summary>
    /// <param name="newMultiplier">The new multiplier value.</param>
    private void HandleMultiplierChanged(int newMultiplier)
    {
        Debug.Log($"[TutorialManager HandleMultiplierChanged] Multiplier changed to {newMultiplier}. Current Step: {_currentStep}");
        if (!IsTutorialMode) return;

        if (_currentStep == TutorialStep.ExplainMultiplier && newMultiplier > 1)
        {
            Debug.Log("[TutorialManager HandleMultiplierChanged] Multiplier activated. Showing multiplier usage instructions.");
            StartCoroutine(ShowDialogueLines(new string[]
            {
                "Awesome! You've activated the multiplier.",
                "Play another set to see how it boosts your score!"
            }, OnMultiplierUsageComplete));
        }
    }

    /// <summary>
    /// Callback after the multiplier explanation dialogue completes.
    /// </summary>
    private void OnMultiplierExplanationComplete()
    {
        Debug.Log("[TutorialManager OnMultiplierExplanationComplete] Multiplier explanation dialogue complete.");
        _currentStep = TutorialStep.ExplainMultiplier;
    }

    /// <summary>
    /// Callback after the multiplier usage dialogue completes.
    /// </summary>
    private void OnMultiplierUsageComplete()
    {
        Debug.Log("[TutorialManager OnMultiplierUsageComplete] Multiplier usage dialogue complete.");
        _currentStep = TutorialStep.WaitForMultiplierPlay;
    }

    /// <summary>
    /// Shows the conclusion dialogue and transitions to the main menu.
    /// </summary>
    private void ShowConclusion()
    {
        if (!_tutorialComplete)
        {
            Debug.Log("[TutorialManager ShowConclusion] Showing conclusion dialogue.");
            _tutorialComplete = true;
            StartCoroutine(ShowDialogueLines(new string[]
            {
                "Fantastic! You reached 50 points!",
                "You now understand how to play cards, form sets, and use the Anemone to boost multipliers!",
                "Let's return to the main menu and start the real game."
            }, OnConclusionComplete));
        }
    }

    /// <summary>
    /// Callback after the conclusion dialogue completes.
    /// </summary>
    private void OnConclusionComplete()
    {
        Debug.Log("[TutorialManager OnConclusionComplete] Conclusion dialogue complete. Returning to main menu.");
        StartCoroutine(ReturnToMenuCoroutine());
    }

    /// <summary>
    /// Coroutine to handle returning to the main menu after the tutorial.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReturnToMenuCoroutine()
    {
        yield return new WaitForSeconds(2f);
        UnsubscribeFromGameEvents();
        SceneManager.LoadScene("MainMenu"); // Replace with your actual main menu scene name
    }

    /// <summary>
    /// Displays multiple lines of dialogue sequentially.
    /// </summary>
    /// <param name="lines">Array of dialogue lines to display.</param>
    /// <param name="onComplete">Callback after all dialogues are displayed.</param>
    /// <returns></returns>
    private IEnumerator ShowDialogueLines(string[] lines, Action onComplete = null)
    {
        var shelly = GameManager.Instance?.ShellyController;
        if (shelly == null)
        {
            Debug.LogError("[TutorialManager ShowDialogueLines] ShellyController is null.");
            yield break;
        }

        foreach (var line in lines)
        {
            while (_isDialogueInProgress)
            {
                yield return null;
            }

            _isDialogueInProgress = true;
            shelly.ActivateTextBox(line);

            // Wait until Shelly finishes the dialogue
            yield return new WaitUntil(() => !_isDialogueInProgress);
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Called by ShellyController when a dialogue line ends.
    /// </summary>
    public void EndDialogue()
    {
        Debug.Log("[TutorialManager EndDialogue] Dialogue ended.");
        _isDialogueInProgress = false;
    }

    /// <summary>
    /// Handles the completion of the tutorial by showing final dialogues and returning to the main menu.
    /// </summary>
    public void HandleTutorialCompletion()
    {
        Debug.Log("[TutorialManager HandleTutorialCompletion] Handling tutorial completion.");
        ShowConclusion();
    }
}