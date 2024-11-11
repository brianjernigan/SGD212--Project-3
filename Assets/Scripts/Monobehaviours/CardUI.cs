using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private TMP_Text _cardRankText;
    [SerializeField] private TMP_Text _cardCostText;

    private CardData _cardData; // Add this field
    public CardData CardData => _cardData; // Add this property

    private RectTransform _playArea;
    private Vector3 _originalPosition;
    private Transform _originalParent;
    private Canvas _canvas;
    private Animator _animator;

    private bool _isSelected = false;
    public bool IsSelected => _isSelected;

    private void Start()
    {
        _playArea = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _originalParent = transform.parent;
        _animator = GetComponent<Animator>();
    }
    
    public void InitializeCard(CardData data)
    {
        _cardData = data; // Set the card data

        _cardImage.sprite = data.CardImage;
        _cardRankText.text = data.CardRank.ToString();
        _cardCostText.text = data.CardCost.ToString();

        OnCardDrawn(); // Trigger draw animation and effect
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = transform.position;
        transform.SetParent(_canvas.transform, true);
        transform.localScale = Vector3.one * 1.1f; // Slightly enlarge the card when dragging
        _animator.SetTrigger("OnStartDrag");
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
            ReturnToHand();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelection();
    }

    private void ToggleSelection()
    {
        _isSelected = !_isSelected;
        _animator.SetBool("IsSelected", _isSelected);
    }

    private void PlayCard()
    {
        transform.SetParent(_playArea);
        _animator.SetTrigger("OnPlay");
        CardEffectManager.Instance.PlayPlayEffect(transform.position);

        // Implement any additional logic for playing the card
        HandManager.Instance.PlayCard(this);

        // Optionally, destroy the card after playing
        // Destroy(gameObject);
    }

    private void ReturnToHand()
    {
        transform.position = _originalPosition;
        transform.SetParent(_originalParent, true);
        _animator.SetTrigger("OnReturnToHand");
    }

    public void OnCardDrawn()
    {
        _animator.SetTrigger("OnDraw");
        CardEffectManager.Instance.PlayDrawEffect(transform.position);
    }
}
