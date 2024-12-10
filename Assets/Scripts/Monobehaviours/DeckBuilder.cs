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
        var tutorialComposition = new List<string>
        {
            "ClownFish",
            "Anemone",
            "Bullshark",
            "Stingray",
            "Plankton",
            "Plankton",
            "Kraken",
            "Plankton",
            "Plankton",
            "Whaleshark",   
            "Hammerhead",   
            "Stingray",
            "Stingray",
            "FishEggs"
        };

        return BuildDeckFromOrderedList(cardPrefab, tutorialComposition);
    }

    private Deck BuildDeckFromOrderedList(GameObject cardPrefab, List<string> orderedCards)
    {
        var deckCards = new List<CardData>();

        foreach (var cardName in orderedCards)
        {
            var cardData = CardLibrary.Instance.GetCardDataByName(cardName);
            if (cardData is not null)
            {
                deckCards.Add(cardData);
            }
        }

        return new Deck(deckCards, cardPrefab);
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
