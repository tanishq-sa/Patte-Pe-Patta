using System.Net.Mime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayMulti()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void PlaySing()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void QuitGame(){
        Application.Quit();
    }
}