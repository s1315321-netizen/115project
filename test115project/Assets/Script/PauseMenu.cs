using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;
    public static bool canPause = true; // 與死亡系統連動：死亡時為 false
    public static PauseManager Instance { get; private set; }

    [Header("--- UI 面板 ---")]
    [Tooltip("遊戲內的 PauseMenuUI 面板")]
    public GameObject pauseMenuUI;
    [Tooltip("遊戲內的 OptionsMenuUI 面板 (Prefab)")]
    public GameObject optionsMenuUI;

    [Header("--- 場景切換 ---")]
    [Tooltip("主選單場景名稱 (請確認大小寫與 Build Settings 一致)")]
    public string mainMenuSceneName = "MainMenu";

    [Header("--- 音效設定 ---")]
    [Tooltip("暫停時是否同時靜音全域音效")]
    public bool pauseAudioOnPause = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        canPause = true;
        Resume();

        if (optionsMenuUI != null)
        {
            optionsMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        // 死亡演出或特定過場時禁止呼叫暫停
        if (!canPause) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    /// <summary>
    /// ESC 鍵響應優先權處理
    /// </summary>
    private void HandleEscapeKey()
    {
        // 1. 若設定選單開著：按 ESC 返回暫停選單
        if (optionsMenuUI != null && optionsMenuUI.activeSelf)
        {
            CloseOptions();
            return;
        }

        // 2. 若暫停選單開著：按 ESC 恢復遊戲
        if (IsGamePaused)
        {
            Resume();
            return;
        }

        // 3. 若背包開著：按 ESC 優先關閉背包
        if (InventoryManager.Instance != null && InventoryManager.Instance.inventoryUI != null && InventoryManager.Instance.inventoryUI.activeSelf)
        {
            InventoryManager.Instance.ToggleInventory();
            return;
        }

        // 4. 正常遊玩中：按 ESC 開啟暫停
        Pause();
    }

    /// <summary>
    /// 供外部 (如 SimpleDeathSystem) 死亡時禁用暫停功能
    /// </summary>
    public static void DisablePause()
    {
        canPause = false;
        if (Instance != null && IsGamePaused)
        {
            Instance.Resume();
        }
    }

    /// <summary>
    /// 供外部重生時重新啟用暫停功能
    /// </summary>
    public static void EnablePause()
    {
        canPause = true;
    }

    public void Pause()
    {
        if (!canPause) return;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        Time.timeScale = 0f;
        IsGamePaused = true;

        // 解除游標鎖定，確保能點擊選單
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseAudioOnPause)
        {
            AudioListener.pause = true;
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        IsGamePaused = false;

        if (pauseAudioOnPause)
        {
            AudioListener.pause = false;
        }
    }

    public void OpenOptions()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsGamePaused = false;
        canPause = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsGamePaused = false;
        canPause = true;
    }
}