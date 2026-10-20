using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; // 必須引用 Cinemachine 命名空間

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

    [Header("--- 傳送出來後的設定 ---")]
    public Transform spawnPoint;
    public Vector2 exitFacingDirection = Vector2.down;

    [Header("--- 黑屏轉場設定 ---")]
    [Tooltip("黑幕上的 CanvasGroup (若留空會自動搜尋場景中名為 FadeScreen 的物件)")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeOutDuration = 0.35f;   // 變黑花費秒數
    public float blackHoldDuration = 0.25f; // 全黑時停頓確認秒數 (確保鏡頭到位)
    public float fadeInDuration = 0.35f;    // 變亮花費秒數

    [Header("--- 音效設定 (選用) ---")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;

    public static string nextSpawnDoorID = "";
    private bool isTeleporting = false;

    void Start()
    {
        // 跨場景抵達目標門時的處理
        if (!string.IsNullOrEmpty(nextSpawnDoorID) && nextSpawnDoorID == myDoorID)
        {
            nextSpawnDoorID = "";
            StartCoroutine(SpawnPlayerHereRoutine());
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
            }
            else
            {
                Debug.Log(lockedMessage);
                if (doorAudioSource != null && doorLockedSound != null)
                {
                    doorAudioSource.PlayOneShot(doorLockedSound);
                }
                return;
            }
        }

        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        // 1. 播放開門音效
        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;
        Rigidbody2D rb = player != null ? player.GetComponent<Rigidbody2D>() : null;

        // 2. 玩家定格（關閉移動輸入、物理煞車、切回靜止動畫）
        if (movement != null)
        {
            movement.enabled = false;
            if (rb != null) rb.velocity = Vector2.zero;
            if (movement.anim != null) movement.anim.SetFloat("Speed", 0f);
        }

        // 3. 平滑變黑（在此期間主角依然在原處，不會有瞬移突兀感）
        yield return StartCoroutine(FadeRoutine(1f, fadeOutDuration));

        // ====================【進入 100% 全黑狀態】====================

        // 情況 A：跨場景切換
        if (!string.IsNullOrEmpty(targetSceneName) && targetSceneName != SceneManager.GetActiveScene().name)
        {
            nextSpawnDoorID = targetDoorID;
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        // 情況 B：同一場景傳送
        DoorPair targetDoor = FindTargetDoor(targetDoorID);
        if (targetDoor != null)
        {
            // 在黑屏中將主角瞬移到目標位置並強制重設攝影機阻尼
            targetDoor.SpawnPlayer(player);
        }
        else
        {
            Debug.LogError($"[DoorPair] 找不到目標門 ID: [{targetDoorID}]！");
        }

        // 【關鍵步驟 1】：等待 1 幀，讓 Cinemachine 在全黑掩護下完成 LateUpdate 鏡頭瞬移定位
        yield return null;

        // 【關鍵步驟 2】：停留確認秒數，給予氛圍感並確保鏡頭已咬死
        if (blackHoldDuration > 0f)
        {
            yield return new WaitForSeconds(blackHoldDuration);
        }

        // ====================【確認到位完畢，平滑變亮】====================

        yield return StartCoroutine(FadeRoutine(0f, fadeInDuration));

        // 4. 恢復玩家控制
        if (movement != null)
        {
            movement.enabled = true;
        }

        isTeleporting = false;
    }

    /// <summary>
    /// 跨場景時目標門接收主角的流程
    /// </summary>
    private IEnumerator SpawnPlayerHereRoutine()
    {
        CanvasGroup fader = GetFadeCanvasGroup();

        // 剛進入新場景瞬間，強制將黑幕設為 100% 全黑遮蔽
        if (fader != null)
        {
            fader.gameObject.SetActive(true);
            fader.alpha = 1f;
            fader.blocksRaycasts = true;
        }

        yield return null; // 等待 1 幀讓場景物件就緒

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;

        if (movement != null) movement.enabled = false;

        // 角色瞬移到位並同步重設鏡頭
        SpawnPlayer(player);

        yield return null; // 讓 Cinemachine 在黑幕中定位
        if (blackHoldDuration > 0f)
        {
            yield return new WaitForSeconds(blackHoldDuration);
        }

        // 鏡頭已定位完畢，平滑淡入
        yield return StartCoroutine(FadeRoutine(0f, fadeInDuration));

        if (movement != null) movement.enabled = true;
    }

    public void SpawnPlayer(GameObject player)
    {
        if (player == null) return;

        // 1. 計算目標位置與位移差量
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + (Vector3)exitFacingDirection;
        Vector3 delta = spawnPos - player.transform.position;

        // 2. 轉移主角座標
        player.transform.position = spawnPos;

        // 3. 強制同步 2D 物理碰撞體位置
        Physics2D.SyncTransforms();

        // 4. 【核心關鍵】：重設 Cinemachine 攝影機，清除阻尼記憶，強制瞬間瞬移
        CinemachineVirtualCamera[] vcams = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (var vcam in vcams)
        {
            if (vcam.Follow == player.transform)
            {
                // 通知目標發生瞬移
                vcam.OnTargetObjectWarped(player.transform, delta);

                // 強制斬斷上一幀平移緩衝，下一幀直接死鎖在目標身上
                vcam.PreviousStateIsValid = false;
            }
        }

        // 5. 調整手電筒朝向
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