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
    
    private RectTransform _playArea;
    private Vector3 _originalPosition;
    private Transform _originalParent;
    private Canvas _canvas;

    private void Start()
    {
        _playArea = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _originalParent = transform.parent;
    }
    
    public void InitializeCard(CardData data)
    {
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
        if (_playArea is not null &&
            RectTransformUtility.RectangleContainsScreenPoint(_playArea, Input.mousePosition, _canvas.worldCamera))
        {
            PlayCard();
        }
        else
        {
            transform.position = _originalPosition;
            transform.SetParent(_originalParent);
        }
    }

    private void PlayCard()
    {
        transform.SetParent(_playArea);
    }
}
