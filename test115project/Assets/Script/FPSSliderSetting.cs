using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FPSSliderSetting : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- UI References ---")]
    [Tooltip("拖入設定選單中的 Slider 組件")]
    public Slider fpsSlider;

    [Tooltip("顯示文字的 TMP_Text")]
    public TMP_Text fpsValueText;

    [Header("--- 顯示文字設定 ---")]
    [Tooltip("平時與調整完後顯示的預設標籤")]
    public string defaultLabel = "FPS";

    [Tooltip("放開拉桿後，經過幾秒還原回預設文字")]
    public float revertDelay = 1.0f;

    // 常用檔位清單 (-1 代表不鎖幀 / Unlimited)
    private readonly int[] fpsLevels = { 30, 60, 90, 120, 144, 240, -1 };
    private const string FPS_PREF_KEY = "User_Target_FPS_Slider_Index";

    private Coroutine revertRoutine;
    private bool isPointerHolding = false;

    void Start()
    {
        // 讀取先前儲存的設定，預設為 Index 1 (60 FPS)
        int savedIndex = PlayerPrefs.GetInt(FPS_PREF_KEY, 1);
        savedIndex = Mathf.Clamp(savedIndex, 0, fpsLevels.Length - 1);

        if (fpsSlider != null)
        {
            fpsSlider.wholeNumbers = true;
            fpsSlider.minValue = 0;
            fpsSlider.maxValue = fpsLevels.Length - 1;

            fpsSlider.value = savedIndex;
            fpsSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        // 開局套用設定
        ApplyTargetFPS(savedIndex, updateText: false);

        // 一開始強制顯示預設文字 "FPS"
        if (fpsValueText != null)
        {
            fpsValueText.text = defaultLabel;
        }
    }

    /// <summary>
    /// 玩家滑動拉桿時觸發
    /// </summary>
    public void OnSliderValueChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        ApplyTargetFPS(index, updateText: true);

        // 儲存設定
        PlayerPrefs.SetInt(FPS_PREF_KEY, index);
        PlayerPrefs.Save();

        // 若不是透過滑鼠拖曳（例如鍵盤左右鍵調整），觸發自動還原計時
        if (!isPointerHolding)
        {
            StartRevertTimer();
        }
    }

    /// <summary>
    /// 套用幀率限制
    /// </summary>
    private void ApplyTargetFPS(int index, bool updateText)
    {
        if (index < 0 || index >= fpsLevels.Length) return;

        int targetFPS = fpsLevels[index];

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;

        if (updateText && fpsValueText != null)
        {
            fpsValueText.text = targetFPS == -1 ? "Unlimited" : $"{targetFPS} FPS";
        }
    }

    // 滑鼠按下拉桿時：標記正在按住，停止還原計時
    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerHolding = true;
        if (revertRoutine != null)
        {
            StopCoroutine(revertRoutine);
        }
    }

    // 滑鼠放開拉桿時：標記放開，啟動還原計時
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerHolding = false;
        StartRevertTimer();
    }

    private void StartRevertTimer()
    {
        if (revertRoutine != null)
        {
            StopCoroutine(revertRoutine);
        }
        revertRoutine = StartCoroutine(RevertTextRoutine());
    }

    private IEnumerator RevertTextRoutine()
    {
        // 核心關鍵：使用 WaitForSecondsRealtime，確保選單暫停 (Time.timeScale = 0) 時依然能正常計時
        yield return new WaitForSecondsRealtime(revertDelay);

        if (fpsValueText != null)
        {
            fpsValueText.text = defaultLabel;
        }

        revertRoutine = null;
    }
}