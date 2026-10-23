using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoomFadeIn : MonoBehaviour
{
    [Header("--- 房間開局淡入設定 ---")]
    [Tooltip("拖入畫面最上層全黑 Image 的 CanvasGroup")]
    public CanvasGroup fadeCanvasGroup;
    [Tooltip("淡入所需要的時間 (秒)")]
    public float fadeDuration = 1.0f;

    void Start()
    {
        if (fadeCanvasGroup != null)
        {
            // 遊戲一開始強制設為全黑 (Alpha = 1)
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;

            // 開始平滑淡入到透明 (Alpha = 0)
            StartCoroutine(FadeInRoutine());
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        // 淡入結束後關閉射線阻擋與物件，避免擋住玩家點擊遊戲 UI
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);
    }
}   