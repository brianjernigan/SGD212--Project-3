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
            // Call to draw multiple cards
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
            }
        }

        public void DiscardCard(HunterCardUI card)
        {
            if (onScreenCards.Contains(card))
            {
                onScreenCards.Remove(card);
                Destroy(card.gameObject);
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
            if (onScreenCard.TryGetComponent(out HunterCardUI cardUI))
            {
                cardUI.InitializeCard(drawnCard);
                onScreenCards.Add(cardUI);
            }
            else
            {
                Debug.LogError("HunterHandManager: HunterCardUI component missing on prefab.");
                Destroy(onScreenCard);
            }
        }
    }
}
