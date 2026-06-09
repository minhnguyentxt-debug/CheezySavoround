using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }
    public void LoadHomeScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}