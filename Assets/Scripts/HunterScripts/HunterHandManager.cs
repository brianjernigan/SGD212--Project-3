using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterScripts
{
    public class HunterHandManager : MonoBehaviour
    {
        [Header("Card Settings")]
        [SerializeField] private GameObject hunterCardPrefab;
        [SerializeField] private Transform handArea; // Parent object to hold cards in the "hand"
        [SerializeField] private HunterDeckManager hunterDeckManager;
        [SerializeField] private float spawnDelay = 0.2f;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private float cardSpacing = 0.5f; // Horizontal spacing between cards
        [SerializeField] private float curveAngle = 10f; // Angle for the card arc

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

        // Method to add and animate cards in the hand area
        public void AddCardToHand(HunterCardUI card)
        {
            onScreenCards.Add(card);
            PositionCardsInHand();
        }

        // Plays the selected card by removing it from the hand and activating its effects
        public void PlayCard(HunterCardUI card)
        {
            if (onScreenCards.Contains(card))
            {
                onScreenCards.Remove(card);
                PositionCardsInHand(); // Reposition remaining cards
                HunterCardEffectManager.Instance.PlayPlayEffect(card.transform.position);
                Destroy(card.gameObject); // Remove the played card from the scene
            }
        }

        // Gets the currently selected card from the hand
        public HunterCardUI GetSelectedCard()
        {
            foreach (var card in onScreenCards)
            {
                if (card.IsSelected) // Assuming IsSelected property in HunterCardUI
                    return card;
            }
            return null;
        }

        // Discards the selected card by removing it from the hand
        public void DiscardCard(HunterCardUI card)
        {
            if (onScreenCards.Contains(card))
            {
                onScreenCards.Remove(card);
                PositionCardsInHand(); // Reposition remaining cards
                Destroy(card.gameObject); // Remove the discarded card from the scene
            }
        }

        // Positions the cards in a fan layout within the hand area
        private void PositionCardsInHand()
        {
            int cardCount = onScreenCards.Count;
            float totalWidth = (cardCount - 1) * cardSpacing;

            for (int i = 0; i < cardCount; i++)
            {
                HunterCardUI card = onScreenCards[i];
                float offsetX = -totalWidth / 2 + i * cardSpacing;
                float angle = -curveAngle / 2 + (curveAngle / (cardCount - 1)) * i;

                Vector3 targetPosition = new Vector3(offsetX, 0, 0);
                Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

                StartCoroutine(AnimateCardToPosition(card.transform, targetPosition, targetRotation));
            }
        }

        // Smoothly animates each card to its target position and rotation
        private IEnumerator AnimateCardToPosition(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation)
        {
            Vector3 startPosition = cardTransform.localPosition;
            Quaternion startRotation = cardTransform.localRotation;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                cardTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / animationDuration);
                cardTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            cardTransform.localPosition = targetPosition;
            cardTransform.localRotation = targetRotation;
        }
    }
}
