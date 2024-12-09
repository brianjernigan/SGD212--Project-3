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
    [SerializeField] private int ClownfishCount = 4;
    [SerializeField] private int AnemoneCount = 2;
    [SerializeField] private int KrakenCount = 1;

    [Header("Dialogue Settings")]
    [SerializeField] private float DialogueWaitTime = 3f;
    private bool _isDialogueInProgress = false;

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
                GameManager.Instance.EnableNormalDialogue = false;
                IsTutorialMode = true;
                Debug.Log("[TutorialManager Awake] Tutorial mode enabled.");
            }
            else
            {
                Debug.LogError("[TutorialManager Awake] GameManager.Instance is null.");
            }
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[TutorialManager Awake] Duplicate TutorialManager destroyed.");
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[TutorialManager OnSceneLoaded] Scene '{scene.name}' loaded.");

        if (!scene.name.Equals("TutorialScene", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[TutorialManager] Not in TutorialScene, destroying TutorialManager.");
            DisableAndDestroy();
        }
        else
        {
            Debug.Log("[TutorialManager] In TutorialScene, enabling TutorialManager.");
            this.enabled = true;
            InitializeTutorial();
        }
    }

    public void InitializeTutorial()
    {
        Debug.Log("[TutorialManager InitializeTutorial]");

        if (GameManager.Instance == null || !GameManager.Instance.IsTutorialMode)
        {
            Debug.Log("[TutorialManager] Tutorial mode not active or GameManager null. Exiting.");
            return;
        }

        SetupTutorialDeck();
        StartCoroutine(BeginTutorialSequence());
        SubscribeToGameEvents();
    }

    private void SetupTutorialDeck()
    {
        Debug.Log("[TutorialManager SetupTutorialDeck]");

        if (GameManager.Instance == null || GameManager.Instance.GameDeck == null || GameManager.Instance.PlayerHand == null)
        {
            Debug.LogError("[TutorialManager SetupTutorialDeck] Missing GameManager or its components.");
            return;
        }

        GameManager.Instance.GameDeck.CardDataInDeck.Clear();
        GameManager.Instance.PlayerHand.ClearHandArea();

        AddTutorialCard("ClownFish", ClownfishCount);
        AddTutorialCard("Anemone", AnemoneCount);
        AddTutorialCard("Kraken", KrakenCount);

        GameManager.Instance.GameDeck.ShuffleDeck();
        Debug.Log("[TutorialManager] Deck shuffled, drawing initial hand.");
        GameManager.Instance.StartDrawFullHandCoroutine(false);
    }

    private void AddTutorialCard(string cardName, int count)
    {
        if (CardLibrary.Instance == null)
        {
            Debug.LogError("[TutorialManager AddTutorialCard] CardLibrary null.");
            return;
        }

        var cardData = CardLibrary.Instance.GetCardDataByName(cardName);
        if (cardData != null)
        {
            GameManager.Instance.GameDeck.AddCard(cardData, count);
            Debug.Log($"[TutorialManager AddTutorialCard] Added {count}x {cardName}.");
        }
        else
        {
            Debug.LogWarning($"[TutorialManager AddTutorialCard] Card '{cardName}' not found.");
        }
    }

    private void SubscribeToGameEvents()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TutorialManager SubscribeToGameEvents] GameManager.Instance null.");
            return;
        }

        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged += HandleMultiplierChanged;
    }

    public void UnsubscribeFromGameEvents()
    {
        Debug.Log("[TutorialManager UnsubscribeFromGameEvents]");

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[TutorialManager UnsubscribeFromGameEvents] GameManager.Instance null.");
            return;
        }

        GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged -= HandleMultiplierChanged;
    }

    private IEnumerator BeginTutorialSequence()
    {
        Debug.Log("[TutorialManager BeginTutorialSequence]");
        yield return new WaitForSeconds(1f);
        ShowIntroDialogue();
    }

    private void ShowIntroDialogue()
    {
        Debug.Log("[TutorialManager ShowIntroDialogue]");
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
        Debug.Log("[TutorialManager OnIntroDialogueComplete]");
        _currentStep = TutorialStep.ExplainCards;
    }

    private void HandleScoreChanged(int newScore)
    {
        Debug.Log($"[TutorialManager HandleScoreChanged] Score: {newScore}, Step: {_currentStep}");

        if (!IsTutorialMode) return;

        switch (_currentStep)
        {
            case TutorialStep.ExplainCards:
                if (newScore > 0)
                {
                    StartCoroutine(ShowDialogueLines(new string[]
                    {
                        "Great job! You scored points from the Clownfish set.",
                        "Now let's talk about the multiplier. Anemones add extra multipliers to future sets!",
                        "Drag an Anemone from your hand to the stage area. Then play another Clownfish set to see the multiplier!",
                        "Try to reach 50 points!"
                    }, OnMultiplierExplanationComplete));
                    _currentStep = TutorialStep.ExplainMultiplier;
                }
                break;

            case TutorialStep.WaitForMultiplierPlay:
                if (newScore >= 50)
                {
                    _currentStep = TutorialStep.Conclusion;
                    ShowConclusion();
                }
                break;
        }
    }

    private void HandleMultiplierChanged(int newMultiplier)
    {
        Debug.Log($"[TutorialManager HandleMultiplierChanged] Multiplier: {newMultiplier}, Step: {_currentStep}");

        if (!IsTutorialMode) return;

        if (_currentStep == TutorialStep.ExplainMultiplier && newMultiplier > 1)
        {
            StartCoroutine(ShowDialogueLines(new string[]
            {
                "Awesome! You've activated the multiplier.",
                "Play another set to see how it boosts your score!"
            }, OnMultiplierUsageComplete));
            _currentStep = TutorialStep.WaitForMultiplierPlay;
        }
    }

    private void OnMultiplierExplanationComplete()
    {
        Debug.Log("[TutorialManager OnMultiplierExplanationComplete]");
        _currentStep = TutorialStep.ExplainMultiplier;
    }

    private void OnMultiplierUsageComplete()
    {
        Debug.Log("[TutorialManager OnMultiplierUsageComplete]");
        _currentStep = TutorialStep.WaitForMultiplierPlay;
    }

    private void ShowConclusion()
    {
        if (!_tutorialComplete)
        {
            Debug.Log("[TutorialManager ShowConclusion]");
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
        Debug.Log("[TutorialManager OnConclusionComplete]");
        StartCoroutine(ReturnToMenuCoroutine());
    }

    private IEnumerator ReturnToMenuCoroutine()
    {
        yield return new WaitForSeconds(2f);
        UnsubscribeFromGameEvents();
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator ShowDialogueLines(string[] lines, Action onComplete = null)
    {
        var shelly = GameManager.Instance?.ShellyController;
        if (shelly == null)
        {
            Debug.LogError("[TutorialManager ShowDialogueLines] ShellyController null.");
            yield break;
        }

        foreach (var line in lines)
        {
            while (_isDialogueInProgress)
                yield return null;

            _isDialogueInProgress = true;
            shelly.ActivateTextBox(line);

            yield return new WaitUntil(() => !_isDialogueInProgress);
        }

        onComplete?.Invoke();
    }

    public void EndDialogue()
    {
        Debug.Log("[TutorialManager EndDialogue] Dialogue ended.");
        _isDialogueInProgress = false;
    }

    public void HandleTutorialCompletion()
    {
        Debug.Log("[TutorialManager HandleTutorialCompletion]");
        ShowConclusion();
    }

    private void DisableAndDestroy()
    {
        Debug.Log("[TutorialManager DisableAndDestroy]");

        UnsubscribeFromGameEvents();

        // Reset tutorial flags in GameManager if present
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsTutorialMode = false;
            GameManager.Instance.EnableNormalDialogue = true;
        }

        Destroy(gameObject);
        Debug.Log("[TutorialManager] Destroyed itself.");
    }

    private void EnableTutorialManager()
    {
        Debug.Log("[TutorialManager EnableTutorialManager]");
        this.enabled = true;

        // Re-initialize tutorial if needed
        if (GameManager.Instance != null && GameManager.Instance.IsTutorialMode)
        {
            InitializeTutorial();
        }
    }
}
