using System.Collections;
using UnityEngine;

namespace HunterScripts
{
    public class HunterCardEffectManager : MonoBehaviour
    {
        public static HunterCardEffectManager Instance { get; private set; }

        [SerializeField] private ParticleSystem cardDrawEffectPrefab;
        [SerializeField] private ParticleSystem cardPlayEffectPrefab;

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
                ParticleSystem instance = Instantiate(cardDrawEffectPrefab, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
            }
        }

        public void PlayPlayEffect(Vector3 position)
        {
            if (cardPlayEffectPrefab != null)
            {
                ParticleSystem instance = Instantiate(cardPlayEffectPrefab, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
            }
        }
    }
}
