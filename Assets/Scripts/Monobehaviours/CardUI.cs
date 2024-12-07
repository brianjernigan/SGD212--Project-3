using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text _topRankText;
    [SerializeField] private TMP_Text _bottomRankText;
    [SerializeField] private TMP_Text _descriptionText;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem _bubbleEffect; // Reference to the BubbleEffect ParticleSystem

    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 1.0f; // Adjustable height for hover effect

    private Camera _mainCamera;
    private bool _isDragging;
    private Vector3 _offset;
    private float _originalYPosition;

    private CardData _cardData;
    private GameCard _gameCard;

    private Vector3 _originalPosition;
    private Vector3 _hoverPosition;
    private Vector3 _originalScale;
    private Transform _lastDropZone;

    private List<Transform> _dropZones;

    private const float CardScaleFactor = 1.25f;
    private bool _isHovered = false; // Ensure this is declared here

    private void Start()
    {
        _mainCamera = Camera.main;
        _originalScale = transform.localScale;

        _dropZones = new List<Transform>()
        {
            GameManager.Instance.Hand.transform,
            GameManager.Instance.Stage.transform,
            GameManager.Instance.Discard.transform
        };

        _originalPosition = transform.position;
        _hoverPosition = _originalPosition + new Vector3(0, hoverHeight, 0); // Now hoverHeight is defined
    }

    public void InitializeCard(CardData data, GameCard gameCard)
    {
        _cardData = data;
        _gameCard = gameCard;
        SetCardUI();
    }

    private void SetCardUI()
    {
        if (_cardData.Type == CardType.Unranked)
        {
            _topRankText.text = "";
            _bottomRankText.text = "";
        }
        else
        {
            _topRankText.text = _cardData.CardRank.ToString();
            _bottomRankText.text = _cardData.CardRank.ToString();
        }

        GetComponent<MeshRenderer>().material = _cardData.CardMat;
        _descriptionText.text = _gameCard.Description;
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.IsDraggingCard) return;
        _isHovered = true;
        StartCoroutine(MoveCard(_hoverPosition));
    }

    private void OnMouseExit()
    {
        if (GameManager.Instance.IsDraggingCard) return;
        _isHovered = false;
        StartCoroutine(MoveCard(_originalPosition));
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        GameManager.Instance.IsDraggingCard = true;

        _originalPosition = transform.position;
        transform.localScale = _originalScale;
        transform.localScale *= CardScaleFactor;

        _originalYPosition = transform.position.y;

        var mouseWorldPosition = GetMouseWorldPosition();
        _offset = transform.position - mouseWorldPosition;

        foreach (var zone in _dropZones)
        {
            if (IsWithinDropZone(zone))
            {
                _lastDropZone = zone;
                break;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;

        var mouseWorldPosition = GetMouseWorldPosition();
        transform.position = new Vector3(mouseWorldPosition.x + _offset.x, _originalYPosition, mouseWorldPosition.z + _offset.z);
    }

    private void OnMouseUp()
    {
        _isDragging = false;
        GameManager.Instance.IsDraggingCard = false;

        Transform newDropZone = null;

        foreach (var zone in _dropZones)
        {
            if (IsWithinDropZone(zone))
            {
                newDropZone = zone;
                break;
            }
        }

        if (newDropZone != null && _lastDropZone != newDropZone)
        {
            if (!GameManager.Instance.TryDropCard(newDropZone, _gameCard))
            {
                transform.position = _originalPosition;
            }
            else
            {
                _lastDropZone = newDropZone;
            }
        }
        else
        {
            transform.position = _originalPosition;
        }

        transform.localScale = _originalScale;
    }

    private Vector3 GetMouseWorldPosition()
    {
        var mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = _mainCamera.WorldToScreenPoint(transform.position).z;

        return _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    private bool IsWithinDropZone(Transform zone)
    {
        var zoneCollider = zone.GetComponent<Collider>();
        if (zoneCollider != null)
        {
            return zoneCollider.bounds.Contains(transform.position);
        }

        return false;
    }

    /// <summary>
    /// Plays the bubble particle effect attached to this card.
    /// </summary>
    public void PlayBubbleEffect()
    {
        if (_bubbleEffect != null)
        {
            _bubbleEffect.Play();
        }
    }

    /// <summary>
    /// Stops the bubble particle effect attached to this card.
    /// </summary>
    public void StopBubbleEffect()
    {
        if (_bubbleEffect != null)
        {
            _bubbleEffect.Stop();
        }
    }

    private IEnumerator MoveCard(Vector3 targetPosition)
    {
        float duration = 0.2f;
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }
}
