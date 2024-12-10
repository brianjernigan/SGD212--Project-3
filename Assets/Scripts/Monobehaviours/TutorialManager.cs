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

    private bool _isDialogueInProgress;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.IsTutorialMode = true;
                GameManager.Instance.EnableNormalDialogue = false;
                IsTutorialMode = true;
            }
        }
        else
        {
            Destroy(gameObject);
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
        if (!scene.name.Equals("TutorialScene", StringComparison.OrdinalIgnoreCase))
        {
            DisableAndDestroy();
        }
        else
        {
            enabled = true;
            InitializeTutorial();
        }
    }

    public void InitializeTutorial()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsTutorialMode)
        {
            return;
        }

        SetupTutorialDeck();
        StartCoroutine(BeginTutorialSequence());
        SubscribeToGameEvents();
    }

    private void SetupTutorialDeck()
    {
        if (GameManager.Instance == null || GameManager.Instance.GameDeck == null || GameManager.Instance.PlayerHand == null)
        {
            return;
        }

        GameManager.Instance.GameDeck.CardDataInDeck.Clear();
        GameManager.Instance.PlayerHand.ClearHandArea();

        AddTutorialCard("ClownFish", ClownfishCount);
        AddTutorialCard("Anemone", AnemoneCount);
        AddTutorialCard("Kraken", KrakenCount);

        GameManager.Instance.GameDeck.ShuffleDeck();
        GameManager.Instance.StartDrawFullHandCoroutine(false);
    }

    private void AddTutorialCard(string cardName, int count)
    {
        if (CardLibrary.Instance == null)
        {
            return;
        }

        var cardData = CardLibrary.Instance.GetCardDataByName(cardName);
        if (cardData != null)
        {
            GameManager.Instance.GameDeck.AddCard(cardData, count);
        }
    }

    private void SubscribeToGameEvents()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnScoreChanged += HandleScoreChanged;
        GameManager.Instance.OnMultiplierChanged += HandleMultiplierChanged;
    }

    public void UnsubscribeFromGameEvents()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

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
        _currentStep = TutorialStep.ExplainCards;
    }

    private void HandleScoreChanged(int newScore)
    {
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
        _isDialogueInProgress = false;
    }

    public void HandleTutorialCompletion()
    {
        ShowConclusion();
    }

    private void DisableAndDestroy()
    {
        UnsubscribeFromGameEvents();

        // Reset tutorial flags in GameManager if present
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IsTutorialMode = false;
            GameManager.Instance.EnableNormalDialogue = true;
        }

        Destroy(gameObject);
    }

    private void EnableTutorialManager()
    {
        enabled = true;

        // Re-initialize tutorial if needed
        if (GameManager.Instance != null && GameManager.Instance.IsTutorialMode)
        {
            InitializeTutorial();
        }
    }
}
