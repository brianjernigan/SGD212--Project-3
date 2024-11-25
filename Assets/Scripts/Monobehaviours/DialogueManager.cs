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
    [SerializeField] private Button helpButton; // Button for detailed help
    [SerializeField] private Button skipButton; // Button to skip dialogue

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Queue<string> dialogueQueue;
    private bool isTyping = false;

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

        // Start introductory dialogue
        Debug.Log("[DIALOGUE] Starting intro dialogue.");
        StartDialogue(new string[]
        {
            "Welcome to Fresh Catch!",
            "Your goal is to achieve the required score for each level:",
            "Level 1: 50 points, Level 2: 100 points, Level 3: 150 points.",
            "Play cards strategically to form sets and maximize your score.",
            "Use your resources wisely: Plays, Draws, and Discards are limited.",
            "Place matching cards or special cards like the Kraken to score points.",
            "Keep an eye on your multiplier for bonus scores!",
            "If you need more detailed help, press the 'Help' button!"
        });
    }

    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[DIALOGUE] No lines provided for dialogue.");
            return;
        }

        dialogueQueue.Clear();
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

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

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

    private void OnContinueButtonClicked()
    {
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
            Debug.Log("[DIALOGUE] Dialogue sequence complete.");
            StartCoroutine(FadeOutDialoguePanel());
        }
    }

    private void SkipDialogue()
    {
        Debug.Log("[DIALOGUE] Skipping dialogue.");
        dialogueQueue.Clear();

        StartCoroutine(FadeOutDialoguePanel());

        if (helpButton != null) helpButton.gameObject.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(false);
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

    private void DisplayInDepthHelp()
    {
        StartDialogue(new string[]
        {
            "Here are more details on how to play Fresh Catch:",
            "1. Flipping Cards: Right-click on a card to flip it and reveal hidden information.",
            "2. Staging Cards: Drag cards into the stage area to create sets. Matching ranks or using special cards scores points.",
            "3. Managing Resources: Use your plays, draws, and discards carefully—they're limited!",
            "4. Multiplier: Create larger sets to increase your multiplier for bonus points.",
            "5. Winning: Score enough points to advance levels. Keep an eye on the score requirements!",
            "Good luck, and have fun!"
        });

        if (helpButton != null)
        {
            helpButton.gameObject.SetActive(false);
        }
    }
}
