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

        private void Start()
        {
            gameManager = HunterGameManager.Instance;
            handManager = HunterHandManager.Instance;

            playButton.onClick.AddListener(OnPlayButtonClicked);
            drawButton.onClick.AddListener(OnDrawButtonClicked);
            discardButton.onClick.AddListener(OnDiscardButtonClicked);
        }

        public void OnPlayButtonClicked()
        {
            handManager.PlaySelectedCard();
        }

        public void OnDrawButtonClicked()
        {
            gameManager.DrawCard();
        }

        public void OnDiscardButtonClicked()
        {
            handManager.DiscardSelectedCard();
        }
    }
}
