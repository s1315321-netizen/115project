using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleDeathSystem : MonoBehaviour
{
    public static SimpleDeathSystem Instance { get; private set; }

    [Header("UI 影片組件")]
    [SerializeField] private RawImage jumpscareRawImage;  // 拖入 RawImage 物件
    [SerializeField] private VideoPlayer videoPlayer;        // 拖入 Video Player 組件

    [Header("死亡介面")]
    [SerializeField] private GameObject gameOverUI;          // 拖入 GameOverUI 物件

    [Header("場景設定")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("玩家設定")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private Transform currentCheckpoint;

    [Header("怪物設定")]
    [SerializeField] private GameObject ghostObj;
    [SerializeField] private MonoBehaviour ghostAiScript;
    [SerializeField] private Transform ghostSpawnPoint;

    [Header("音效設定")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource jumpscareAudioSource;
    [SerializeField] private AudioSource deathAmbienceSource;

    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. 初始化：將影片畫面設為完全透明 (Alpha = 0)
        SetRawImageAlpha(0f);

        // 2. 死亡面板預設關閉
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // 3. 預載影片
        if (videoPlayer != null && videoPlayer.isActiveAndEnabled)
        {
            videoPlayer.Prepare();
        }
    }

    /// 
    /// 女鬼撞到玩家時呼叫
    /// 
    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        StartCoroutine(DeathProcess());
    }

    private IEnumerator DeathProcess()
    {
        // 1. 停用玩家操作、怪物行為與音樂
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (ghostAiScript != null) ghostAiScript.enabled = false;
        if (bgmSource != null) bgmSource.Stop();

        // 2. 播放 Jumpscare 影片
        if (videoPlayer != null && jumpscareRawImage != null)
        {
            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
                while (!videoPlayer.isPrepared) yield return null;
            }

            // 瞬間顯現影片 (Alpha = 1)
            SetRawImageAlpha(1f);

            if (jumpscareAudioSource != null) jumpscareAudioSource.Play();
            videoPlayer.Play();

            // 等待影片播放完成
            float duration = (float)videoPlayer.length;
            if (duration <= 0) duration = 1.5f;
            yield return new WaitForSeconds(duration);

            // 影片播完，瞬間變回完全透明 (Alpha = 0)
            SetRawImageAlpha(0f);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        // 3. 開啟死亡畫面 (黑底、文字與按鈕)
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (deathAmbienceSource != null) deathAmbienceSource.Play();
    }

    /// 
    /// 點擊【重生】按鈕
    /// 
    public void Btn_RestartGame()
    {
        if (deathAmbienceSource != null) deathAmbienceSource.Stop();

        // 1. 關閉死亡介面，確保影片為透明狀態
        if (gameOverUI != null) gameOverUI.SetActive(false);
        SetRawImageAlpha(0f);

        // 2. 重置玩家座標與慣性
        if (playerObj != null)
        {
            if (currentCheckpoint != null)
            {
                playerObj.transform.position = currentCheckpoint.position;
            }
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 恢復玩家移動
            if (playerMovementScript != null) playerMovementScript.enabled = true;
        }

        // 3. 重置女鬼位置與 AI
        if (ghostObj != null && ghostSpawnPoint != null)
        {
            ghostObj.transform.position = ghostSpawnPoint.position;
        }
        if (ghostAiScript != null)
        {
            ghostAiScript.enabled = true;
        }

        // 4. 重啟背景音樂
        if (bgmSource != null) bgmSource.Play();

        // 5. 再次為下次突臉預載影片
        if (videoPlayer != null && videoPlayer.isActiveAndEnabled)
        {
            videoPlayer.Prepare();
        }

        isDead = false;
    }

    /// 
    /// 點擊【Home】按鈕
    /// 
    public void Btn_BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetRawImageAlpha(float alphaValue)
    {
        if (jumpscareRawImage != null)
        {
            Color c = jumpscareRawImage.color;
            c.a = alphaValue;
            jumpscareRawImage.color = c;
        }
    }

    public void SetCheckpoint(Transform newPoint)
    {
        currentCheckpoint = newPoint;
    }
}