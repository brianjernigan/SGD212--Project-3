using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HunterScripts
{
    public class HunterHandManager : MonoBehaviour
    {
        [Header("Card Settings")]
        [SerializeField] private GameObject hunterCardPrefab;
        [SerializeField] private RectTransform handArea; // Should have Horizontal Layout Group
        [SerializeField] private HunterDeckManager hunterDeckManager;
        [SerializeField] private float spawnDelay = 0.2f; // Delay between spawns
        [SerializeField] private float animationDuration = 0.3f; // Duration for entrance animation

        private HunterHand playerHand;
        public HunterHand PlayerHand => playerHand;

        private HunterDeck currentDeck;
        private List<HunterCardUI> onScreenCards = new List<HunterCardUI>();

        public static HunterHandManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            if (hunterDeckManager == null)
            {
                Debug.LogError("HunterHandManager: 'hunterDeckManager' is not assigned in the Inspector.");
            }

            currentDeck = hunterDeckManager.CurrentDeck;
            playerHand = new HunterHand(currentDeck);
        }

        private void OnEnable()
        {
            if (hunterDeckManager != null)
            {
                hunterDeckManager.OnCardDrawn += HandleCardDrawn;
            }
            else
            {
                Debug.LogError("HunterHandManager: 'hunterDeckManager' is null in OnEnable.");
            }
        }

        private void OnDisable()
        {
            if (hunterDeckManager != null)
            {
                hunterDeckManager.OnCardDrawn -= HandleCardDrawn;
            }
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
            if (drawnCard == null)
            {
                Debug.LogWarning("HandleCardDrawn: 'drawnCard' is null.");
                return;
            }

            if (!playerHand.AddCard(drawnCard))
            {
                Debug.LogWarning("HandleCardDrawn: Player's hand is full. Cannot add more cards.");
                return;
            }

            // Instantiate the card prefab as a child of the hand area
            GameObject onScreenCard = Instantiate(hunterCardPrefab, handArea);
            HunterCardUI cardUI = onScreenCard.GetComponent<HunterCardUI>();

            if (cardUI == null)
            {
                Debug.LogError("HandleCardDrawn: 'HunterCardUI' component is missing on the instantiated card prefab.");
                Destroy(onScreenCard);
                return;
            }

            // Initialize the card with its data
            cardUI.InitializeCard(drawnCard);
            onScreenCards.Add(cardUI);

            // Play draw effect
            if (HunterCardEffectManager.Instance != null)
            {
                HunterCardEffectManager.Instance.PlayDrawEffect(onScreenCard.transform.position);
            }
            else
            {
                Debug.LogError("HandleCardDrawn: 'HunterCardEffectManager.Instance' is null.");
            }

            // Start the entrance animation
            StartCoroutine(AnimateCardEntrance(onScreenCard.transform));
        }

        private IEnumerator AnimateCardEntrance(Transform cardTransform)
        {
            // Store original scale
            Vector3 targetScale = cardTransform.localScale;
            cardTransform.localScale = Vector3.zero;

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                cardTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cardTransform.localScale = targetScale;
        }

        public void PlayCard(HunterCardUI cardUI)
        {
            if (onScreenCards.Contains(cardUI))
            {
                onScreenCards.Remove(cardUI);
            }

            playerHand.RemoveCard(cardUI.CardData);

            // Activate card effect if any (implementation depends on your game logic)
            // Example:
            // cardUI.CardData.effect.ActivateEffect();

            // Play play effect
            if (HunterCardEffectManager.Instance != null)
            {
                HunterCardEffectManager.Instance.PlayPlayEffect(cardUI.transform.position);
            }
            else
            {
                Debug.LogError("PlayCard: 'HunterCardEffectManager.Instance' is null.");
            }

            // Start play card animation (optional)
            StartCoroutine(AnimateCardPlay(cardUI.transform));

            Destroy(cardUI.gameObject);
        }

        private IEnumerator AnimateCardPlay(Transform cardTransform)
        {
            Vector3 initialScale = cardTransform.localScale;
            Vector3 targetScale = initialScale * 1.2f; // Example: Scale up slightly
            float halfDuration = animationDuration / 2f;
            float elapsed = 0f;

            // Scale up
            while (elapsed < halfDuration)
            {
                cardTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cardTransform.localScale = targetScale;
            elapsed = 0f;

            // Scale down to zero
            while (elapsed < halfDuration)
            {
                cardTransform.localScale = Vector3.Lerp(targetScale, Vector3.zero, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cardTransform.localScale = Vector3.zero;
        }

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
                Debug.Log("PlaySelectedCard: No card selected to play.");
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
                Debug.Log("DiscardSelectedCard: No card selected to discard.");
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
            hunterDeckManager.DiscardFromHand(cardUI.CardData);

            // Play discard effect if any (implementation depends on your game logic)
            // Example:
            // HunterCardEffectManager.Instance.PlayDiscardEffect(cardUI.transform.position);

            // Start discard animation (optional)
            StartCoroutine(AnimateCardDiscard(cardUI.transform));

            Destroy(cardUI.gameObject);
        }

        private IEnumerator AnimateCardDiscard(Transform cardTransform)
        {
            Vector3 initialScale = cardTransform.localScale;
            Vector3 targetScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                cardTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cardTransform.localScale = targetScale;
        }
    }
}
