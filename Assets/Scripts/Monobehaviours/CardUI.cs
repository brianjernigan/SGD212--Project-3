using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private TMP_Text _cardRankText;
    [SerializeField] private TMP_Text _cardCostText;
    
    private RectTransform _stageAreaRectTransform;
    private RectTransform _discardAreaRectTransform;
    private RectTransform _handAreaRectTransform;
    
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Transform _originalParent;
    private Canvas _canvas;

    private HandManager _handManager;
    private CardData _cardData;
    private StageAreaController _stageAreaController;

    private void Start()
    {
        var stageArea = GameObject.FindGameObjectWithTag("StageArea");
        
        _stageAreaRectTransform = stageArea.GetComponent<RectTransform>();
        _handAreaRectTransform = GameObject.FindGameObjectWithTag("HandArea").GetComponent<RectTransform>();
        _discardAreaRectTransform = GameObject.FindGameObjectWithTag("DiscardArea").GetComponent<RectTransform>();
        
        _stageAreaController = stageArea.GetComponent<StageAreaController>();
        
        _canvas = GetComponentInParent<Canvas>();
        _originalParent = transform.parent;
        _originalScale = transform.localScale;
        // _animator = GetComponent<Animator>();
    }
    
    public void InitializeCard(CardData data, HandManager handManager)
    {
        _cardData = data;
        _handManager = handManager;
        _cardImage = data.CardImage;
        //_cardImage.sprite = data.CardImage.sprite; // Assuming CardImage is a Sprite
        _cardRankText.text = data.CardRank.ToString();
        _cardCostText.text = data.CardCost.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        _originalPosition = transform.position;
        transform.SetParent(_canvas.transform);
        transform.localScale *= 1.1f; // Slightly enlarge the card when dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var isInStageArea =
            RectTransformUtility.RectangleContainsScreenPoint(_stageAreaRectTransform, Input.mousePosition,
                _canvas.worldCamera);

        var isInDiscardArea = RectTransformUtility.RectangleContainsScreenPoint(_discardAreaRectTransform,
            Input.mousePosition, _canvas.worldCamera);
        
        var isInHandArea = RectTransformUtility.RectangleContainsScreenPoint(_handAreaRectTransform,
            Input.mousePosition, _canvas.worldCamera);
        
        if (_stageAreaRectTransform is not null && isInStageArea)
        {
            StageCard(_cardData);
        }
        else if (_discardAreaRectTransform is not null && isInDiscardArea)
        {
            DiscardCard(_cardData);
        } 
        else if (_handAreaRectTransform is not null & isInHandArea)
        {
            ReturnCardToHand(_cardData);
        }
        else
        {
            ReturnCardToOrigin(_cardData);
        }
    }

    private void DiscardCard(CardData card)
    {
        _handManager.DiscardCardFromHand(card);
        _handManager.DrawCard();
        Destroy(gameObject);
    }

    private void StageCard(CardData card)
    {
        if (_stageAreaController.AddCardToStageArea(card))
        {
            transform.SetParent(_stageAreaRectTransform);
            transform.localScale = _originalScale;
        }
        else
        {
            ReturnCardToOrigin(card);
        }
        // Trigger play animation via Animator
        // _animator.SetTrigger("OnPlay");
        // Play particle effect
        // CardEffectManager.Instance.PlayPlayEffect(transform.position);
    }

    private void ReturnCardToHand(CardData card)
    {
        // Check for cards
        transform.SetParent(_handAreaRectTransform);
        transform.localScale = _originalScale;
    }

    private void ReturnCardToOrigin(CardData card)
    {
        transform.localScale = _originalScale;
        transform.position = _originalPosition;
        transform.SetParent(_originalParent);
    }

    public void OnCardDrawn()
    {
        // Trigger draw animation via Animator
        // _animator.SetTrigger("OnDraw");
        // Play draw particle effect
        // CardEffectManager.Instance.PlayDrawEffect(transform.position);
    }
}
