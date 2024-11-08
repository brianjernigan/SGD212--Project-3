using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Handles visual representation of card
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private TMP_Text _cardRankText;
    [SerializeField] private TMP_Text _cardCostText;
    
    private RectTransform _playAreaRectTransform;
    private RectTransform _discardAreaRectTransform;
    private Vector3 _originalPosition;
    private Transform _originalParent;
    private Canvas _canvas;

    private HandManager _handManager;
    private CardData _cardData;
    private PlayAreaController _playAreaController;

    private void Start()
    {
        _playAreaRectTransform = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<RectTransform>();
        _playAreaController = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<PlayAreaController>();
        _discardAreaRectTransform = GameObject.FindGameObjectWithTag("DiscardArea").GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _originalParent = transform.parent;
    }
    
    public void InitializeCard(CardData data, HandManager handManager)
    {
        _cardData = data;
        _handManager = handManager;
        
        _cardImage = data.CardImage;
        _cardRankText.text = data.CardRank.ToString();
        _cardCostText.text = data.CardCost.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = transform.position;
        transform.SetParent(_canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_playAreaRectTransform is not null &&
            RectTransformUtility.RectangleContainsScreenPoint(_playAreaRectTransform, Input.mousePosition, _canvas.worldCamera))
        {
            PlayCard();
        }
        else if (_discardAreaRectTransform is not null &&
            RectTransformUtility.RectangleContainsScreenPoint(_discardAreaRectTransform, Input.mousePosition, _canvas.worldCamera))
        {
            DiscardCard(_cardData);
        }
        else
        {
            transform.position = _originalPosition;
            transform.SetParent(_originalParent);
        }
    }

    private void DiscardCard(CardData data)
    {
        _handManager.DiscardCardFromHand(data);
        Destroy(gameObject);
    }

    private void PlayCard()
    {
        transform.SetParent(_playAreaRectTransform);
        _playAreaController.AddCardToPlayArea(_cardData);
    }
}
