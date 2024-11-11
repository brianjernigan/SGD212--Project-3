using UnityEngine;
using System.Collections.Generic; // For List<>
using UnityEngine.UI; // For Button and other UI elements


namespace HunterScripts
{
    public class HunterUIManager : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button drawButton;
        [SerializeField] private Button discardButton;

        private HunterHandManager handManager;
        private HunterDeckManager deckManager;

        private void Start()
        {
            // Find the instances of hand and deck managers
            handManager = HunterHandManager.Instance;
            if (handManager == null)
            {
                Debug.LogError("HunterUIManager: HunterHandManager instance not found.");
            }

            deckManager = HunterDeckManager.Instance;
            if (deckManager == null)
            {
                Debug.LogError("HunterUIManager: HunterDeckManager instance not found.");
            }

            // Assign button listeners
            playButton.onClick.AddListener(OnPlayButtonClicked);
            drawButton.onClick.AddListener(OnDrawButtonClicked);
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }

        private void OnDrawButtonClicked()
        {
            if (deckManager != null)
            {
                handManager.DrawCardsToHand(1); // Draws one card at a time
            }
            else
            {
                Debug.LogWarning("OnDrawButtonClicked: DeckManager is not available.");
            }
        }

        private void OnPlayButtonClicked()
        {
            if (handManager != null)
            {
                HunterCardUI selectedCard = handManager.GetSelectedCard();
                if (selectedCard != null)
                {
                    handManager.PlayCard(selectedCard);
                }
                else
                {
                    Debug.Log("OnPlayButtonClicked: No card selected to play.");
                }
            }
        }

        // Discard selected card
        private void OnDiscardButtonClicked()
        {
            if (handManager != null)
            {
                HunterCardUI selectedCard = handManager.GetSelectedCard();
                if (selectedCard != null)
                {
                    handManager.DiscardCard(selectedCard);
                }
                else
                {
                    Debug.Log("OnDiscardButtonClicked: No card selected to discard.");
                }
            }
        }
    }
}
