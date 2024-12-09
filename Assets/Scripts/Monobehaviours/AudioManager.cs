using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")] 
    [SerializeField] private AudioSource _ambientAudio;
    [SerializeField] private AudioSource _cardDrawAudio;
    [SerializeField] private AudioSource _cardFlipAudio;
    [SerializeField] private AudioSource _deckShuffleAudio;
    [SerializeField] private AudioSource _discardAudio;
    [SerializeField] private AudioSource _backToHandAudio;
    [SerializeField] private AudioSource _scoreSetAudio;
    [SerializeField] private AudioSource _seagullAudio;
    [SerializeField] private AudioSource _stageCardAudio;
    [SerializeField] private AudioSource _shellyAudio;
    [SerializeField] private AudioSource _loseAudio;
    [SerializeField] private AudioSource _transformAudio;
    
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
        _cardDrawAudio.time = 0.4f;
        _cardDrawAudio.Play();
    }

    public void PlayCardFlipAudio()
    {
        _cardFlipAudio.Play();
    }

    public void PlayStageCardAudio()
    {
        _stageCardAudio.time = 0.1f;
        _stageCardAudio.Play();
    }

    public void PlayDiscardAudio()
    {
        _discardAudio.Play();
    }

    public void PlayScoreSetAudio()
    {
        _scoreSetAudio.Play();
    }

    public void PlayBackToHandAudio()
    {
        _backToHandAudio.Play();
    }

    public void PlayAmbientAudio()
    {
        _ambientAudio.Play();
    }

    public void PlayShellyAudio()
    {
        _shellyAudio.Play();
    }

    public void StopShellyAudio()
    {
        if (_shellyAudio.isPlaying)
        {
            _shellyAudio.Stop();
        }
    }

    public void PlayLoseAudio()
    {
        if (_loseAudio.isPlaying)
        {
            _loseAudio.Stop();
        }
        _loseAudio.Play();
    }

    public void StopLoseAudio()
    {
        if (_loseAudio.isPlaying)
        {
            _loseAudio.Stop();
        }
    }

    public void PlayTransformAudio()
    {
        _transformAudio.Play();
    }
}
