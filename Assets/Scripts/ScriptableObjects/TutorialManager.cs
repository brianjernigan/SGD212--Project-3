using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private bool tutorialSkipped = false; // Declare the variable

    private void Start()
    {
        // Subscribe to the OnSkipTutorial event
        DialogueManager.Instance.OnSkipTutorial += OnSkipTutorial;
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // Ensure the game is in tutorial mode
        GameManager.Instance.IsInTutorial = true;

        // Step 1: Introduction
        DialogueManager.Instance.StartDialogue(new string[]
        {
            "Welcome to Fresh Catch!",
            "In this tutorial, we'll show you how to play cards and understand their effects.",
            "Let's get started!"
        });

        // Wait for dialogue to finish or tutorial to be skipped
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive() || tutorialSkipped);

        if (tutorialSkipped)
        {
            yield break; // Exit the coroutine if skipped
        }

        // Step 2: Setup Tutorial Deck
        SetupTutorialDeck();

        // Step 3: Draw Cards
        DialogueManager.Instance.StartDialogue(new string[]
        {
            "First, we'll draw a hand of cards."
        });

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive() || tutorialSkipped);

        if (tutorialSkipped)
        {
            yield break;
        }

        // Draw specific cards
        GameManager.Instance.DrawSpecificHand(new List<string> { "Clownfish", "Turtle", "Kraken", "Net", "Bullshark" });
        yield return new WaitForSeconds(2f); // Wait for cards to be drawn

        if (tutorialSkipped)
        {
            yield break;
        }

        // Step 4: Explain Hand
        DialogueManager.Instance.StartDialogue(new string[]
        {
            "Here are the cards we've drawn.",
            "Let's see how we can use them."
        });

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive() || tutorialSkipped);

        if (tutorialSkipped)
        {
            yield break;
        }

        // Automated Turns
        yield return StartCoroutine(AutomatedTurns());

        if (tutorialSkipped)
        {
            yield break;
        }

        // Step 5: Transition to Player Control
        DialogueManager.Instance.StartDialogue(new string[]
        {
            "Now it's your turn to play!",
            "Use what you've learned to score points and win the game.",
            "Good luck!"
        });

        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive() || tutorialSkipped);

        if (tutorialSkipped)
        {
            yield break;
        }

        // End of tutorial
        GameManager.Instance.IsInTutorial = false;
        Debug.Log("Tutorial complete.");
    }

    private void SetupTutorialDeck()
    {
        // Create a small deck with predefined cards
        GameManager.Instance.GameDeck.ClearDeck();
        GameManager.Instance.GameDeck.AddCards(new Dictionary<string, int>
        {
            { "Clownfish", 2 },
            { "Turtle", 2 },
            { "Kraken", 1 },
            { "Net", 1 },
            { "Bullshark", 2 },
            { "Seahorse", 2 }
        });
        GameManager.Instance.GameDeck.ShuffleDeck();
    }

    private IEnumerator AutomatedTurns()
    {
        // Turn 1: Play Clownfish
        yield return StartCoroutine(PlayCardAndExplain("Clownfish", new string[]
        {
            "We played 'Clownfish' because it allows us to draw an extra card.",
            "This helps us get more options in our hand."
        }));

        if (tutorialSkipped)
        {
            yield break;
        }

        // Turn 2: Play Kraken
        yield return StartCoroutine(PlayCardAndExplain("Kraken", new string[]
        {
            "The 'Kraken' card clears all cards on the stage.",
            "It's useful when you want to reset the game state."
        }));

        // Add more automated turns as needed...
    }

    private IEnumerator PlayCardAndExplain(string cardName, string[] explanations)
    {
        if (tutorialSkipped)
        {
            yield break;
        }

        // Find the card in hand
        GameCard cardToPlay = GameManager.Instance.PlayerHand.CardsInHand.Find(card => card.Data.CardName == cardName);
        if (cardToPlay != null)
        {
            // Play the card to the stage
            GameManager.Instance.TryDropCard(GameManager.Instance.Stage.transform, cardToPlay);

            // Wait for the card to be placed
            yield return new WaitForSeconds(1f);

            if (tutorialSkipped)
            {
                yield break;
            }

            // Provide explanations
            DialogueManager.Instance.StartDialogue(explanations);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive() || tutorialSkipped);
        }
    }

    private void OnSkipTutorial()
    {
        tutorialSkipped = true;
        StopAllCoroutines();
        EndTutorial();
    }

    private void EndTutorial()
    {
        GameManager.Instance.IsInTutorial = false;
        Debug.Log("Tutorial skipped or completed.");
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnSkipTutorial -= OnSkipTutorial;
        }
    }
}
