using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickStartButton()
    {
        Debug.Log("[MenuController] Start Game button clicked.");

        // Ensure GameManager exists and starts the normal game mode
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNormalGame();
        }

        // Load Level 1 scene
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickHelpButton()
    {
        Debug.Log("[MenuController] Help button clicked. Loading HelpMenu...");
        SceneManager.LoadScene("HelpMenu");
    }

    public void OnClickCreditsButton()
    {
        Debug.Log("[MenuController] Credits button clicked. Loading CreditsMenu...");
        SceneManager.LoadScene("CreditsMenu");
    }

    public void OnClickQuitButton()
    {
        Debug.Log("[MenuController] Quit button clicked. Exiting application.");
        Application.Quit();
    }
}
