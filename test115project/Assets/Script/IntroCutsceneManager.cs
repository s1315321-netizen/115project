using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroCutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public struct CutsceneSlide
    {
        [Header("--- 圖片與文字 ---")]
        public CanvasGroup slideCanvasGroup;
        [TextArea(2, 4)]
        public string slideTextContent;
    }

    [Header("--- 過場設定 ---")]
    public CutsceneSlide[] slides;
    public TMP_Text subtitleText;
    public float typingSpeed = 0.04f;
    public float fadeDuration = 1.0f;
    public string gameplaySceneName = "room1";

    [Header("--- 全局黑幕轉場 (選填) ---")]
    [Tooltip("可選：放在最上層的全黑 Panel 的 CanvasGroup，用來做最後切換場景的淡出")]
    public CanvasGroup globalBlackScreen;

    private bool isSkipping = false;

    private void Start()
    {
        // 初始化所有圖與文字
        foreach (var slide in slides)
        {
            if (slide.slideCanvasGroup != null)
            {
                slide.slideCanvasGroup.alpha = 0f;
                slide.slideCanvasGroup.gameObject.SetActive(false);
            }
        }
        if (subtitleText != null) subtitleText.text = "";

        if (globalBlackScreen != null)
        {
            globalBlackScreen.gameObject.SetActive(true);
            globalBlackScreen.alpha = 1f;
            // 遊戲剛開始時，從全黑慢慢淡入顯示第一張圖
            StartCoroutine(FadeGlobal(0f, 1f));
        }

        StartCoroutine(PlayCutsceneSequence());
    }

    private void Update()
    {
        // 允許按 Esc、Enter 或空白鍵直接跳過過場進入遊戲
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) && !isSkipping)
        {
            isSkipping = true;
            StartCoroutine(SkipAndLoadScene());
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i].slideCanvasGroup == null) continue;

            CanvasGroup currentGroup = slides[i].slideCanvasGroup;
            string content = slides[i].slideTextContent;

            currentGroup.gameObject.SetActive(true);

            // 1. 圖片淡入
            yield return StartCoroutine(Fade(currentGroup, 0f, 1f, fadeDuration));

            // 2. 打字機顯示對應文字
            if (subtitleText != null) subtitleText.text = "";
            foreach (char letter in content.ToCharArray())
            {
                if (isSkipping) yield break;
                if (subtitleText != null) subtitleText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            // 3. 停留觀看 3 秒
            float timer = 0f;
            while (timer < 3.0f)
            {
                if (isSkipping) yield break;
                timer += Time.deltaTime;
                yield return null;
            }

            // 4. 若不是最後一張，淡出圖片與文字
            if (i < slides.Length - 1)
            {
                yield return StartCoroutine(Fade(currentGroup, 1f, 0f, fadeDuration));
                currentGroup.gameObject.SetActive(false);
                if (subtitleText != null) subtitleText.text = "";
            }
        }

        // 播完最後三張圖後，平滑淡出至全黑再載入遊戲
        yield return StartCoroutine(SkipAndLoadScene());
    }

    private IEnumerator Fade(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (isSkipping) yield break;
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }

    private IEnumerator FadeGlobal(float targetAlpha, float duration)
    {
        if (globalBlackScreen == null) yield break;
        float startAlpha = globalBlackScreen.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            globalBlackScreen.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        globalBlackScreen.alpha = targetAlpha;
    }

    private IEnumerator SkipAndLoadScene()
    {
        isSkipping = true;

        // 如果有設定全黑幕，淡出至全黑 (Alpha -> 1)
        if (globalBlackScreen != null)
        {
            yield return StartCoroutine(FadeGlobal(1f, 0.8f));
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}