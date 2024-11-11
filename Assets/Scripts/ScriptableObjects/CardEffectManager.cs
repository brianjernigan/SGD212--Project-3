using System.Collections;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance { get; private set; }

    [SerializeField] private ParticleSystem cardDrawEffectPrefab;
    [SerializeField] private ParticleSystem cardPlayEffectPrefab;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayDrawEffect(Vector3 position)
    {
        if (cardDrawEffectPrefab != null)
        {
            ParticleSystem instance = ParticlePooler.Instance.GetPooledParticle(cardDrawEffectPrefab);
            instance.transform.position = position;
            instance.Play();
            StartCoroutine(ReturnParticleToPool(instance));
        }
    }

    public void PlayPlayEffect(Vector3 position)
    {
        if (cardPlayEffectPrefab != null)
        {
            ParticleSystem instance = ParticlePooler.Instance.GetPooledParticle(cardPlayEffectPrefab);
            instance.transform.position = position;
            instance.Play();
            StartCoroutine(ReturnParticleToPool(instance));
        }
    }

    private IEnumerator ReturnParticleToPool(ParticleSystem particle)
    {
        yield return new WaitForSeconds(particle.main.duration + particle.main.startLifetime.constantMax);
        ParticlePooler.Instance.ReturnToPool(particle);
    }

    public void TriggerAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}
