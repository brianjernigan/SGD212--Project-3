using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Existing methods
    public void OnClickStartButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickBackButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickHelpButton()
    {
        SceneManager.LoadScene("Help");
    }

    public void OnClickCreditsButton()
    {
        SceneManager.LoadScene("Credits");
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }

    // New method for Tutorial button
    public void OnClickTutorialButton()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
