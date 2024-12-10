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
    
    public Deck BuildTutorialDeck(GameObject cardPrefab)
    {
        var tutorialComposition = new Dictionary<string, int>
        {
            { "ClownFish", 4 },
            { "Anemone", 2 },
            { "Kraken", 1 }
            // Add or remove cards as needed for the tutorial
        };

        return BuildDeckFromComposition(cardPrefab, tutorialComposition);
    }
    
public Deck BuildNormalLevelDeck(GameObject cardPrefab, int deckSize)
{
    // Example: A varied deck composition that sums up to deckSize
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

    // Scaling logic...
    // Ensure total counts match deckSize


        // Adjust counts proportionally if deckSize differs
        // For simplicity, we'll scale the counts based on deckSize
        // You can implement more sophisticated logic as needed

        float totalBaseCards = 42f;
        float scaleFactor = deckSize / totalBaseCards;

        var scaledComposition = new Dictionary<string, int>();
        foreach (var entry in normalComposition)
        {
            scaledComposition[entry.Key] = Mathf.Max(1, Mathf.RoundToInt(entry.Value * scaleFactor));
        }

        // Optional: Ensure the total cards match deckSize by adjusting the counts
        int currentTotal = 0;
        foreach (var count in scaledComposition.Values)
        {
            currentTotal += count;
        }

        while (currentTotal < deckSize)
        {
            // Add one to the first card to reach deckSize
            foreach (var key in scaledComposition.Keys)
            {
                scaledComposition[key]++;
                currentTotal++;
                if (currentTotal >= deckSize) break;
            }
        }

        while (currentTotal > deckSize)
        {
            // Subtract one from the last card to reach deckSize
            foreach (var key in scaledComposition.Keys)
            {
                if (scaledComposition[key] > 1)
                {
                    scaledComposition[key]--;
                    currentTotal--;
                    if (currentTotal <= deckSize) break;
                }
            }
        }

        return BuildDeckFromComposition(cardPrefab, scaledComposition);
    }

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
        }

        // Shuffle the deck to ensure randomness
        var shuffledDeck = ShuffleDeck(deckComposition);

        return new Deck(shuffledDeck, cardPrefab);
    }

    
    private Dictionary<CardData, int> ShuffleDeck(Dictionary<CardData, int> deckComposition)
    {
        var shuffledDeck = new Dictionary<CardData, int>();

        return deckComposition;
    }
}
