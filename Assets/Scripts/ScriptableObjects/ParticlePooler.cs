using System.Collections.Generic;
using UnityEngine;

public class ParticlePooler : MonoBehaviour
{
    public static ParticlePooler Instance { get; private set; }

    [SerializeField] private int poolSize = 20;

    private Dictionary<ParticleSystem, Queue<ParticleSystem>> poolDictionary = new Dictionary<ParticleSystem, Queue<ParticleSystem>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Do not destroy on load if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ParticleSystem GetPooledParticle(ParticleSystem prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<ParticleSystem>();
            InitializePool(prefab);
        }

        if (poolDictionary[prefab].Count > 0)
        {
            ParticleSystem obj = poolDictionary[prefab].Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            ParticleSystem obj = Instantiate(prefab);
            return obj;
        }
    }

    private void InitializePool(ParticleSystem prefab)
    {
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem obj = Instantiate(prefab);
            obj.gameObject.SetActive(false);
            poolDictionary[prefab].Enqueue(obj);
        }
    }

    public void ReturnToPool(ParticleSystem obj)
    {
        obj.gameObject.SetActive(false);
        poolDictionary[obj].Enqueue(obj);
    }
}
