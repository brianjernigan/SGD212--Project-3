using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShellyController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject _shellyTextBox;
    [SerializeField] private TMP_Text _shellyDialog;
    [SerializeField] private Image _shellyImage;
    [SerializeField] private Sprite _shellyClosed;
    [SerializeField] private Sprite _shellyOpen;

    [Header("Dialogue Settings")]
    [SerializeField] private float _typingSpeed = 0.1f; // Speed of the typing effect

    [Header("Random Dialog Options")]
    [SerializeField] private List<string> thinkingDialogs = new List<string>
    {
        "Hmm, let me think about that...",
        "Interesting choice...",
        "What could be the best move here?"
    };

    [SerializeField] private List<string> scoreDialogs = new List<string>
    {
        "Great job! Keep it up!",
        "You're on fire!",
        "What a fantastic move!"
    };

    private bool _isMouthOpen;
    private Coroutine _currentDialogCoroutine;
    private bool _isDialogueActive = false;
    
    public string GetRandomThinkingDialog()
    {
        if (thinkingDialogs.Count == 0)
        {
            return "Thinking...";
        }

        return thinkingDialogs[Random.Range(0, thinkingDialogs.Count)];
    }
    
    public string GetRandomScoreDialog()
    {
        if (scoreDialogs.Count == 0)
        {
            return "Good job!";
        }

        return scoreDialogs[Random.Range(0, scoreDialogs.Count)];
    }
    
    public void ActivateTextBox(string message)
    {
        if (_isDialogueActive)
        {
            return;
        }
        
        _shellyTextBox.SetActive(true);
        _isDialogueActive = true;

        // If a dialogue coroutine is already running, stop it to prevent overlap
        if (_currentDialogCoroutine != null)
        {
            StopCoroutine(_currentDialogCoroutine);
            _currentDialogCoroutine = null;
        }

        _currentDialogCoroutine = StartCoroutine(ShellyDialogRoutine(message));
    }
    
    public void DeactivateTextBox()
    {
        if (!_isDialogueActive)
        {
            return;
        }
        
        _shellyTextBox.SetActive(false);
        _isDialogueActive = false;

        // If a dialogue coroutine is running, stop it
        if (_currentDialogCoroutine != null)
        {
            StopCoroutine(_currentDialogCoroutine);
            _currentDialogCoroutine = null;
        }

        // Notify TutorialManager that the dialogue line has ended
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.EndDialogue();
        }
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

        // Notify TutorialManager that dialogue is complete
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.EndDialogue();
        }

        _isDialogueActive = false;
    }

    private void OnDisable()
    {
        // Ensure that any running coroutine is stopped when the object is disabled
        if (_currentDialogCoroutine != null)
        {
            StopCoroutine(_currentDialogCoroutine);
            _currentDialogCoroutine = null;
        }
    }
}
