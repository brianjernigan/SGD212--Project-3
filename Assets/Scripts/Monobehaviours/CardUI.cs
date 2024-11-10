using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject _cardObject;
    private CardData _cardData;

    private Image _cardImage;
    
    private TMP_Text _cardRankText;
    private TMP_Text _cardCostText;
    
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Transform _originalParent;

    private RectTransform _handArea;
    private RectTransform _stageArea;
    private RectTransform _discardArea;
    
    public void InitializeCard(CardData cardData, GameObject cardObject)
    {
        _cardData = cardData;
        _cardObject = cardObject;
        SetCardUI();
    }

    private void SetCardUI()
    {
        _cardRankText = _cardObject.transform.GetChild(0).GetComponent<TMP_Text>();
        _cardCostText = _cardObject.transform.GetChild(1).GetComponent<TMP_Text>();
        _cardImage = _cardObject.transform.GetChild(2).GetComponent<Image>();

        _cardRankText.text = _cardData.CardRank.ToString();
        _cardCostText.text = _cardData.CardCost.ToString();
        _cardImage.sprite = _cardData.CardSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = transform.position;
        _originalParent = transform.parent;
        _originalScale = transform.localScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }
}
