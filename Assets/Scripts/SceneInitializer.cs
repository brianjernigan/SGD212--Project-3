using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSceneReferences(
                stageArea: FindObjectWithTag("StageArea"),
                discardArea: FindObjectWithTag("DiscardArea"),
                deck: FindObjectWithTag("Deck"),
                whirlpoolCenter: FindObjectWithTag("WhirlpoolCenter"),
                handArea: FindHandAreaByLevel(GameManager.Instance.CurrentLevel)
            );

            Debug.Log("[SceneInitializer] Scene-specific references dynamically assigned.");
        }
        else
        {
            Debug.LogWarning("[SceneInitializer] GameManager instance not found.");
        }
    }

    private GameObject FindObjectWithTag(string tag)
    {
        GameObject obj = GameObject.FindWithTag(tag);
        if (obj == null)
        {
            Debug.LogWarning($"[SceneInitializer] Object with tag '{tag}' not found.");
        }
        return obj;
    }

    private GameObject FindHandAreaByLevel(int level)
    {
        string tag = $"HandArea{level}";
        return FindObjectWithTag(tag);
    }
}
