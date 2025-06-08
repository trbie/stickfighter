using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonEvents : MonoBehaviour
{
    public Button normalModeButton;
    public Button onslaughtModeButton;

    public void Play()
    {
        normalModeButton.gameObject.SetActive(true);
        onslaughtModeButton.gameObject.SetActive(true);
    }

    public void Tutorial()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
    }

    public void PlayNormal()
    {
        GameMode.Current = GameMode.Mode.Normal;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Demo");
    }

    public void PlayOnslaught()
    {
        GameMode.Current = GameMode.Mode.Onslaught;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Demo");
    }
}