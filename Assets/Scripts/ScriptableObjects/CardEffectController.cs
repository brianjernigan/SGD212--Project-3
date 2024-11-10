using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _cardDrawEffectPrefab;
    [SerializeField] private ParticleSystem _cardPlayEffectPrefab;
    [SerializeField] private Animator _animator;

    public void TriggerDrawEffect(Vector3 position)
    {
        
    }

    public void TriggerPlayEffect(Vector3 position)
    {
        
    }

    public void TriggerAnimation(string triggerName)
    {
        _animator?.SetTrigger(triggerName);
    }
}
