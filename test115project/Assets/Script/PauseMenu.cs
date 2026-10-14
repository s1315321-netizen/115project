using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;
    public static bool canPause = true; // 全域暫停開關：死亡或過場時可設為 false
    public static PauseManager Instance { get; private set; }

    [Header("--- UI References ---")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [Header("--- Audio Settings (Optional) ---")]
    [Tooltip("暫停時是否同時靜音場景音效")]
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
        canPause = true; // 重進遊戲或重新載入場景時重置為允許暫停
        Resume();

        if (optionsMenuUI != null)
        {
            optionsMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        // 關鍵：如果暫停功能被停用（例如玩家已死亡），直接忽略 ESC 鍵
        if (!canPause) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    /// <summary>
    /// 當玩家死亡、觸發 Jumpscare 或播放過場動畫時呼叫此方法
    /// </summary>
    public static void DisablePause()
    {
        canPause = false;

        // 若死亡瞬間暫停選單剛好開著，強制關閉並恢復時間流速，確保死亡協程正常執行
        if (Instance != null && IsGamePaused)
        {
            Instance.Resume();
        }
    }

    /// <summary>
    /// 復活或重新開始時呼叫此方法重新啟用暫停
    /// </summary>
    public static void EnablePause()
    {
        canPause = true;
    }

    private void HandleEscapeKey()
    {
        if (optionsMenuUI != null && optionsMenuUI.activeSelf)
        {
            CloseOptions();
            return;
        }

        if (IsGamePaused)
        {
            Resume();
            return;
        }

        if (InventoryManager.Instance != null && InventoryManager.Instance.inventoryUI != null && InventoryManager.Instance.inventoryUI.activeSelf)
        {
            InventoryManager.Instance.ToggleInventory();
            return;
        }

        Pause();
    }

    public void Pause()
    {
        if (!canPause) return;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        Time.timeScale = 0f;
        IsGamePaused = true;

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