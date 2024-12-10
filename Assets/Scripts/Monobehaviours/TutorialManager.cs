using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public TutorialStep CurrentStep { get; set; } = TutorialStep.Introduction;
    private bool _tutorialComplete;
    private bool _isDialogueInProgress;

    public void SetTutorialStep(TutorialStep newStep)
    {
        if (CurrentStep == newStep) return;
        
        CurrentStep = newStep;
        HandleStepChange(CurrentStep);
    }
    
    private void HandleStepChange(TutorialStep currentStep)
    {
        switch (currentStep)
        {
            case TutorialStep.Introduction:
                ShowIntroDialogue();
                break;

            case TutorialStep.DiscardStep:
                StartManualDiscardStep();
                break;

            case TutorialStep.ActivateAnemone:
                StartPlayAnemoneStep();
                break;

            case TutorialStep.ScoreThreeSet:
                StartScoreThreeSetStep();
                break;

            case TutorialStep.DrawNewHandOne:
                StartDrawNewHandOneStep();
                break;

            case TutorialStep.ScoreFourSet:
                StartScoreFourSetStep();
                break;

            case TutorialStep.DrawNewHandTwo:
                StartDrawNewHandTwoStep();
                break;

            case TutorialStep.ActivateHammerhead:
                StartActivateHammerheadStep();
                break;

            case TutorialStep.ActivateFishEggs:
                StartActivateFishEggsStep();
                break;

            case TutorialStep.ScoreWhaleShark:
                StartScoreWhaleSharkStep();
                break;

            case TutorialStep.Conclusion:
                ShowConclusion();
                break;

            default:
                Debug.LogWarning($"Unhandled tutorial step: {currentStep}");
                break;
        }
    }

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
    }

    public void InitializeTutorial()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsTutorialMode)
        {
            return;
        }

        SetupTutorialDeck();
        StartCoroutine(BeginTutorialSequence());
        GameManager.Instance.StartCoroutine(GameManager.Instance.DrawInitialHandCoroutine());
    }

    private void SetupTutorialDeck()
    {
        if (GameManager.Instance == null || GameManager.Instance.GameDeck == null || GameManager.Instance.PlayerHand == null)
        {
            return;
        }
        
        GameManager.Instance.PlayerHand.ClearHandArea();
    }

    private IEnumerator BeginTutorialSequence()
    {
        yield return new WaitForSeconds(1f);
        ShowIntroDialogue();
    }

    private void ShowIntroDialogue()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Hi, I'm Shelly! Welcome to the Fresh Catch tutorial!",
            "First, let's learn how to discard. Discarding frees up space in your hand for more cards.",
            "Go ahead and drag your Bullshark, Stingray, and Plankton cards overtop the vortex to discard them."
        }, OnIntroDialogueComplete));
    }

    private void OnIntroDialogueComplete()
    {
        SetTutorialStep(TutorialStep.DiscardStep);
    }
    
    private void StartPlayAnemoneStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Great! Now that we've narrowed our hand down, we can start making moves.",
            "Drag that Anemone card to the stage area.",
            "Press Play to activate its effect and draw 2 clownfish to your hand."
        }));
    }

    private void StartManualDiscardStep()
    {
        Debug.Log("Waiting for discard");
    }

    private void StartScoreThreeSetStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Now, play three Clownfish cards to score your first set and earn points!"
        }));
    }

    private void StartScoreFourSetStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Oooh, it looks like you've got a Kraken card! Kraken cards can be played as any rank, like a wildcard.",
            "Even better, you've got 3 Planktons too!",
            "If you're able to score a set of 4 of a kind, you'll receive a 2x multiplier!",
            "Score your Kraken and Planktons to continue."
        }));
    }

    private void StartDrawNewHandOneStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Nice work! You scored your first points!",
            "Now, we need to draw some more cards so we can continue.",
            "Press the Draw button to draw new hand."
        }));
    }
    
    private void StartDrawNewHandTwoStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "We're almost there!",
            "Draw another hand and let's get out of here."
        }));
    }

    private void StartActivateHammerheadStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Nice! Certain cards have special effects based on the cards remaining in your deck or hand.",
            "That Hammerhead card will give add to your multiplier based on the number of Stingrays.",
            "Why don't you play it and increase that multiplier?"
        }));
    }

    private void StartActivateFishEggsStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Whoa, we can really rack up some points now.",
            "I see you have some Fish Eggs in your hand.",
            "Fish Eggs will transform into a random card in your hand when activated.",
            "Since we shaved our hand down, its only option is to transform into that Whaleshark.",
            "Go ahead and activate it now!"
        }));
    }

    private void StartScoreWhaleSharkStep()
    {
        StartCoroutine(ShowDialogueLines(new[]
        {
            "Whalesharks are another special card. They can be played as a set of 1, 2, 3, or 4.",
            "They also add to your multiplier based on the amount of plankton left in your deck.",
            "This is shaping up to be a big set! Score your two Whalesharks and we can get to playing the real thing!"
        }));
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
