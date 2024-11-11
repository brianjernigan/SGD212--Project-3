using UnityEngine;
using UnityEngine.UI;

namespace HunterScripts
{
    public class HunterUIManager : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button drawButton;
        [SerializeField] private Button discardButton;

        private HunterGameManager gameManager;
        private HunterHandManager handManager;
        private HunterDeckManager deckManager;

        private void Start()
        {
            // Initialize references
            gameManager = HunterGameManager.Instance;
            handManager = HunterHandManager.Instance; // Ensure HunterHandManager uses Singleton
            deckManager = HunterDeckManager.Instance;

            // Optional: Assign button listeners via script
            playButton.onClick.AddListener(OnPlayButtonClicked);
             drawButton.onClick.AddListener(OnDrawButtonClicked);
             discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }

        // Public methods to be linked to buttons
        public void OnPlayButtonClicked()
        {
            handManager.PlaySelectedCard();
        }

        public void OnDrawButtonClicked()
        {
            deckManager.DrawCard();
        }

        public void OnDiscardButtonClicked()
        {
            handManager.DiscardSelectedCard();
        }
    }
}
