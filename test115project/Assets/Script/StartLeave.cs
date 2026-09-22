using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject mainMenuUI;     // 主選單面板（放 Start / Options / Leave 的那層，如果有分層的話）
    public GameObject optionsMenuUI;  // OptionsMenu 面板

    public void GoToRoom1()
    {
        SceneManager.LoadScene("room1");
    }

    public void OpenOptions()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}