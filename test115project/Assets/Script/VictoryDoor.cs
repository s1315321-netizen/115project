using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryDoor : Interactable
{
    [Header("--- 勝利通關設定 ---")]
    [Tooltip("開啟這扇門所需的鑰匙名稱或 ID (需與 InventoryManager 一致)")]
    public string requiredKeyID = "MainDoorKey";
    [Tooltip("是否需要消耗鑰匙")]
    public bool consumeKey = true;
    [Tooltip("勝利結算畫面 UI 物件 (Panel 或 Canvas)")]
    public GameObject victoryCanvas;
    [Tooltip("提示訊息")]
    public string lockedMessage = "大門深鎖，需要一把大門鑰匙...";

    [Header("--- 轉場與音效設定 ---")]
    public CanvasGroup fadeCanvasGroup; // 若有全螢幕黑幕可以共用
    public float fadeOutDuration = 0.5f;
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;

    private bool isUnlocked = false;

    void Start()
    {
        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (isUnlocked) return;

        // 檢查背包是否有大門鑰匙
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyID))
        {
            if (consumeKey)
            {
                InventoryManager.Instance.RemoveItem(requiredKeyID);
            }

            // 啟動如同死亡畫面般的依序結算流程
            StartCoroutine(VictorySequenceRoutine());
        }
        else
        {
            Debug.Log(lockedMessage);
            PlayerInteract playerInteract = FindObjectOfType<PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.ShowPickupPrompt("需要大門鑰匙才能逃脫");
            }
        }
    }

    private IEnumerator VictorySequenceRoutine()
    {
        isUnlocked = true;
        Debug.Log("[VictoryDoor] 成功解鎖大門，啟動通關結算序列...");

        // 1. 播放開門音效
        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        // 2. 如果有設定黑幕轉場，先平滑變黑（製造逃脫的儀式感）
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // 3. 顯示勝利結算 UI
        if (victoryCanvas != null)
        {
            victoryCanvas.SetActive(true);
        }

        // 4. 凍結遊戲時間（使背景靜止）
        Time.timeScale = 0f;

        // 5. 顯示並解鎖滑鼠游標供玩家點擊按鈕
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}