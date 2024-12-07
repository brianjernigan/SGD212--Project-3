using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private bool tutorialSkipped = false;
    private bool waitingForScore = false;

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

        // Listen for score changes to detect when player hits 50 points.
        GameManager.Instance.OnScoreChanged += OnScoreChanged;

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

        // Step 2: Explain Cards and Effects
        yield return StartCoroutine(Step2ExplainCards());

        // Step 3: Setup the Deck and Draw Initial Hand
        yield return StartCoroutine(Step3SetupAndDraw());

        // Step 4: Automated Full Game to 50 Points
        yield return StartCoroutine(Step4PlayTo50Points());

        EndTutorial();
    }

    private IEnumerator Step1Introduction()
    {
        Debug.Log("[TUTORIAL] Step 1: Introduction.");

        DialogueManager.Instance?.ClearDialogue();
        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Welcome to Fresh Catch!",
            "In this card game, you'll draw cards, form sets, and score points.",
            "Each card type can have unique effects, influencing your strategy.",
            "In this tutorial, we'll guide you through a fully automated game, showing each step.",
            "By the end, you'll have at least 50 points and a solid understanding of the game."
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        Debug.Log("[TUTORIAL] Step 1 completed.");
    }

    private IEnumerator Step2ExplainCards()
    {
        Debug.Log("[TUTORIAL] Step 2: Explain Cards and Effects.");

        DialogueManager.Instance?.ClearDialogue();
        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Let's go over some card effects and strategies before we start:",
            "- Clownfish: Increases your next played set's multiplier if you have Anemones.",
            "- Turtle: Lets you draw extra cards next turn.",
            "- Kraken: Acts as any rank, helping you complete sets.",
            "- WhaleShark: Its score multiplies based on how many Plankton remain in the deck.",
            "- Net: Helps you draw pairs of cards.",
            "- Orca, Stingray, Hammerhead, and more each alter your deck, discard cards, or modify your hand.",
            "We'll see some of these effects in action as we play to 50 points."
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        Debug.Log("[TUTORIAL] Step 2 completed.");
    }

    private IEnumerator Step3SetupAndDraw()
    {
        Debug.Log("[TUTORIAL] Step 3: Setup and Draw Initial Hand.");

        // Setup the deck with a variety of cards
        SetupTutorialDeck();

        yield return new WaitForSeconds(1f);

        DialogueManager.Instance?.ClearDialogue();
        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "We've prepared a special deck with a variety of cards.",
            "First, we'll draw a starting hand and then begin playing automatically.",
            "Watch closely as we form sets, use card effects, and increment our score."
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        // Draw a hand of cards to start with
        // Let's just draw a few known cards to begin the play
        List<string> initialCards = new List<string> { "Clownfish", "Turtle", "Kraken", "Whaleshark", "Net" };
        GameManager.Instance.DrawSpecificHand(initialCards);

        yield return new WaitForSeconds(2f);

        Debug.Log("[TUTORIAL] Step 3 completed.");
    }

    private IEnumerator Step4PlayTo50Points()
    {
        Debug.Log("[TUTORIAL] Step 4: Playing Automatically to 50 Points.");

        DialogueManager.Instance?.ClearDialogue();
        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Now we'll play through the game automatically.",
            "We'll draw cards, place them on the stage, and press 'Play' to score sets.",
            "As we proceed, watch how the score increments and how different cards help us reach our goal.",
            "Sit back, relax, and observe!"
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        waitingForScore = true;

        // Now we automate the process of playing until score >= 50
        while (GameManager.Instance.CurrentScore < 50)
        {
            // Draw more cards if needed
            if (GameManager.Instance.DrawsRemaining > 0 && GameManager.Instance.CardsOnScreen < GameManager.Instance.HandSize)
            {
                GameManager.Instance.DrawFullHand();
                yield return new WaitForSeconds(1f);
            }

            // Attempt to stage and play a set
            yield return StartCoroutine(AutomatedPlaySet());

            // Small delay to simulate thinking
            yield return new WaitForSeconds(1f);
        }

        DialogueManager.Instance?.ClearDialogue();
        DialogueManager.Instance?.StartDialogue(new string[]
        {
            "Excellent! We've reached 50 points!",
            "You saw how card effects can influence what you draw, how sets are formed, and how scoring works.",
            "Now that you've seen a full automated game, you're ready to try playing on your own.",
            "Good luck and have fun with Fresh Catch!"
        });

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive());

        waitingForScore = false;
        Debug.Log("[TUTORIAL] Step 4 completed, reached 50 points.");
    }

    private IEnumerator AutomatedPlaySet()
    {
        // Attempt to create a staged set:
        // Strategy: If we have multiple cards of the same rank, stage them.
        // Otherwise, stage a single WhaleShark or a Kraken and play it.
        
        var playerHand = GameManager.Instance.PlayerHand;
        var cardsInHand = playerHand.CardsInHand;

        if (cardsInHand.Count == 0) yield break;

        // Simple heuristic: Stage up to 3 cards of the same rank if possible.
        var groupsByRank = new Dictionary<int, List<GameCard>>();
        foreach (var card in cardsInHand)
        {
            if (!groupsByRank.ContainsKey(card.Data.CardRank))
            {
                groupsByRank[card.Data.CardRank] = new List<GameCard>();
            }
            groupsByRank[card.Data.CardRank].Add(card);
        }

        // Find a rank with multiple cards
        List<GameCard> bestSet = null;
        foreach (var kvp in groupsByRank)
        {
            if (kvp.Value.Count >= 3)
            {
                bestSet = kvp.Value.GetRange(0, 3);
                break;
            }
            else if (kvp.Value.Count == 2)
            {
                bestSet = kvp.Value;
            }
        }

        // If we couldn't find a set of 2 or more of the same rank,
        // just play a single WhaleShark or Kraken (if available) to score some points.
        if (bestSet == null)
        {
            var whale = cardsInHand.Find(c => c.Data.CardName == "Whaleshark");
            if (whale != null) bestSet = new List<GameCard> { whale };
            else
            {
                var kraken = cardsInHand.Find(c => c.Data.CardName == "Kraken");
                if (kraken != null) bestSet = new List<GameCard> { kraken };
                else
                {
                    // If no special card, just take any card
                    bestSet = new List<GameCard> { cardsInHand[0] };
                }
            }
        }

        // Stage the chosen cards
        foreach (var card in bestSet)
        {
            if (GameManager.Instance.TryDropCard(GameManager.Instance.Stage.transform, card))
            {
                GameManager.Instance.RearrangeStage();
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Now click the play button to score the set
        GameManager.Instance.OnClickPlayButton();
        yield return new WaitForSeconds(0.5f);
    }

    private void OnScoreChanged(int newScore)
    {
        if (waitingForScore && newScore >= 50)
        {
            Debug.Log("[TUTORIAL] Player reached 50 points!");
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

        // Add a variety of cards to ensure we can reach 50 points:
        // Plenty of pairs and multiples to form sets.
        GameManager.Instance.GameDeck.AddCards(new Dictionary<string, int>
        {
            { "Clownfish", 4 },
            { "Turtle", 4 },
            { "Kraken", 3 },
            { "Net", 2 },
            { "Bullshark", 4 },
            { "Whaleshark", 2 },
            { "Plankton", 10 },
            { "Orca", 2 },
            { "Stingray", 3 },
            { "Moray", 2 },
            { "CookieCutter", 2 },
            { "Anemone", 4 },
            { "Treasure", 4 },
            { "Hammerhead", 2 },
            { "FishEggs", 4 },
            { "Seahorse", 2 }
        });

        GameManager.Instance.GameDeck.ShuffleDeck();
        Debug.Log("[TUTORIAL] Tutorial deck setup complete.");
    }

    private void EndTutorial()
    {
        Debug.Log("[TUTORIAL] Tutorial ended.");
        StopAllCoroutines(); 
        GameManager.Instance.IsInTutorial = false;

        GameManager.Instance.RearrangeHand();
        GameManager.Instance.RearrangeStage();
    }
}
