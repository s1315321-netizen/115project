using System.Collections;
using UnityEngine;
using TMPro;

public class MonologueManager : MonoBehaviour
{
    public static MonologueManager Instance { get; private set; }

    [Header("--- UI 組件 ---")]
    [Tooltip("對話框的 CanvasGroup，用來控制淡入淡出")]
    public CanvasGroup dialogueCanvasGroup;
    [Tooltip("顯示獨白文字的 TextMeshPro 組件")]
    public TMP_Text monologueText;
    [Tooltip("提示「按 E 繼續」的小圖示或文字 (選填)")]
    public GameObject continueIndicator;

    [Header("--- 演出參數 ---")]
    [Tooltip("每個字出現的間隔秒數 (打字機效果)")]
    public float typingSpeed = 0.04f;
    [Tooltip("淡入淡出的時間")]
    public float fadeDuration = 0.15f;

    // 判斷目前是否正在對話中（供 PlayerInteract 檢查，避免重複觸發）
    public bool IsShowing { get; private set; } = false;

    private Coroutine currentRoutine;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0f;
            dialogueCanvasGroup.blocksRaycasts = false;
        }
        if (monologueText != null) monologueText.text = "";
        if (continueIndicator != null) continueIndicator.SetActive(false);
    }

    /// <summary>
    /// 顯示單句主角自言自語
    /// </summary>
    public void ShowMonologue(string line)
    {
        ShowMonologue(new string[] { line });
    }

    /// <summary>
    /// 顯示多句連續的主角自言自語
    /// </summary>
    public void ShowMonologue(string[] lines)
    {
        if (IsShowing || lines == null || lines.Length == 0) return;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(MonologueSequenceRoutine(lines));
    }

    private IEnumerator MonologueSequenceRoutine(string[] lines)
    {
        IsShowing = true;
        FreezePlayer(true);

        if (monologueText != null) monologueText.text = "";
        if (continueIndicator != null) continueIndicator.SetActive(false);

        // 1. 淡入對話框
        yield return StartCoroutine(FadeCanvasGroup(1f));

        // 2. 逐句播放對話
        foreach (string line in lines)
        {
            if (monologueText != null) monologueText.text = "";
            if (continueIndicator != null) continueIndicator.SetActive(false);

            // 重要：先等待一幀，消化掉剛才觸發互動時按下的那個 E 鍵
            yield return null;

            // --- 打字機階段 ---
            if (typingSpeed <= 0f)
            {
                if (monologueText != null) monologueText.text = line;
            }
            else
            {
                for (int i = 0; i < line.Length; i++)
                {
                    if (monologueText != null) monologueText.text += line[i];

                    float timer = 0f;
                    bool skipTyping = false;
                    while (timer < typingSpeed)
                    {
                        timer += Time.unscaledDeltaTime;
                        // 打字途中按 E 或空白鍵，直接顯示整句話
                        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                        {
                            skipTyping = true;
                            break;
                        }
                        yield return null;
                    }

                    if (skipTyping)
                    {
                        if (monologueText != null) monologueText.text = line;
                        yield return null; // 消化掉跳過打字的那次按鍵
                        break;
                    }
                }
            }

            // 顯示「請按繼續」的小提示
            if (continueIndicator != null) continueIndicator.SetActive(true);

            // --- 等待玩家按下 E 或空白鍵進入下一句 / 關閉 ---
            while (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
        }

        if (continueIndicator != null) continueIndicator.SetActive(false);

        // 3. 淡出對話框
        yield return StartCoroutine(FadeCanvasGroup(0f));

        if (monologueText != null) monologueText.text = "";

        // 4. 恢復主角移動
        FreezePlayer(false);

        // 重要：等待一幀再解除 IsShowing，防止關閉對話框的瞬間又立刻觸發同一個物品！
        yield return null;
        IsShowing = false;
        currentRoutine = null;
    }

    private void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.enabled = false;
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.velocity = Vector2.zero;
                    if (playerMovement.anim != null) playerMovement.anim.SetFloat("Speed", 0f);
                }
            }
        }
        else
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
                playerMovement = null;
            }
        }
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        if (dialogueCanvasGroup == null) yield break;

        float startAlpha = dialogueCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = targetAlpha;
    }
}