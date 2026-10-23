using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("--- UI Panels ---")]
    public GameObject mainMenuUI;     // 主選單面板 (Start / Options / Leave)
    public GameObject optionsMenuUI;  // OptionsMenu 面板

    [Header("--- Scene Transition ---")]
    public string firstSceneName = "room1"; // 預設要載入的第一關名稱
    public CanvasGroup fadeCanvasGroup;     // 拖入黑幕上的 CanvasGroup
    public float fadeDuration = 1.0f;       // 淡出/淡入過渡時間 (秒)
    public bool fadeInOnStart = true;       // 剛進入此場景時是否從全黑淡入顯示畫面

    private bool isTransitioning = false;   // 防止重複觸發切換

    void Start()
    {
        // 進入主選單時，重置遊戲時間與音效
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 初始化面板狀態
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        // 如果勾選進入場景淡入：從全黑 (1) 平滑變透明 (0)
        if (fadeInOnStart && fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeRoutine(0f));
        }
        else if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        // 轉場期間不允許按鍵操作
        if (isTransitioning) return;

        // 當設定面板開啟時，按 ESC 鍵可直接返回主選單
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenuUI != null && optionsMenuUI.activeSelf)
            {
                CloseOptions();
            }
        }
    }

    /// <summary>
    /// 點擊【Start Game】進入遊戲（平滑淡出至全黑後載入場景）
    /// </summary>
    public void GoToRoom1()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoadRoutine(firstSceneName));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        isTransitioning = true;

        // 淡出至全黑 (Alpha = 1)
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeRoutine(1f));
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        // 載入新場景
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 平滑控制黑幕透明度協程
    /// </summary>
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.gameObject.SetActive(true);

        // 只要不是完全透明，就阻擋滑鼠射線點擊
        fadeCanvasGroup.blocksRaycasts = (targetAlpha > 0f);

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // 使用 unscaledDeltaTime 確保時間縮放為 0 時轉場依然流暢
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // 完全透明時關閉射線阻擋，允許玩家點擊按鈕
        if (targetAlpha <= 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 點擊【Options】開啟設定
    /// </summary>
    public void OpenOptions()
    {
        if (isTransitioning) return;
        
        if (optionsMenuUI != null) optionsMenuUI.SetActive(true);
    }

    /// <summary>
    /// 點擊【Back】或按 ESC 關閉設定
    /// </summary>
    public void CloseOptions()
    {
        if (isTransitioning) return;
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    /// <summary>
    /// 點擊【Quit】離開遊戲
    /// </summary>
    public void QuitGame()
    {
        if (isTransitioning) return;
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}