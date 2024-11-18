using System.Collections;
using System.Collections.Generic;
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
}
