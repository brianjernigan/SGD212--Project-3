using System.Collections.Generic;
using UnityEngine;

public class ParticlePooler : MonoBehaviour
{
    public static ParticlePooler Instance { get; private set; }

    [SerializeField] private ParticleSystem particlePrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<ParticleSystem> poolQueue = new Queue<ParticleSystem>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for(int i = 0; i < poolSize; i++)
        {
            ParticleSystem obj = Instantiate(particlePrefab);
            obj.gameObject.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public ParticleSystem GetPooledParticle()
    {
        if(poolQueue.Count > 0)
        {
            ParticleSystem obj = poolQueue.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            ParticleSystem obj = Instantiate(particlePrefab);
            return obj;
        }
    }

    public void ReturnToPool(ParticleSystem obj)
    {
        obj.gameObject.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}
