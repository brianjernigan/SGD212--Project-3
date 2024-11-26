using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEffect : MonoBehaviour
{
    [Header("Wave Parameters")]
    [Tooltip("Amplitude of the wave (height of oscillation).")]
    [SerializeField] private float waveAmplitude = 5f;

    [Tooltip("Speed of the wave oscillation.")]
    [SerializeField] private float waveSpeed = 1f;

    [Tooltip("List of target objects to apply the wave effect. If empty, all child objects will be used.")]
    [SerializeField] private List<Transform> targetObjects = new List<Transform>();

    [Tooltip("Optional: Offset each object's wave phase for a cascading effect.")]
    [SerializeField] private float phaseOffset = 0.5f;

    [Tooltip("Use localPosition for wave movement. Disable for world position.")]
    [SerializeField] private bool useLocalPosition = true;

    private bool isWaveActive = false;
    private Coroutine waveCoroutine;
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

    private void Start()
    {
        InitializeTargets();
        StartWave(); // Automatically start the wave animation
    }

    /// <summary>
    /// Initializes the target objects for the wave effect.
    /// </summary>
    private void InitializeTargets()
    {
        Debug.Log("Initializing WaveEffect targets...");
        originalPositions.Clear();

        // If no targets are assigned, use all child objects
        if (targetObjects == null || targetObjects.Count == 0)
        {
            foreach (Transform child in transform)
            {
                targetObjects.Add(child);
                Debug.Log($"Auto-added child object: {child.name}");
            }
        }

        // Store the original positions of all target objects
        foreach (Transform obj in targetObjects)
        {
            if (obj != null)
            {
                Vector3 originalPosition = useLocalPosition ? obj.localPosition : obj.position;
                originalPositions[obj] = originalPosition;
                Debug.Log($"Stored original position for {obj.name}: {originalPosition}");
            }
            else
            {
                Debug.LogWarning("Target object is null. Skipping.");
            }
        }

        Debug.Log($"WaveEffect initialized with {targetObjects.Count} target(s).");
            }
        }
    }

    /// <summary>
    /// Starts the wave animation.
    /// </summary>
    public void StartWave()
    {
        if (isWaveActive)
        {
            Debug.LogWarning("Wave effect is already active.");
            return;
        }

        if (targetObjects.Count == 0)
        {
            Debug.LogError("No target objects found. Wave effect cannot start.");
            return;
        }

        Debug.Log("Starting WaveAnimation coroutine.");
        if (isWaveActive || targetObjects.Count == 0) return;

        isWaveActive = true;
        waveCoroutine = StartCoroutine(WaveAnimation());
    }

    /// <summary>
    /// Stops the wave animation and resets objects to their original positions.
    /// </summary>
    public void StopWave()
    {
        if (!isWaveActive)
        {
            Debug.LogWarning("Wave effect is not active.");
            return;
        }

        Debug.Log("Stopping WaveAnimation coroutine.");
        if (!isWaveActive) return;

        isWaveActive = false;

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        ResetObjectPositions();
    }

    /// <summary>
    /// Coroutine that applies the wave effect to the target objects.
    /// </summary>
    private IEnumerator WaveAnimation()
    {
        Debug.Log("WaveAnimation coroutine started.");
        float time = 0f;

        while (isWaveActive)
        {
            time += Time.deltaTime * waveSpeed;

            foreach (Transform obj in targetObjects)
            {
                if (obj == null)
                {
                    Debug.LogWarning("A target object is null. Skipping.");
                    continue;
                }

                if (!originalPositions.ContainsKey(obj))
                {
                    Debug.LogError($"Original position for {obj.name} is missing. Skipping animation.");
                    continue;
                }
                if (obj == null || !originalPositions.ContainsKey(obj)) continue;

                Vector3 originalPosition = originalPositions[obj];
                float yOffset = Mathf.Sin(time + obj.GetInstanceID() * phaseOffset) * waveAmplitude;

                if (useLocalPosition)
                {
                    obj.localPosition = new Vector3(originalPosition.x, originalPosition.y + yOffset, originalPosition.z);
                    Debug.Log($"{obj.name}: Updated localPosition to {obj.localPosition}");
                }
                else
                {
                    obj.position = new Vector3(originalPosition.x, originalPosition.y + yOffset, originalPosition.z);
                    Debug.Log($"{obj.name}: Updated position to {obj.position}");
                }
            }

            yield return null;
        }
        Debug.Log("WaveAnimation coroutine ended.");
    }

    /// <summary>
    /// Resets all target objects to their original positions.
    /// </summary>
    private void ResetObjectPositions()
    {
        Debug.Log("Resetting target objects to their original positions...");

        foreach (Transform obj in targetObjects)
        {
            if (obj == null)
            {
                Debug.LogWarning("A target object is null during reset. Skipping.");
                continue;
            }

            if (originalPositions.ContainsKey(obj))
            {
                Vector3 originalPosition = originalPositions[obj];
                if (useLocalPosition)
                {
                    obj.localPosition = originalPosition;
                    Debug.Log($"{obj.name}: Reset localPosition to {originalPosition}");
                }
                else
                {
                    obj.position = originalPosition;
                    Debug.Log($"{obj.name}: Reset position to {originalPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"{obj.name} does not have an original position stored. Skipping reset.");
            }
        }

        Debug.Log("All target objects have been reset.");
        foreach (Transform obj in targetObjects)
        {
            if (obj == null || !originalPositions.ContainsKey(obj)) continue;

            Vector3 originalPosition = originalPositions[obj];
            if (useLocalPosition)
            {
                obj.localPosition = originalPosition;
            }
            else
            {
                obj.position = originalPosition;
            }
        }
    }

    /// <summary>
    /// Dynamically adds a new object to the wave effect.
    /// </summary>
    /// <param name="newTarget">The Transform to add to the wave effect.</param>
    public void AddTarget(Transform newTarget)
    {
        if (newTarget != null && !targetObjects.Contains(newTarget))
        {
            targetObjects.Add(newTarget);
            Vector3 originalPosition = useLocalPosition ? newTarget.localPosition : newTarget.position;
            originalPositions[newTarget] = originalPosition;
            Debug.Log($"Added {newTarget.name} to the wave effect.");
        }
        else
        {
            Debug.LogWarning($"Cannot add {newTarget?.name ?? "null"} to the wave effect. It may already exist in the target list.");
        }
    }

    /// <summary>
    /// Dynamically removes an object from the wave effect.
    /// </summary>
    /// <param name="target">The Transform to remove from the wave effect.</param>
    public void RemoveTarget(Transform target)
    {
        if (targetObjects.Contains(target))
        {
            targetObjects.Remove(target);
            originalPositions.Remove(target);
            Debug.Log($"Removed {target.name} from the wave effect.");
        }
        else
        {
            Debug.LogWarning($"{target?.name ?? "null"} is not in the target list. Cannot remove.");
        }
    }

    /// <summary>
    /// Debug visualization of the original positions in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (originalPositions == null) return;

        Gizmos.color = Color.blue;
        foreach (KeyValuePair<Transform, Vector3> entry in originalPositions)
        {
            Gizmos.DrawSphere(entry.Value, 0.1f);
        }
    }
}
