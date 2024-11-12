using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private GameObject _cardObject;
    private CardData _cardData;
    private GameCard _gameCard;
    private GameObject _descriptionBox;

    private Image _cardImage;
    private Image _descriptionBoxImage;
    
    private TMP_Text _cardRankText;
    private TMP_Text _cardCostText;
    private TMP_Text _descriptionText;
    
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Transform _originalParent;
    private RectTransform _originalArea;
    private int _originalSiblingIndex;
    
    public void InitializeCard(CardData cardData, GameCard gameCard)
    {
        _cardData = cardData;
        _gameCard = gameCard;
        _cardObject = gameObject;
        SetCardUI();
    }

    private void SetCardUI()
    {
        _cardRankText = _cardObject.transform.GetChild(0).GetComponent<TMP_Text>();
        _cardCostText = _cardObject.transform.GetChild(1).GetComponent<TMP_Text>();
        _descriptionBox = _cardObject.transform.GetChild(2).gameObject;
        _descriptionText = _descriptionBox.GetComponentInChildren<TMP_Text>();
        _cardImage = _cardObject.GetComponent<Image>();
        _descriptionBoxImage = _descriptionBox.GetComponent<Image>();

        _cardRankText.text = _cardData.CardRank.ToString();
        _cardCostText.text = _cardData.CardCost.ToString();
        _descriptionText.text = _gameCard.Description;

        if (_cardData.CardSprite is not null)
        {
            _cardImage.sprite = _cardData.CardSprite;
        }

        if (_cardData.DescriptionBoxSprite is not null)
        {
            _descriptionBoxImage.sprite = _cardData.DescriptionBoxSprite;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = transform.position;
        _originalParent = transform.parent;
        _originalScale = transform.localScale;
        _originalArea = GetCurrentArea(eventData);
        _originalSiblingIndex = transform.GetSiblingIndex();

        transform.localScale *= 1.1f;
        GameManager.Instance.GameCanvasGroup.alpha = 0.9f;

        transform.SetParent(GameManager.Instance.GameCanvas.transform);
        _descriptionBox.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        
        Debug.Log(GetCurrentArea(eventData));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var dropArea = GetCurrentArea(eventData);
        
        if (dropArea != _originalArea && dropArea is not null)
        {
            if (GameManager.Instance.OnCardDropped(dropArea, _gameCard))
            {
                transform.SetParent(dropArea);
                transform.localScale = _originalScale;
            }
            else
            {
                ReturnToOrigin();
            }
        }
        else
        {
            ReturnToOrigin();
        }
        
        GameManager.Instance.GameCanvasGroup.alpha = 1.0f;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _descriptionBox.gameObject.SetActive(!_descriptionBox.gameObject.activeSelf);
        }
    }

    private void ReturnToOrigin()
    {
        transform.SetParent(_originalParent);
        transform.localScale = _originalScale;
        transform.position = _originalPosition;
        transform.SetSiblingIndex(_originalSiblingIndex);
    }

    private RectTransform GetCurrentArea(PointerEventData eventData)
    {
        var dropAreas = new List<RectTransform>()
        {
            GameManager.Instance.HandArea,
            GameManager.Instance.StageArea,
            GameManager.Instance.DiscardArea,
            GameManager.Instance.GamePanel
        };

        foreach (var area in dropAreas)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(area, Input.mousePosition, GameManager.Instance.GameCanvas.worldCamera))
            {
                return area; 
            }
        }

        return null;
    }
}
