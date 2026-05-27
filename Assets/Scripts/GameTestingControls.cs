using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTestingControls : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.P;
    [SerializeField] private KeyCode restartKey = KeyCode.F5;
    [SerializeField] private KeyCode quitKey = KeyCode.F10;

    public bool IsPaused { get; private set; }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(restartKey))
        {
            RestartScene();
        }

        if (Input.GetKeyDown(quitKey))
        {
            QuitGame();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool isPaused)
    {
        IsPaused = isPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
