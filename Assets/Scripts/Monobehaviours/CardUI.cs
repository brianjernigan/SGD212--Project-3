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
    
    private RectTransform _playArea;
    private Vector3 _originalPosition;
    private Transform _originalParent;
    private Canvas _canvas;
    private Animator _animator;

    private void Start()
    {
        _playArea = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _originalParent = transform.parent;
        _animator = GetComponent<Animator>();
    }
    
    public void InitializeCard(CardData data)
    {
        _cardImage.sprite = data.CardImage.sprite; // Assuming CardImage is a Sprite
        _cardRankText.text = data.CardRank.ToString();
        _cardCostText.text = data.CardCost.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = transform.position;
        transform.SetParent(_canvas.transform);
        transform.localScale = Vector3.one * 1.1f; // Slightly enlarge the card when dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localScale = Vector3.one; // Reset scale
        if (_playArea != null &&
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
        // Trigger play animation via Animator
        _animator.SetTrigger("OnPlay");
        // Play play particle effect
        CardEffectManager.Instance.PlayPlayEffect(transform.position);
    }

    public void OnCardDrawn()
    {
        // Trigger draw animation via Animator
        _animator.SetTrigger("OnDraw");
        // Play draw particle effect
        CardEffectManager.Instance.PlayDrawEffect(transform.position);
    }
}
