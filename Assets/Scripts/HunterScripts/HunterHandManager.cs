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
        [SerializeField] private float cardSpacing = 2f; // Base spacing between cards
        [SerializeField] private float maxCurveAngle = 45f; // Maximum angle for fan layout
        [SerializeField] private float radius = 5f; // Radius of the arc for card positioning

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

        // Improved method to position cards in an arc to prevent overlapping
        private void PositionCardsInHand()
        {
            int cardCount = onScreenCards.Count;
            if (cardCount == 0) return; // No cards to position

            // Clamp the curve angle based on the number of cards
            float curveAngle = Mathf.Clamp(maxCurveAngle / (cardCount > 1 ? cardCount - 1 : 1), 15f, maxCurveAngle);

            // Calculate the total angle span
            float totalAngle = Mathf.Min(maxCurveAngle, (cardCount - 1) * curveAngle);

            // Starting angle
            float startAngle = -totalAngle / 2;

            for (int i = 0; i < cardCount; i++)
            {
                HunterCardUI card = onScreenCards[i];

                // Calculate the angle for the current card
                float angle = startAngle + i * curveAngle;
                float rad = angle * Mathf.Deg2Rad;

                // Calculate the position along the arc
                Vector3 targetPosition = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0);

                // Assign position to each card
                card.transform.localPosition = targetPosition;

                // Assign rotation to make the card face the center of the arc
                card.transform.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        public void DeselectOtherCards(HunterCardUI selectedCard)
        {
            foreach (var card in onScreenCards)
            {
                if (card != selectedCard && card.IsSelected)
                {
                    card.ToggleSelection(); // This will trigger the card to return to its original position
                }
            }
        }

    }

}
