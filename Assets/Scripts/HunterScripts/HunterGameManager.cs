using UnityEngine;
using System.Collections.Generic;

namespace HunterScripts
{
    public class HunterGameManager : MonoBehaviour
    {
        public static HunterGameManager Instance { get; private set; }

        [Header("Game Data")]
        public List<HunterCardData> allPossibleCards;
        public List<HunterCardData> AllPossibleCards => allPossibleCards;

        [Header("Managers")]
        [SerializeField] private HunterDeckManager hunterDeckManager;
        [SerializeField] private HunterHandManager hunterHandManager;

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

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            hunterDeckManager.InitializeDeck(allPossibleCards);
            hunterHandManager.DrawCardsToHand(5);
        }

        public void EndGame()
        {
            // Implement end game logic here
        }

        public void DrawCard()
        {
            hunterDeckManager.DrawCard();
        }

        public void DiscardFromHand(HunterCardData card = null)
        {
            hunterDeckManager.DiscardFromHand(card);
        }
    }
}
