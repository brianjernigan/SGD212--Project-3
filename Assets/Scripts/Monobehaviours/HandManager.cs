using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private RectTransform handArea; // Should have Horizontal Layout Group
    [SerializeField] private float spawnDelay = 0.2f;

    private Hand playerHand;
    private List<CardUI> onScreenCards = new List<CardUI>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerHand = new Hand(DeckManager.Instance.CurrentDeck);
    }

    private void OnEnable()
    {
        DeckManager.Instance.OnCardDrawn += HandleCardDrawn;
    }

    private void OnDisable()
    {
        DeckManager.Instance.OnCardDrawn -= HandleCardDrawn;
    }

    public void DrawCardsToHand(int numberOfCards)
    {
        StartCoroutine(DrawCardsCoroutine(numberOfCards));
    }

    private IEnumerator DrawCardsCoroutine(int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            DeckManager.Instance.DrawCard();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void HandleCardDrawn(CardData drawnCard)
    {
        if (drawnCard == null) return;
        if (!playerHand.AddCardToHand(drawnCard)) return;

        var onScreenCard = Instantiate(cardPrefab, handArea);
        var cardUI = onScreenCard.GetComponent<CardUI>();
        cardUI.InitializeCard(drawnCard);
        onScreenCards.Add(cardUI);
    }

    public void PlayCard(CardUI cardUI)
    {
        if (onScreenCards.Contains(cardUI))
        {
            onScreenCards.Remove(cardUI);
            playerHand.DiscardCardFromHand(cardUI.CardData);
            DeckManager.Instance.DiscardCard(cardUI.CardData);
            Destroy(cardUI.gameObject);
        }
    }

    // Additional methods for selecting and discarding cards
    public void DiscardSelectedCard()
    {
        var selectedCard = GetSelectedCard();
        if (selectedCard != null)
        {
            PlayCard(selectedCard);
        }
        else
        {
            Debug.Log("No card selected to discard.");
        }
    }

    private CardUI GetSelectedCard()
    {
        foreach (var card in onScreenCards)
        {
            if (card.IsSelected)
                return card;
        }
        return null;
    }
}
