using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermanentFixturesManager : MonoBehaviour
{
    public static PermanentFixturesManager Instance { get; private set; }

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
}
