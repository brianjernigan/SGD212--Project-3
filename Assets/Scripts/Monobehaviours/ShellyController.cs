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

    private void Awake()
    {
        if (_shellyTextBox == null)
        {
            Debug.LogError("[ShellyController Awake] _shellyTextBox is not assigned.");
        }
        if (_shellyDialog == null)
        {
            Debug.LogError("[ShellyController Awake] _shellyDialog is not assigned.");
        }
        if (_shellyImage == null)
        {
            Debug.LogError("[ShellyController Awake] _shellyImage is not assigned.");
        }
    }

    /// <summary>
    /// Returns a random thinking dialogue string.
    /// </summary>
    public string GetRandomThinkingDialog()
    {
        if (thinkingDialogs.Count == 0)
        {
            Debug.LogWarning("[ShellyController GetRandomThinkingDialog] thinkingDialogs list is empty.");
            return "Thinking...";
        }

        return thinkingDialogs[Random.Range(0, thinkingDialogs.Count)];
    }

    /// <summary>
    /// Returns a random score dialogue string.
    /// </summary>
    public string GetRandomScoreDialog()
    {
        if (scoreDialogs.Count == 0)
        {
            Debug.LogWarning("[ShellyController GetRandomScoreDialog] scoreDialogs list is empty.");
            return "Good job!";
        }

        return scoreDialogs[Random.Range(0, scoreDialogs.Count)];
    }

    /// <summary>
    /// Activates the text box and starts displaying the provided message.
    /// Prevents overlapping dialogues if one is already active.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void ActivateTextBox(string message)
    {
        if (_isDialogueActive)
        {
            Debug.LogWarning("[ShellyController ActivateTextBox] Dialogue already active. Skipping new dialogue.");
            return;
        }

        Debug.Log($"[ShellyController ActivateTextBox] Activating text box with message: {message}");
        _shellyTextBox.SetActive(true);
        _isDialogueActive = true;

        // If a dialogue coroutine is already running, stop it to prevent overlap
        if (_currentDialogCoroutine != null)
        {
            StopCoroutine(_currentDialogCoroutine);
            _currentDialogCoroutine = null;
            Debug.Log("[ShellyController ActivateTextBox] Stopped existing dialogue coroutine.");
        }

        _currentDialogCoroutine = StartCoroutine(ShellyDialogRoutine(message));
    }

    /// <summary>
    /// Deactivates the text box and stops any ongoing dialogue coroutine.
    /// </summary>
    public void DeactivateTextBox()
    {
        if (!_isDialogueActive)
        {
            Debug.LogWarning("[ShellyController DeactivateTextBox] Attempted to deactivate text box when no dialogue is active.");
            return;
        }

        Debug.Log("[ShellyController DeactivateTextBox] Deactivating text box.");
        _shellyTextBox.SetActive(false);
        _isDialogueActive = false;

        // If a dialogue coroutine is running, stop it
        if (_currentDialogCoroutine != null)
        {
            StopCoroutine(_currentDialogCoroutine);
            _currentDialogCoroutine = null;
            Debug.Log("[ShellyController DeactivateTextBox] Stopped dialogue coroutine upon deactivation.");
        }

        // Notify TutorialManager that the dialogue line has ended
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.EndDialogue();
        }
    }

    /// <summary>
    /// Coroutine that types out the dialogue message character by character.
    /// </summary>
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
