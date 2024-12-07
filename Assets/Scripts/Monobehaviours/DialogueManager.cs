using System; // Added to recognize 'Action'
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button helpButton; 
    [SerializeField] private Button skipButton; 

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float autoAdvanceDelay = 1.5f; // Delay before auto-advancing to next line

    public event Action OnDialogueComplete;
    public event Action OnSkipTutorial;

    private Queue<string> dialogueQueue;
    private bool isTyping = false;
    private bool autoAdvance = false; 

    // Tracks how many times we've forced the user to click continue.
    // Once this reaches 5, all subsequent dialogue will auto-advance.
    private int totalManualContinuesUsed = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DIALOGUE] DialogueManager initialized.");
        }
        else
        {
            Debug.LogWarning("[DIALOGUE] Duplicate DialogueManager found and destroyed.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        dialogueQueue = new Queue<string>();

        if (dialogueCanvasGroup == null)
        {
            Debug.LogError("[DIALOGUE] CanvasGroup is not assigned!");
        }
        else
        {
            dialogueCanvasGroup.alpha = 0f;
            dialogueCanvasGroup.interactable = false;
            dialogueCanvasGroup.blocksRaycasts = false;
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            continueButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[DIALOGUE] Continue button is not assigned!");
        }

        if (helpButton != null)
        {
            helpButton.onClick.AddListener(DisplayInDepthHelp);
            helpButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[DIALOGUE] Help button is not assigned!");
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
            skipButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[DIALOGUE] Skip button is not assigned!");
        }

        // Example initial dialogue in auto-advance mode:
        StartDialogue(new string[]
        {
            "Welcome to Fresh Catch!",
            "Your goal is to achieve the required score for each level.",
            "This dialogue will mostly auto-advance after each line, so sit back and enjoy!"
        }, true); // Setting autoAdvance = true here
    }

    /// <summary>
    /// Starts a dialogue sequence.
    /// </summary>
    /// <param name="lines">The array of dialogue lines to display.</param>
    /// <param name="autoAdvanceDialogue">If true, dialogue auto-advances without clicks. If false, user must click to continue (unless they've clicked continue 5 times before).</param>
    public void StartDialogue(string[] lines, bool autoAdvanceDialogue = false)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[DIALOGUE] No lines provided for dialogue.");
            return;
        }

        ClearDialogue();
        autoAdvance = autoAdvanceDialogue;

        foreach (var line in lines)
        {
            dialogueQueue.Enqueue(line);
        }

        if (!isTyping)
        {
            StartCoroutine(DisplayNextDialogue());
        }
    }

    private IEnumerator DisplayNextDialogue()
    {
        if (dialogueQueue.Count == 0)
        {
            Debug.Log("[DIALOGUE] No more dialogue in the queue.");
            yield break;
        }

        string line = dialogueQueue.Dequeue();

        Debug.Log($"[DIALOGUE] Displaying line: {line}");

        yield return StartCoroutine(FadeInDialoguePanel());

        yield return StartCoroutine(TypeSentence(line));

        // Check if we should force manual continue or auto advance
        bool shouldAutoAdvance = autoAdvance || (totalManualContinuesUsed >= 5);

        if (!shouldAutoAdvance)
        {
            // Manual mode: show continue button
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
            }

            // If this is the last line, show help button and hide skip button
            if (dialogueQueue.Count == 0)
            {
                if (helpButton != null) helpButton.gameObject.SetActive(true);
                if (skipButton != null) skipButton.gameObject.SetActive(false);
            }
            else if (skipButton != null)
            {
                skipButton.gameObject.SetActive(true);
            }
        }
        else
        {
            // Auto-advance mode
            if (dialogueQueue.Count > 0)
            {
                if (skipButton != null) skipButton.gameObject.SetActive(true);
                yield return new WaitForSeconds(autoAdvanceDelay);
                StartCoroutine(DisplayNextDialogue());
            }
            else
            {
                // No more lines; end dialogue after a delay
                yield return new WaitForSeconds(autoAdvanceDelay);
                Debug.Log("[DIALOGUE] Dialogue sequence complete (auto-advance).");
                StartCoroutine(FadeOutDialoguePanel());
                OnDialogueComplete?.Invoke();
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private IEnumerator FadeInDialoguePanel()
    {
        if (dialogueCanvasGroup == null)
        {
            Debug.LogError("[DIALOGUE] CanvasGroup is not assigned for fading.");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1f;
        dialogueCanvasGroup.interactable = true;
        dialogueCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOutDialoguePanel()
    {
        if (dialogueCanvasGroup == null)
        {
            Debug.LogError("[DIALOGUE] CanvasGroup is not assigned for fading.");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
    }

    private void OnContinueButtonClicked()
    {
        // User clicked continue, increment the manual continues used
        totalManualContinuesUsed++;

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        if (dialogueQueue.Count > 0)
        {
            StartCoroutine(DisplayNextDialogue());
        }
        else
        {
            Debug.Log("[DIALOGUE] Dialogue sequence complete (manual mode).");
            StartCoroutine(FadeOutDialoguePanel());
            OnDialogueComplete?.Invoke(); // Notify listeners that dialogue is complete
        }
    }

    public bool IsDialogueActive()
    {
        return dialogueCanvasGroup.alpha > 0 || isTyping;
    }

    public void ClearDialogue()
    {
        dialogueQueue.Clear();
        dialogueText.text = "";
        StopAllCoroutines();
        // Fade out panel immediately
        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
        isTyping = false;
    }

    private void DisplayInDepthHelp()
    {
        // Display help, auto-advance since it's just info
        StartDialogue(new string[]
        {
            "More details on how to play Fresh Catch:",
            "1. Flipping Cards: Right-click on a card to flip it.",
            "2. Staging Cards: Drag cards onto the stage to form sets.",
            "3. Resource Management: Plays, Draws, and Discards are limited, use them wisely!",
            "4. Multipliers: Larger sets increase your multiplier and yield bonus points.",
            "5. Winning: Score enough points to advance levels.",
            "Good luck, and have fun!"
        }, true); // Automatically advance help text

        if (helpButton != null)
        {
            helpButton.gameObject.SetActive(false);
        }
    }

    private void SkipDialogue()
    {
        Debug.Log("[DIALOGUE] Skipping dialogue.");
        dialogueQueue.Clear();

        StartCoroutine(FadeOutDialoguePanel());

        if (helpButton != null) helpButton.gameObject.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(false);

        OnSkipTutorial?.Invoke();
    }
}
