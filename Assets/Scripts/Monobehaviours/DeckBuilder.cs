using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    public static DeckBuilder Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Builds the default deck with a broad variety of cards.
    /// Used as a base or fallback.
    /// </summary>
    public Deck BuildDefaultDeck(GameObject cardPrefab)
    {
        var defaultComposition = new Dictionary<string, int>
        {
            { "Plankton", 4 },
            { "FishEggs", 4 },
            { "Seahorse", 4 },
            { "ClownFish", 4 },
            { "CookieCutter", 4 },
            { "Turtle", 4 },
            { "Stingray", 4 },
            { "Bullshark", 4 },
            { "Hammerhead", 4 },
            { "Orca", 4 },
            { "Anemone", 3 },
            { "Kraken", 3 },
            { "Moray", 3 },
            { "Net", 3 },
            { "Whaleshark", 3 }
        };

        return BuildDeckFromComposition(cardPrefab, defaultComposition);
    }

    /// <summary>
    /// Builds a tutorial-specific deck with a simpler composition,
    /// ensuring the player can learn with straightforward cards.
    /// </summary>
    public Deck BuildTutorialDeck(GameObject cardPrefab)
    {
        var tutorialComposition = new Dictionary<string, int>
        {
            { "ClownFish", 4 },
            { "Anemone", 2 },
            { "Kraken", 1 }
            // Add or remove cards as needed for tutorial
        };

        return BuildDeckFromComposition(cardPrefab, tutorialComposition);
    }

    /// <summary>
    /// Builds a normal game deck for a given level. You can adjust this logic
    /// based on the level index or difficulty.
    /// For simplicity, here's a composition that changes based on level size.
    /// </summary>
    public Deck BuildNormalLevelDeck(GameObject cardPrefab, int deckSize)
    {
        // Example: For normal levels, distribute cards somewhat evenly.
        // Adjust card types and counts as desired.
        // The total should sum up approximately to deckSize.
        
        var normalComposition = new Dictionary<string, int>
        {
            { "Plankton", 10 },
            { "FishEggs", 8 },
            { "Seahorse", 6 },
            { "ClownFish", 6 },
            { "CookieCutter", 4 },
            { "Anemone", 3 },
            { "Kraken", 3 },
            { "Whaleshark", 2 }
        };

        // If deckSize differs from sum, you can tune or trim randomly.
        // For now, assume deckSize is matched or just trust this composition.
        return BuildDeckFromComposition(cardPrefab, normalComposition);
    }

    /// <summary>
    /// Converts a dictionary of string->count card entries into
    /// CardData->int composition for creating a Deck.
    /// </summary>
    private Deck BuildDeckFromComposition(GameObject cardPrefab, Dictionary<string, int> composition)
    {
        var deckComposition = new Dictionary<CardData, int>();

        foreach (var entry in composition)
        {
            var cardData = CardLibrary.Instance.GetCardDataByName(entry.Key);
            if (cardData != null)
            {
                deckComposition[cardData] = entry.Value;
            }
            else
            {
                Debug.LogWarning($"[DeckBuilder] Card '{entry.Key}' not found in CardLibrary.");
            }
        }

        return new Deck(deckComposition, cardPrefab);
    }
}
