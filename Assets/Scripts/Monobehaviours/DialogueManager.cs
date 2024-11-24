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

        // Start introductory dialogue
        Debug.Log("[DIALOGUE] Starting intro dialogue.");
        StartDialogue(new string[]
        {
            "Welcome to Fresh Catch!",
            "The goal is to create as many matching sets as possible.",
            "Use your cards wisely and aim for the highest score!",
            "Good luck, and let's dive in!"
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
}
