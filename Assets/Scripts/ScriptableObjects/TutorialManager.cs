using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private bool tutorialSkipped = false;

    private void Start()
    {
        Debug.Log("[TUTORIAL] TutorialManager started.");

        // Subscribe to DialogueManager's skip event
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnSkipTutorial += OnSkipTutorial;
        }
        else
        {
            Debug.LogError("[TUTORIAL] DialogueManager instance is null! Ensure DialogueManager is in the scene.");
        }

        // Check if GameManager is properly initialized
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TUTORIAL] GameManager instance is null! Ensure GameManager is in the scene.");
            return;
        }

        StartCoroutine(TutorialSequence());
    }

    private void OnSkipTutorial()
    {
        Debug.Log("[TUTORIAL] Tutorial skipped by the user.");
        tutorialSkipped = true;
        StopAllCoroutines();
        EndTutorial();
    }

    private IEnumerator TutorialSequence()
    {
        Debug.Log("[TUTORIAL] Starting TutorialSequence.");

        GameManager.Instance.IsInTutorial = true;

        // Step 1: Introduction
        yield return StartCoroutine(Step1Introduction());

        // Step 2: Setup and Draw Cards
        yield return StartCoroutine(Step2DrawCards());

        // Step 3: Play Cards Explanation
        yield return StartCoroutine(Step3PlayCards());

        // Step 4: Let the Player Play
        yield return StartCoroutine(Step4PlayerTurn());

        EndTutorial();
    }

    private IEnumerator Step1Introduction()
    {
        Debug.Log("[TUTORIAL] Step 1: Introduction.");
        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Welcome to Fresh Catch!",
            "This tutorial will guide you through the basics of the game.",
            "Let's start with how to draw and play cards."
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());
        Debug.Log("[TUTORIAL] Step 1 completed.");
    }

    private IEnumerator Step2DrawCards()
    {
        Debug.Log("[TUTORIAL] Step 2: Drawing cards.");

        // Setup tutorial deck
        SetupTutorialDeck();

        yield return new WaitForSeconds(1f); // Small delay for deck setup

        // Draw specific cards
        List<string> cardsToDraw = new List<string> { "Clownfish", "Turtle", "Kraken", "Net", "Bullshark" };
        Debug.Log("[TUTORIAL] Drawing specific hand: " + string.Join(", ", cardsToDraw));
        GameManager.Instance.DrawSpecificHand(cardsToDraw);

        yield return new WaitForSeconds(2f); // Wait for cards to appear in the player's hand

        if (GameManager.Instance.PlayerHand.NumCardsInHand == cardsToDraw.Count)
        {
            Debug.Log("[TUTORIAL] Cards successfully drawn for Step 2.");
            DialogueManager.Instance?.StartDialogue(new string[]
            {
                "These are some cards you can use.",
                "Each card has unique effects. Let's learn how to use them!"
            });
        }
        else
        {
            Debug.LogError("[TUTORIAL] Failed to draw the correct cards for Step 2.");
        }

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());
        Debug.Log("[TUTORIAL] Step 2 completed.");
    }

    private IEnumerator Step3PlayCards()
    {
        Debug.Log("[TUTORIAL] Step 3: Demonstrating card play.");

        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Let's play some cards and explain their effects.",
            "For example, the Kraken clears the stage, and the Net collects specific ranks."
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        yield return StartCoroutine(AutomatedTurn("Clownfish", new string[]
        {
            "We played 'Clownfish' to draw an extra card.",
            "Drawing extra cards gives you more options to play strategically."
        }));

        yield return StartCoroutine(AutomatedTurn("Kraken", new string[]
        {
            "We played 'Kraken' to clear the stage.",
            "This is helpful when you want to reset your cards and start fresh."
        }));

        yield return StartCoroutine(AutomatedTurn("Net", new string[]
        {
            "We played 'Net' to capture all cards of a specific rank.",
            "This helps gather more cards for scoring."
        }));

        Debug.Log("[TUTORIAL] Step 3 completed.");
    }

    private IEnumerator Step4PlayerTurn()
    {
        Debug.Log("[TUTORIAL] Step 4: Player's turn.");

        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Now it's your turn!",
            "Use what you've learned to play, draw, and score points.",
            "Good luck!"
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        Debug.Log("[TUTORIAL] Step 4 completed. Player control handed over.");
        GameManager.Instance.IsInTutorial = false;
    }

    private IEnumerator AutomatedTurn(string cardName, string[] explanation)
    {
        Debug.Log($"[TUTORIAL] Attempting to play card: {cardName}.");

        GameCard cardToPlay = GameManager.Instance.PlayerHand.CardsInHand.Find(card => card.Data.CardName == cardName);
        if (cardToPlay != null)
        {
            GameManager.Instance.TryDropCard(GameManager.Instance.Stage.transform, cardToPlay);
            Debug.Log($"[TUTORIAL] Successfully played card: {cardName}.");

            DialogueManager.Instance?.StartDialogue(explanation);

            yield return new WaitUntil(() =>
                DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());
        }
        else
        {
            Debug.LogWarning($"[TUTORIAL] Card '{cardName}' not found in hand. Skipping turn.");
        }
    }

    private void SetupTutorialDeck()
    {
        Debug.Log("[TUTORIAL] Setting up tutorial deck.");
        if (GameManager.Instance.GameDeck == null)
        {
            Debug.LogError("[TUTORIAL] GameDeck is null. Cannot setup tutorial deck.");
            return;
        }

        GameManager.Instance.GameDeck.ClearDeck();

        GameManager.Instance.GameDeck.AddCards(new Dictionary<string, int>
        {
            { "Clownfish", 2 },
            { "Turtle", 2 },
            { "Kraken", 1 },
            { "Net", 1 },
            { "Bullshark", 2 }
        });

        GameManager.Instance.GameDeck.ShuffleDeck();
        Debug.Log("[TUTORIAL] Tutorial deck setup complete.");
    }

    private void EndTutorial()
    {
        Debug.Log("[TUTORIAL] Tutorial ended.");
        GameManager.Instance.IsInTutorial = false;

        // Optionally add post-tutorial logic here
    }
}
