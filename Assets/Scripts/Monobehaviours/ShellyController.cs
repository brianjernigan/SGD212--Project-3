using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShellyController : MonoBehaviour
{
    [SerializeField] private GameObject _shellyTextBox;
    [SerializeField] private TMP_Text _shellyDialog;
    [SerializeField] private Image _shellyImage;
    [SerializeField] private Sprite _shellyClosed;
    [SerializeField] private Sprite _shellyOpen;

    private readonly float _typingSpeed = 0.1f;
    private bool _isMouthOpen;

    private Coroutine _currentDialogCoroutine;

    public void ActivateTextBox(string message = "")
    {
        if (!_shellyTextBox.activeSelf)
        {
            _shellyTextBox.SetActive(true);
        }
        
        UpdateShellyDialog(message);
    }

    public void DeactivateTextBox()
    {
        _shellyTextBox.SetActive(false);
    }

    private void UpdateShellyDialog(string message)
    {
        if (_currentDialogCoroutine != null)
        {
            StopCoroutine(_currentDialogCoroutine);
        }

        _currentDialogCoroutine = StartCoroutine(ShellyDialogRoutine(message));
    }

    private IEnumerator ShellyDialogRoutine(string message)
    {
        _shellyDialog.text = "";

        AudioManager.Instance.PlayShellyAudio();
        
        foreach (var character in message)
        {
            _shellyDialog.text += character;
            _isMouthOpen = !_isMouthOpen;
            _shellyImage.sprite = _isMouthOpen ? _shellyOpen : _shellyClosed;
            yield return new WaitForSeconds(_typingSpeed);
        }

        _shellyImage.sprite = _shellyClosed;
        AudioManager.Instance.StopShellyAudio();
        _currentDialogCoroutine = null;
    }

    public string GetRandomShellyScoreDialog()
    {
        var scoreDialog = new[] {"Bingo!", "Nice!", "Great move!"};
        return scoreDialog[Random.Range(0, scoreDialog.Length)];
    }
}
