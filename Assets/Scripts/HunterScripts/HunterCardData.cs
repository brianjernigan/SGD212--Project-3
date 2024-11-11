using UnityEngine;

namespace HunterScripts
{
    [CreateAssetMenu(fileName = "New Hunter Card", menuName = "Hunter/Hunter Card")]
    public class HunterCardData : ScriptableObject
    {
        public string cardName;
        public Material cardMaterial;
        public int cardCost;
        public int cardRank;

        // If CardEffect is not defined, comment out or remove this line
        // public CardEffect effect;
    }
}
