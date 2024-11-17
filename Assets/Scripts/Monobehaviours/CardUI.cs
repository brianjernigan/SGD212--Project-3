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
    private float _yPos;

    private GameObject _cardObject;
    private CardData _cardData;
    private GameCard _gameCard;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Transform _currentDropZone;

    private void Start()
    {
        _mainCamera = Camera.main;
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

    private void OnMouseDown()
    {
        _isDragging = true;

        _originalPosition = transform.position;
        _originalScale = transform.localScale;

        _yPos = transform.position.y;

        var mouseWorldPosition = GetMouseWorldPosition();
        _offset = transform.position - mouseWorldPosition;
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;

        var mouseWorldPosition = GetMouseWorldPosition();
        transform.position = new Vector3(mouseWorldPosition.x + _offset.x, _yPos, mouseWorldPosition.z + _offset.z);
    }

    private void OnMouseUp()
    {
        _isDragging = false;

        Transform validZone = null;

        var zones = new List<Transform>()
        {
            GameManager.Instance.Hand.transform,
            GameManager.Instance.Stage.transform,
            GameManager.Instance.Discard.transform
        };

        foreach (var zone in zones)
        {
            if (IsWithinDropZone(zone))
            {
                validZone = zone;
                break;
            }
        }

        if (validZone is not null)
        {
            if (!GameManager.Instance.TryDropCard(validZone, _gameCard))
            {
                transform.position = _originalPosition;
            }
        }
        else
        {
            transform.position = _originalPosition;
        }
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
