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

        var deck = BuildDeckFromComposition(cardPrefab, defaultComposition);
        
        deck.ShuffleDeck();

        return deck;
    }
    
    public Deck BuildTutorialDeck(GameObject cardPrefab)
    {
        var tutorialComposition = new Dictionary<string, int>
        {
            { "ClownFish", 3 },
            { "Anemone", 1 },
            { "Kraken", 1 }
        };

        return BuildDeckFromComposition(cardPrefab, tutorialComposition);
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

        return new Deck(deckComposition, cardPrefab);
    }
}
