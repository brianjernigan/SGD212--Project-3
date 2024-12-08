using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    public static DeckBuilder Instance { get; private set; }

    private void Awake()
    {
        if (Instance is null)
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
    /// Builds the default deck with all possible cards.
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

        var deckComposition = new Dictionary<CardData, int>();
        foreach (var entry in defaultComposition)
        {
            var cardData = CardLibrary.Instance.GetCardDataByName(entry.Key);
            if (cardData is not null)
            {
                deckComposition[cardData] = entry.Value;
            }
        }

        return new Deck(deckComposition, cardPrefab);
    }

    /// <summary>
    /// Builds a tutorial-specific deck containing only selected cards.
    /// </summary>
    public Deck BuildTutorialDeck(GameObject cardPrefab)
    {
        var tutorialComposition = new Dictionary<string, int>
        {
            { "ClownFish", 2 },
            { "Anemone", 2 },
            { "Whaleshark", 1 },
            { "Kraken", 1 },
            { "Plankton", 2 },
            { "Seahorse", 1 }
            // Add other tutorial-specific cards as needed
        };

        var deckComposition = new Dictionary<CardData, int>();
        foreach (var entry in tutorialComposition)
        {
            var cardData = CardLibrary.Instance.GetCardDataByName(entry.Key);
            if (cardData is not null)
            {
                deckComposition[cardData] = entry.Value;
            }
            else
            {
                Debug.LogWarning($"[DeckBuilder] Tutorial card '{entry.Key}' not found in CardLibrary.");
            }
        }

        // Optional: Shuffle the tutorial deck for randomness
        // If you prefer a fixed order for the tutorial, you can skip shuffling
        return new Deck(deckComposition, cardPrefab);
    }
}
