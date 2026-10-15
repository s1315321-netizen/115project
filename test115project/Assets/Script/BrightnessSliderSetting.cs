using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal; // 控制 Light 2D 必備命名空間
using TMPro;

public class BrightnessSliderSetting : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- UI References ---")]
    [Tooltip("拖入亮度 Slider")]
    public Slider brightnessSlider;

    [Tooltip("顯示亮度數值的文字 (例如 50% / Brightness)")]
    public TMP_Text brightnessValueText;

    [Header("--- 燈光組件 ---")]
    [Tooltip("場景中的 Global Light 2D (留空則自動在場景中搜尋)")]
    public Light2D globalLight;

    [Header("--- 亮度範圍對應 ---")]
    [Tooltip("0% 對應的 Light2D 強度")]
    public float minIntensity = 0.01f;

    [Tooltip("100% 對應的 Light2D 強度")]
    public float maxIntensity = 0.35f;

    [Header("--- 顯示設定 ---")]
    public string defaultLabel = "Brightness";
    public float revertDelay = 1.0f;
    public float defaultPercent = 50f; // 首次遊玩預設 50%

    private const string BRIGHTNESS_PREF_KEY = "User_Brightness_Percent";
    private Coroutine revertRoutine;
    private bool isPointerHolding = false;

    void Awake()
    {
        // 場景載入時立即套用儲存的亮度
        FindGlobalLightIfNeeded();
        float savedPercent = PlayerPrefs.GetFloat(BRIGHTNESS_PREF_KEY, defaultPercent);
        ApplyBrightness(savedPercent, updateText: false);
    }

    void OnEnable()
    {
        // 每次打開設定面板時，重新確認燈光綁定與數值
        FindGlobalLightIfNeeded();
    }

    void Start()
    {
        float savedPercent = PlayerPrefs.GetFloat(BRIGHTNESS_PREF_KEY, defaultPercent);

        if (brightnessSlider != null)
        {
            // 設定拉桿範圍為 0 ~ 100 整數
            brightnessSlider.wholeNumbers = true;
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 100f;

            brightnessSlider.value = savedPercent;
            brightnessSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        if (brightnessValueText != null)
        {
            brightnessValueText.text = defaultLabel;
        }
    }

    /// <summary>
    /// 玩家滑動拉桿時觸發
    /// </summary>
    public void OnSliderValueChanged(float value)
    {
        ApplyBrightness(value, updateText: true);

        // 儲存設定至本機
        PlayerPrefs.SetFloat(BRIGHTNESS_PREF_KEY, value);
        PlayerPrefs.Save();

        if (!isPointerHolding)
        {
            StartRevertTimer();
        }
    }

    /// <summary>
    /// 核心換算：將 0~100% 線性對應至 minIntensity (0.01) ~ maxIntensity (0.35)
    /// </summary>
    private void ApplyBrightness(float percent, bool updateText)
    {
        FindGlobalLightIfNeeded();

        // 0% -> 0.0, 100% -> 1.0
        float t = Mathf.Clamp01(percent / 100f);

        // 線性插值計算強度
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        if (globalLight != null)
        {
            globalLight.intensity = targetIntensity;
        }

        if (updateText && brightnessValueText != null)
        {
            brightnessValueText.text = $"{Mathf.RoundToInt(percent)}%";
        }
    }

    /// <summary>
    /// 自動搜尋場景中 Light Type 為 Global 的 Light 2D
    /// </summary>
    private void FindGlobalLightIfNeeded()
    {
        if (globalLight == null)
        {
            Light2D[] lights = FindObjectsOfType<Light2D>();
            foreach (var l in lights)
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    globalLight = l;
                    break;
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerHolding = true;
        if (revertRoutine != null) StopCoroutine(revertRoutine);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerHolding = false;
        StartRevertTimer();
    }

    private void StartRevertTimer()
    {
        if (revertRoutine != null) StopCoroutine(revertRoutine);
        revertRoutine = StartCoroutine(RevertTextRoutine());
    }

    private IEnumerator RevertTextRoutine()
    {
        // 暫停時 (Time.timeScale = 0) 依然順暢倒數 1 秒
        yield return new WaitForSecondsRealtime(revertDelay);

        if (brightnessValueText != null)
        {
            brightnessValueText.text = defaultLabel;
        }
        revertRoutine = null;
    }
}