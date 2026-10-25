using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictorySystem : Interactable
{
    [Header("--- 通關設定 ---")]
    [Tooltip("開啟這扇門所需的鑰匙名稱或 ID")]
    public string requiredKeyID = "MainDoorKey";
    [Tooltip("是否需要消耗鑰匙")]
    public bool consumeKey = true;
    [Tooltip("提示訊息")]
    public string lockedMessage = "大門深鎖，需要一把大門鑰匙...";

    [Header("--- 勝利介面組件 (CanvasGroup) ---")]
    [Tooltip("整個結算畫面的 GameObject (Canvas)")]
    public GameObject victoryCanvasObj;
    [Tooltip("黑色背景的 CanvasGroup")]
    public CanvasGroup bgCanvasGroup;
    [Tooltip("標題文字的 CanvasGroup")]
    public CanvasGroup titleCanvasGroup;
    [Tooltip("按鈕容器的 CanvasGroup")]
    public CanvasGroup buttonsCanvasGroup;

    [Header("--- 音效設定 ---")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;
    public AudioSource victoryAmbienceSource;

    [Header("--- 場景設定 ---")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "room1";

    private bool hasWon = false;

    private void Start()
    {
        // 遊戲一開始先關閉結算介面
        if (victoryCanvasObj != null)
        {
            victoryCanvasObj.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (hasWon) return;

        // 檢查背包是否有大門鑰匙
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyID))
        {
            if (consumeKey)
            {
                InventoryManager.Instance.RemoveItem(requiredKeyID);
            }

            hasWon = true;
            PauseManager.DisablePause();

            StartCoroutine(VictoryProcess());
        }
        else
        {
            Debug.Log(lockedMessage);
            PlayerInteract playerInteract = FindObjectOfType<PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.ShowPickupPrompt("");
            }
        }
    }

    private IEnumerator VictoryProcess()
    {
        // 1. 播放開門音效
        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        yield return new WaitForSeconds(0.5f);

        // 2. 啟用結算介面，並將所有 Alpha 歸零
        if (victoryCanvasObj != null)
        {
            victoryCanvasObj.SetActive(true);
        }

        SetAlpha(bgCanvasGroup, 0f);
        SetAlpha(titleCanvasGroup, 0f);
        SetAlpha(buttonsCanvasGroup, 0f);

        if (victoryAmbienceSource != null)
        {
            victoryAmbienceSource.Play();
        }

        // 3. 模擬動畫時間軸：依序淡入
        // --- 階段一：黑色背景淡入 ---
        yield return StartCoroutine(FadeInGroup(bgCanvasGroup, 1f));

        // --- 階段二：標題文字淡入 ---
        yield return StartCoroutine(FadeInGroup(titleCanvasGroup, 1f));

        // --- 階段三：按鈕淡入 ---
        yield return StartCoroutine(FadeInGroup(buttonsCanvasGroup, 1f));

        // 4. 凍結遊戲與解鎖游標
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator FadeInGroup(CanvasGroup group, float duration)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 使用 unscaledDeltaTime 確保在 timeScale 為 0 也能平滑過渡（如果後面先暫停的話）
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    private void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
    }

    /// <summary>
    /// 點擊【再玩一次】按鈕
    /// </summary>
    public void Btn_RestartGame()
    {
        Time.timeScale = 1f;
        PauseManager.EnablePause();
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// 點擊【返回主選單】按鈕
    /// </summary>
    public void Btn_BackToMenu()
    {
        Time.timeScale = 1f;
        PauseManager.EnablePause();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}