using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string _cardName;
    [SerializeField] private Sprite _cardImage; // Use Sprite instead of Image
    [SerializeField] private int _cardCost;
    [SerializeField] private int _cardRank;

    [Header("3D Model")]
    [SerializeField] private Material _cardMaterial; // For the card plane

    [Header("Card Effect")]
    [SerializeField] private CardEffectType _effectType;

    // Properties
    public string CardName => _cardName;
    public Sprite CardImage => _cardImage;
    public int CardCost => _cardCost;
    public int CardRank => _cardRank;
    public Material CardMaterial => _cardMaterial;
    public CardEffectType EffectType => _effectType;
}

public enum CardEffectType
{
    // Numerical Cards
    Minnow,
    Shrimp,
    Clownfish,
    Bass,
    Cod,
    Pufferfish,
    Salmon,
    Tuna,
    Shark,
    Orca,

    // Special Cards
    TheNet,
    TidePool,
    DeepCurrent,
    StormSurge,
    SunkenTreasure,
    SchoolOfFish
}
