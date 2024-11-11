using UnityEngine;
using System.Collections.Generic; // For List<>
using UnityEngine.UI; // For Button and other UI elements

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
            // Initialize the deck at the start of the game
            if (hunterDeckManager != null)
            {
                hunterDeckManager.InitializeDeck(allPossibleCards); // Pass allPossibleCards as an argument
                hunterDeckManager.OnCardDrawn += hunterHandManager.HandleCardDrawn; // Subscribe to card drawn event
            }
            else
            {
                Debug.LogError("HunterGameManager: HunterDeckManager is not assigned.");
            }

            DrawFullHand(); // Start the game with a full hand
        }

        private void DrawFullHand()
        {
            for (int i = 0; i < 5; i++)
            {
                hunterDeckManager.DrawCard();
            }
        }

        private void OnDestroy()
        {
            if (hunterDeckManager != null)
            {
                hunterDeckManager.OnCardDrawn -= hunterHandManager.HandleCardDrawn; // Unsubscribe on destroy
            }
        }
    }
}
