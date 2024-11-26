using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")] 
    [SerializeField] private AudioSource _buttonClickAudio;
    [SerializeField] private AudioSource _cardDrawAudio;
    [SerializeField] private AudioSource _cardFlipAudio;
    [SerializeField] private AudioSource _discardAudio;
    [SerializeField] private AudioSource _matchAudio;
    [SerializeField] private AudioSource _playCardAudio;
    [SerializeField] private AudioSource _seagullsAudio;
    [SerializeField] private AudioSource _splashAudio;
    
    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayCardDrawAudio()
    {
        if (_cardDrawAudio != null)
        {
            _cardDrawAudio.Play();
        }
        else
        {
            Debug.LogWarning("Card draw audio source is not assigned.");
        }
    }

    public void PlayCardFlipAudio()
    {
        if (_cardFlipAudio != null)
        {
            _cardFlipAudio.Play();
        }
        else
        {
            Debug.LogWarning("Card flip audio source is not assigned.");
        }
    }
}
