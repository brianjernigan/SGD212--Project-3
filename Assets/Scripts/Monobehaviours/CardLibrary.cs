using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLibrary : MonoBehaviour
{
    public static CardLibrary Instance { get; private set; }

    [SerializeField] private List<CardData> _allPossibleCards;
    public List<CardData> AllPossibleCards => _allPossibleCards;

    private Dictionary<string, CardData> _cardDataByName;
    private Dictionary<string, ICardEffect> _cardEffectByName;

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
        
        InitializeCardLibrary();
    }

    public void InitializeCardLibrary()
    {
        _cardDataByName = new Dictionary<string, CardData>();

        foreach (var card in _allPossibleCards)
        {
            _cardDataByName.TryAdd(card.CardName, card);
        }

        _cardEffectByName = new Dictionary<string, ICardEffect>
        {
            { "Plankton", new PlanktonEffect() },
            { "FishEggs", new FishEggsEffect() },
            { "Seahorse", new SeahorseEffect() },
            { "ClownFish", new ClownFishEffect() },
            { "CookieCutter", new CookieCutterEffect() },
            { "Turtle", new TurtleEffect() },
            { "Stingray", new StingrayEffect() },
            { "Bullshark", new BullsharkEffect() },
            { "Hammerhead", new HammerheadEffect() },
            { "Orca", new OrcaEffect() },
            { "Anemone", new AnemoneEffect() },
            { "Kraken", new KrakenEffect() },
            { "Moray", new MorayEffect() },
            { "Net", new NetEffect() },
            { "Whaleshark", new WhaleSharkEffect() }
        };
    }

    public CardData GetCardDataByName(string cardName)
    {
        return _cardDataByName.GetValueOrDefault(cardName);
    }

    public ICardEffect GetCardEffectByName(string cardName)
    {
        return _cardEffectByName.GetValueOrDefault(cardName);
    }
}
