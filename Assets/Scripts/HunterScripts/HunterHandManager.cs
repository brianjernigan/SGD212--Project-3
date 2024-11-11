using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterScripts
{
    public class HunterHandManager : MonoBehaviour
    {
        [SerializeField] private GameObject hunterCardPrefab;
        [SerializeField] private RectTransform handArea; // Should have Horizontal Layout Group
        [SerializeField] private HunterDeckManager hunterDeckManager;
        [SerializeField] private float spawnDelay = 0.2f; // Delay between spawns

        private HunterHand playerHand;
        public HunterHand PlayerHand => playerHand;

        private HunterDeck currentDeck;
        private List<HunterCardUI> onScreenCards = new List<HunterCardUI>();

        // Singleton Pattern
        public static HunterHandManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            currentDeck = hunterDeckManager.CurrentDeck;
            playerHand = new HunterHand(currentDeck);
        }

        private void OnEnable()
        {
            hunterDeckManager.OnCardDrawn += HandleCardDrawn;
        }

        private void OnDisable()
        {
            hunterDeckManager.OnCardDrawn -= HandleCardDrawn;
        }

        public void DrawCardsToHand(int numberOfCards)
        {
            StartCoroutine(DrawCardsCoroutine(numberOfCards));
        }

        private IEnumerator DrawCardsCoroutine(int numberOfCards)
        {
            for (int i = 0; i < numberOfCards; i++)
            {
                hunterDeckManager.DrawCard();
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void HandleCardDrawn(HunterCardData drawnCard)
        {
            if (drawnCard == null) return;
            if (!playerHand.AddCard(drawnCard)) return;

            // Instantiate the card prefab as a child of the hand area
            var onScreenCard = Instantiate(hunterCardPrefab, handArea);
            var cardUI = onScreenCard.GetComponent<HunterCardUI>();
            if (cardUI != null)
            {
                cardUI.InitializeCard(drawnCard);
                onScreenCards.Add(cardUI);

                // Optional: Play a draw effect at the card's position
                HunterCardEffectManager.Instance.PlayDrawEffect(onScreenCard.transform.position);

                // Optional: Animate card entrance
                StartCoroutine(AnimateCardEntrance(cardUI));
            }
            else
            {
                Debug.LogError("HunterCardUI component is missing on the card prefab.");
            }
        }

        private IEnumerator AnimateCardEntrance(HunterCardUI cardUI)
        {
            // Optional: Animate scaling from 0 to 1 for a pop-in effect
            Vector3 originalScale = cardUI.transform.localScale;
            cardUI.transform.localScale = Vector3.zero;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                cardUI.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cardUI.transform.localScale = originalScale;

            // Optionally, wait for a short delay before allowing the next card to spawn
            yield return new WaitForSeconds(spawnDelay);
        }

        public void PlayCard(HunterCardUI cardUI)
        {
            if (onScreenCards.Contains(cardUI))
            {
                onScreenCards.Remove(cardUI);
            }

            playerHand.RemoveCard(cardUI.CardData);

            // Activate card effect
            // Ensure that 'effect' is properly defined and accessible in HunterCardData
            if (cardUI.CardData.effectType != 0) // Replace with your actual effect activation logic
            {
                // Example: ActivateEffect method based on effectType
                // cardUI.CardData.effect.ActivateEffect(HunterGameManager.Instance);
                // Replace the above line with your actual effect activation code
            }

            // Optionally, trigger animations or particle effects here

            Destroy(cardUI.gameObject);
        }

        // New Methods for UI Manager

        public void PlaySelectedCard()
        {
            // Retrieve the selected card
            HunterCardUI selectedCard = GetSelectedCard();
            if (selectedCard != null)
            {
                PlayCard(selectedCard);
            }
            else
            {
                Debug.Log("No card selected to play.");
            }
        }

        public void DiscardSelectedCard()
        {
            // Retrieve the selected card
            HunterCardUI selectedCard = GetSelectedCard();
            if (selectedCard != null)
            {
                DiscardCard(selectedCard);
            }
            else
            {
                Debug.Log("No card selected to discard.");
            }
        }

        private HunterCardUI GetSelectedCard()
        {
            foreach (var card in onScreenCards)
            {
                if (card.IsSelected)
                    return card;
            }
            return null;
        }

        private void DiscardCard(HunterCardUI cardUI)
        {
            if (onScreenCards.Contains(cardUI))
            {
                onScreenCards.Remove(cardUI);
            }

            playerHand.RemoveCard(cardUI.CardData);

            // Add to discard pile
            hunterDeckManager.DiscardFromHand(cardUI.CardData);

            // Optionally, trigger animations or particle effects here

            Destroy(cardUI.gameObject);
        }
    }
}
