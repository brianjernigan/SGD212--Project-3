using System.Collections;
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

    public Deck BuildDefaultDeck(GameObject cardPrefab)
    {
        var defaultComposition = new Dictionary<string, int>
        {
            { "Plankton", 4 },
            { "FishEggs", 4 },
            { "Seahorse", 4 },
            { "Clownfish", 4 },
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
}
