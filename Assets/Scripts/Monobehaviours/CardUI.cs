using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardUI : MonoBehaviour
{
    [Header("Rank Texts")]
    [SerializeField] private TMP_Text _topRankText;
    [SerializeField] private TMP_Text _bottomRankText;

    private Camera _mainCamera;
    private bool _isDragging;
    private Vector3 _offset;
    private float _originalYPosition;

    private GameObject _cardObject;
    private CardData _cardData;
    private GameCard _gameCard;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private Transform _lastDropZone;

    private List<Transform> _dropZones;

    private const float CardScaleFactor = 1.25f;

    private void Start()
    {
        _mainCamera = Camera.main;
        _originalScale = transform.localScale;
        _originalRotation = transform.rotation;

        _dropZones = new List<Transform>()
        {
            GameManager.Instance.Hand.transform,
            GameManager.Instance.Stage.transform,
            GameManager.Instance.Discard.transform
        };
    }
    
    public void InitializeCard(CardData data, GameCard gameCard)
    {
        _cardData = data;
        _gameCard = gameCard;
        SetCardUI();
    }

    private void SetCardUI()
    {
        _topRankText.text = _cardData.CardRank.ToString();
        _bottomRankText.text = _cardData.CardRank.ToString();
        GetComponent<MeshRenderer>().material = _cardData.CardMat;
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.IsDraggingCard) return;
        transform.localScale *= CardScaleFactor;
    }

    private void OnMouseExit()
    {
        if (GameManager.Instance.IsDraggingCard) return;
        transform.localScale = _originalScale;
        transform.rotation = Quaternion.Euler(90f, 0f, 180f);
    }

    // private void OnMouseOver()
    // {
    //     var mouseWorldPosition = GetMouseWorldPosition();
    //
    //     var cardCenter = GetComponent<Collider>().bounds.center;
    //     var offset = mouseWorldPosition - cardCenter;
    //
    //     var normalizedOffset = new Vector2(offset.x, offset.z).normalized;
    //
    //     var maxTiltAngle = 10f;
    //     var tiltX = -normalizedOffset.y * maxTiltAngle;
    //     var tiltZ = normalizedOffset.x * maxTiltAngle;
    //
    //     var targetRotation = Quaternion.Euler(tiltX, _originalRotation.eulerAngles.y, tiltZ);
    //     transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    // }

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

        if (newDropZone is not null && _lastDropZone != newDropZone)
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
        if (zoneCollider is not null)
        {
            return zoneCollider.bounds.Contains(transform.position);
        }

        return false;
    }
}
