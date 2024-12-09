using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHotkeySwitcher : MonoBehaviour
{
    [System.Serializable]
    public class SceneHotkey
    {
        public string sceneName; // The name of the scene to load
        public KeyCode hotkey;   // The key used to load the scene
    }

    [SerializeField]
    private SceneHotkey[] sceneHotkeys; // Array of hotkey-to-scene mappings

    private void Update()
    {
        foreach (var hotkey in sceneHotkeys)
        {
            if (Input.GetKeyDown(hotkey.hotkey))
            {
                Debug.Log($"[SceneHotkeySwitcher] Loading scene: {hotkey.sceneName} via hotkey: {hotkey.hotkey}");
                SceneManager.LoadScene(hotkey.sceneName);
                break; // Ensure only one hotkey action is processed per frame
            }
        }
    }
}
