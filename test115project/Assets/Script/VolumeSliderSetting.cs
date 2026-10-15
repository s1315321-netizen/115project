using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class VolumeSliderSetting : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- UI References ---")]
    [Tooltip("拖入音量 Slider")]
    public Slider volumeSlider;

    [Tooltip("顯示音量數值的文字 (例如 80% / Volume)")]
    public TMP_Text volumeValueText;

    [Header("--- 顯示設定 ---")]
    public string defaultLabel = "Volume";
    public float revertDelay = 1.0f;

    private const string VOLUME_PREF_KEY = "User_Master_Volume";
    private Coroutine revertRoutine;
    private bool isPointerHolding = false;

    void Start()
    {
        // 讀取先前儲存的音量 (預設為 0.8 即 80%)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_PREF_KEY, 0.8f);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        ApplyVolume(savedVolume, updateText: false);

        if (volumeValueText != null)
        {
            volumeValueText.text = defaultLabel;
        }
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolume(value, updateText: true);

        // 儲存設定
        PlayerPrefs.SetFloat(VOLUME_PREF_KEY, value);
        PlayerPrefs.Save();

        if (!isPointerHolding)
        {
            StartRevertTimer();
        }
    }

    private void ApplyVolume(float value, bool updateText)
    {
        // 直接控制 Unity 全域音量
        AudioListener.volume = value;

        if (updateText && volumeValueText != null)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            volumeValueText.text = $"{percent}%";
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
        yield return new WaitForSecondsRealtime(revertDelay);

        if (volumeValueText != null)
        {
            volumeValueText.text = defaultLabel;
        }
        revertRoutine = null;
    }
}