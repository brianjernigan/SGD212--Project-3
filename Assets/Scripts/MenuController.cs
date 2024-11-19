using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickStartButton()
    {
        SceneManager.LoadScene("L1");
    }

    public void OnClickHelpButton()
    {
        SceneManager.LoadScene("HelpMenu");
    }

    public void OnClickCreditsButton()
    {
        SceneManager.LoadScene("CreditsMenu");
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
