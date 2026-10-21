using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class DoorPair : Interactable
{
    [Header("--- 門的配對代號 ---")]
    public string myDoorID = "Room1_DoorA";
    public string targetDoorID = "Room2_DoorA";

    [Header("--- 跨場景設定 (同場景請留空) ---")]
    public string targetSceneName = "";

    [Header("--- 上鎖與背包鑰匙連動 ---")]
    public bool isLocked = false;
    [Tooltip("開啟此門所需的鑰匙名稱或 ID (需與 Item/背包系統一致)")]
    public string requiredKeyID = "RoomKey";
    [Tooltip("開門後是否消耗/扣除該鑰匙 (勾選=消耗，不勾=可重複使用)")]
    public bool consumeKeyOnUse = false;
    public string lockedMessage = "門被鎖上了，需要鑰匙...";

    [Header("--- 門的外觀切換設定 ---")]
    [Tooltip("圖 1：上鎖或未開時顯示的物件 (例如 Wall1_1)")]
    public GameObject lockedVisualObject;
    [Tooltip("圖 2：解鎖後或開啟後顯示的物件 (例如 Wall1_2)")]
    public GameObject unlockedVisualObject;

    [Header("--- 傳送出來後的設定 ---")]
    public Transform spawnPoint;
    public Vector2 exitFacingDirection = Vector2.down;

    [Header("--- 黑屏轉場設定 ---")]
    [Tooltip("黑幕上的 CanvasGroup (若留空會自動搜尋場景中名為 FadeScreen 的物件)")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeOutDuration = 0.35f;
    public float blackHoldDuration = 0.25f;
    public float fadeInDuration = 0.35f;

    [Header("--- 音效設定 (選用) ---")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;

    public static string nextSpawnDoorID = "";
    private bool isTeleporting = false;

    void Start()
    {
        // 初始化門的外觀顯示狀態
        UpdateDoorVisuals();

        // 跨場景抵達目標門時的處理
        if (!string.IsNullOrEmpty(nextSpawnDoorID) && nextSpawnDoorID == myDoorID)
        {
            nextSpawnDoorID = "";
            StartCoroutine(SpawnPlayerHereRoutine());
        }
    }

    /// <summary>
    /// 根據目前是否上鎖，切換圖 1 與圖 2 的顯示
    /// </summary>
    private void UpdateDoorVisuals()
    {
        if (lockedVisualObject != null)
        {
            lockedVisualObject.SetActive(isLocked); // 若上鎖則顯示圖 1，解鎖則隱藏
        }

        if (unlockedVisualObject != null)
        {
            unlockedVisualObject.SetActive(!isLocked); // 若解鎖則顯示圖 2，上鎖則隱藏
        }
    }

    public override void Interact()
    {
        if (isTeleporting) return;

        // 檢查是否上鎖
        if (isLocked)
        {
            if (CheckAndUseKey(requiredKeyID))
            {
                Debug.Log($"[DoorPair] 成功使用 [{requiredKeyID}] 解鎖了門！");
                isLocked = false;

                // 【核心】：成功解鎖的瞬間，立即將圖 1 切換為圖 2！
                UpdateDoorVisuals();
            }
            else
            {
                // 【修改這裡】：組合動態提示訊息（例如：需要 房間鑰匙 才能開門）
                string promptMsg = string.IsNullOrEmpty(requiredKeyID) ? lockedMessage : $"need [{requiredKeyID}]";
                Debug.Log(promptMsg);

                // 1. 如果有設定音效，播放解鎖失敗的敲門/上鎖聲
                if (doorAudioSource != null && doorLockedSound != null)
                {
                    doorAudioSource.PlayOneShot(doorLockedSound);
                }

                // 2. 讓主角頭上的提示系統（PlayerInteract）顯示提示文字
                PlayerInteract playerInteract = FindObjectOfType<PlayerInteract>();
                if (playerInteract != null)
                {
                    playerInteract.ShowPickupPrompt(promptMsg); // 可共用提示顯示方法，或依你專案的方法調整
                }

                return;
            }
        }

        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;
        Rigidbody2D rb = player != null ? player.GetComponent<Rigidbody2D>() : null;

        if (movement != null)
        {
            movement.enabled = false;
            if (rb != null) rb.velocity = Vector2.zero;
            if (movement.anim != null) movement.anim.SetFloat("Speed", 0f);
        }

        yield return StartCoroutine(FadeRoutine(1f, fadeOutDuration));

        if (!string.IsNullOrEmpty(targetSceneName) && targetSceneName != SceneManager.GetActiveScene().name)
        {
            nextSpawnDoorID = targetDoorID;
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        DoorPair targetDoor = FindTargetDoor(targetDoorID);
        if (targetDoor != null)
        {
            targetDoor.SpawnPlayer(player);
        }
        else
        {
            Debug.LogError($"[DoorPair] 找不到目標門 ID: [{targetDoorID}]！");
        }

        yield return null;

        if (blackHoldDuration > 0f)
        {
            yield return new WaitForSeconds(blackHoldDuration);
        }

        yield return StartCoroutine(FadeRoutine(0f, fadeInDuration));

        if (movement != null)
        {
            movement.enabled = true;
        }

        isTeleporting = false;
    }

    private IEnumerator SpawnPlayerHereRoutine()
    {
        CanvasGroup fader = GetFadeCanvasGroup();

        if (fader != null)
        {
            fader.gameObject.SetActive(true);
            fader.alpha = 1f;
            fader.blocksRaycasts = true;
        }

        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;

        if (movement != null) movement.enabled = false;

        SpawnPlayer(player);

        yield return null;
        if (blackHoldDuration > 0f)
        {
            yield return new WaitForSeconds(blackHoldDuration);
        }

        yield return StartCoroutine(FadeRoutine(0f, fadeInDuration));

        if (movement != null) movement.enabled = true;
    }

    public void SpawnPlayer(GameObject player)
    {
        if (player == null) return;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + (Vector3)exitFacingDirection;
        Vector3 delta = spawnPos - player.transform.position;

        player.transform.position = spawnPos;
        Physics2D.SyncTransforms();

        CinemachineVirtualCamera[] vcams = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (var vcam in vcams)
        {
            if (vcam.Follow == player.transform)
            {
                vcam.OnTargetObjectWarped(player.transform, delta);
                vcam.PreviousStateIsValid = false;
            }
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null && movement.flashlight != null)
        {
            float angle = Mathf.Atan2(exitFacingDirection.y, exitFacingDirection.x) * Mathf.Rad2Deg - 90f;
            movement.flashlight.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private DoorPair FindTargetDoor(string targetID)
    {
        DoorPair[] allDoors = FindObjectsOfType<DoorPair>();
        foreach (DoorPair door in allDoors)
        {
            if (door != this && door.myDoorID == targetID)
            {
                return door;
            }
        }
        return null;
    }

    private bool CheckAndUseKey(string keyID)
    {
        if (string.IsNullOrEmpty(keyID)) return true;

        if (InventoryManager.Instance != null)
        {
            if (InventoryManager.Instance.HasItem(keyID))
            {
                if (consumeKeyOnUse)
                {
                    InventoryManager.Instance.RemoveItem(keyID);
                }
                return true;
            }
            return false;
        }

        Debug.LogWarning("[DoorPair] 場景中未找到 InventoryManager.Instance！");
        return false;
    }

    private CanvasGroup GetFadeCanvasGroup()
    {
        if (fadeCanvasGroup != null) return fadeCanvasGroup;

        GameObject fadeObj = GameObject.Find("FadeScreen");
        if (fadeObj != null)
        {
            fadeCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
        }
        return fadeCanvasGroup;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        CanvasGroup fader = GetFadeCanvasGroup();
        if (fader == null) yield break;

        fader.gameObject.SetActive(true);
        fader.blocksRaycasts = (targetAlpha > 0f);

        float startAlpha = fader.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fader.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        fader.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            fader.blocksRaycasts = false;
        }
    }
}