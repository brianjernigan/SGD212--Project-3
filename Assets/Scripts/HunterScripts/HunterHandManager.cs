using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HunterScripts
{
    public class HunterHandManager : MonoBehaviour
    {
        [Header("Card Settings")]
        [SerializeField] private GameObject hunterCardPrefab;
        [SerializeField] private Transform handArea;
        [SerializeField] private float cardSpacing = 0.5f; // Adjust spacing between cards
        [SerializeField] private float curveAngle = 15f; // Angle for fan layout (optional)

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
        }

        public void DrawCardsToHand(int numberOfCards)
        {
            StartCoroutine(DrawCardsCoroutine(numberOfCards));
        }

        private IEnumerator DrawCardsCoroutine(int numberOfCards)
        {
            for (int i = 0; i < numberOfCards; i++)
            {
                HunterDeckManager.Instance?.DrawCard();
                yield return new WaitForSeconds(0.2f);
            }
        }

        public HunterCardUI GetSelectedCard()
        {
            foreach (var card in onScreenCards)
            {
                if (card.IsSelected)
                {
                    return card;
                }
            }
            return null;
        }

        public void PlayCard(HunterCardUI card)
        {
            if (onScreenCards.Contains(card))
            {
                onScreenCards.Remove(card);
                HunterCardEffectManager.Instance?.PlayPlayEffect(card.transform.position);
                Destroy(card.gameObject);
                PositionCardsInHand(); // Reposition remaining cards
            }
        }

        public void DiscardCard(HunterCardUI card)
        {
            if (onScreenCards.Contains(card))
            {
                onScreenCards.Remove(card);
                Destroy(card.gameObject);
                PositionCardsInHand(); // Reposition remaining cards
            }
        }

        public void HandleCardDrawn(HunterCardData drawnCard)
        {
            if (hunterCardPrefab == null || handArea == null)
            {
                Debug.LogError("HunterHandManager: Prefab or hand area not assigned.");
                return;
            }

            GameObject onScreenCard = Instantiate(hunterCardPrefab, handArea);
            onScreenCard.transform.localScale = Vector3.one;

            if (onScreenCard.TryGetComponent(out HunterCardUI cardUI))
            {
                cardUI.InitializeCard(drawnCard);
                onScreenCards.Add(cardUI);
                PositionCardsInHand(); // Arrange cards after adding
            }
            else
            {
                Debug.LogError("HunterHandManager: HunterCardUI component missing on prefab.");
                Destroy(onScreenCard);
            }
        }

        // Method to position cards in a spread-out layout within the hand area
        private void PositionCardsInHand()
        {
            int cardCount = onScreenCards.Count;
            if (cardCount == 0) return; // No cards to position

            float totalWidth = (cardCount - 1) * cardSpacing;

            for (int i = 0; i < cardCount; i++)
            {
                HunterCardUI card = onScreenCards[i];

                // Calculate the x-offset for each card based on its position in the list
                float offsetX = -totalWidth / 2 + i * cardSpacing;
                Vector3 targetPosition = new Vector3(offsetX, 0, 0);

                // Avoid applying rotation if there’s only one card to prevent NaN errors
                Quaternion targetRotation = Quaternion.identity;
                if (cardCount > 1)
                {
                    float angle = -curveAngle / 2 + (curveAngle / (cardCount - 1)) * i;
                    targetRotation = Quaternion.Euler(0, 0, angle);
                }

                // Assign position and rotation to each card
                card.transform.localPosition = targetPosition;
                card.transform.localRotation = targetRotation;
            }
        }

    }
}
