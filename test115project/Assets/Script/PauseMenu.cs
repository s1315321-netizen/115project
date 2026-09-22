using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;   // 把整個 PauseMenu 面板拖進來（裡面包含三顆按鈕）
    public GameObject OptionsMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI.activeSelf)
            {
                Resume();   // 選單開著時按 ESC → 關閉選單
            }
            else
            {
                Pause();    // 選單關著時按 ESC → 開啟選單
            }
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        OptionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        OptionsMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }


    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}