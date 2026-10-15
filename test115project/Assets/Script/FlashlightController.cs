using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("--- Light & Toggle Settings ---")]
    public Light2D flashlightLight;
    public KeyCode toggleKey = KeyCode.F;
    public bool isLightOn = true;

    [Header("--- Battery Settings ---")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float drainRate = 2f;
    public float rechargeAmount = 100f;

    [Header("--- Reload Settings ---")]
    public KeyCode reloadKey = KeyCode.R;
    public string batteryItemID = "Battery";
    public string reloadPromptMessage = "Battery Reloaded"; // 頭頂顯示的文字

    [Header("--- UI Slider & Colors ---")]
    public Slider batterySlider;
    public Image batteryFillImage;            // Slider 的 Fill 物件（可拖入或自動抓取）
    public Color fullBatteryColor = new Color(0.2f, 0.85f, 0.2f); // 綠色 (>50%)
    public Color midBatteryColor = new Color(1f, 0.85f, 0.1f);    // 黃色 (<=50%)
    public Color lowBatteryColor = new Color(0.9f, 0.2f, 0.2f);    // 紅色 (<=20%)

    [Header("--- Low Battery Flicker ---")]
    public float lowBatteryThreshold = 20f;
    private float defaultIntensity = 1f;

    private PlayerInteract playerInteract;

    void Start()
    {
        if (flashlightLight != null)
        {
            defaultIntensity = flashlightLight.intensity;
            flashlightLight.enabled = isLightOn;
        }

        // 自動抓取主角身上的 PlayerInteract 供頭頂提示使用
        playerInteract = GetComponentInParent<PlayerInteract>();
        if (playerInteract == null)
        {
            playerInteract = FindObjectOfType<PlayerInteract>();
        }

        // 自動抓取 Slider 內部的 Fill Image（若 Inspector 沒手動拉）
        if (batteryFillImage == null && batterySlider != null && batterySlider.fillRect != null)
        {
            batteryFillImage = batterySlider.fillRect.GetComponent<Image>();
        }

        currentBattery = maxBattery;
        UpdateUI();
    }

    void Update()
    {
        // 暫停時不處理操作與耗電
        if (PauseManager.IsGamePaused) return;

        // 1. 開關手電筒 (F)
        if (Input.GetKeyDown(toggleKey))
        {
            if (currentBattery > 0)
            {
                isLightOn = !isLightOn;
                ApplyLightState();
            }
        }

        // 2. 更換電池 (R)
        if (Input.GetKeyDown(reloadKey))
        {
            TryReloadBattery();
        }

        // 3. 電量消耗與閃爍
        if (isLightOn && currentBattery > 0)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= lowBatteryThreshold)
            {
                HandleFlicker();
            }
            else if (flashlightLight != null)
            {
                flashlightLight.intensity = defaultIntensity;
            }

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                isLightOn = false;
                ApplyLightState();
                Debug.Log("Flashlight battery depleted!");
            }

            UpdateUI();
        }
    }

    private void ApplyLightState()
    {
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isLightOn;
            if (isLightOn)
            {
                flashlightLight.intensity = defaultIntensity;
            }
        }
    }

    /// <summary>
    /// 嘗試從背包拿電池更換
    /// </summary>
    private void TryReloadBattery()
    {
        if (currentBattery >= maxBattery)
        {
            Debug.Log("Flashlight battery is already full.");
            return;
        }

        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(batteryItemID))
        {
            // 消耗一顆電池
            InventoryManager.Instance.RemoveItem(batteryItemID);

            // 補充手電筒電量並點亮
            currentBattery = Mathf.Min(currentBattery + rechargeAmount, maxBattery);
            isLightOn = true;
            ApplyLightState();
            UpdateUI();

            // 【頭頂提示】：在角色頭上跳出提示
            if (playerInteract != null)
            {
                playerInteract.ShowPickupPrompt(reloadPromptMessage);
            }

            Debug.Log($"[Flashlight] Battery reloaded! Current battery: {Mathf.RoundToInt(currentBattery)}%");
        }
        else
        {
            Debug.Log("No spare batteries in inventory!");
        }
    }

    /// <summary>
    /// 重生或重置時呼叫
    /// </summary>
    public void ResetFlashlight(bool turnOn = true)
    {
        currentBattery = maxBattery;
        isLightOn = turnOn;
        ApplyLightState();
        UpdateUI();
    }

    private void HandleFlicker()
    {
        if (flashlightLight == null) return;

        if (Random.value < 0.3f)
        {
            flashlightLight.intensity = defaultIntensity * Random.Range(0.2f, 0.7f);
        }
        else
        {
            flashlightLight.intensity = defaultIntensity;
        }
    }

    /// <summary>
    /// 更新電量條數值與顏色
    /// </summary>
    private void UpdateUI()
    {
        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
            batterySlider.value = currentBattery;
        }

        // 依據電量比例切換顏色
        if (batteryFillImage != null)
        {
            float percent = currentBattery / maxBattery;

            if (percent <= 0.2f)
            {
                // 剩餘 20% 以下：紅色
                batteryFillImage.color = lowBatteryColor;
            }
            else if (percent <= 0.5f)
            {
                // 剩餘 50% 以下：黃色
                batteryFillImage.color = midBatteryColor;
            }
            else
            {
                // 大於 50%：綠色
                batteryFillImage.color = fullBatteryColor;
            }
        }
    }
}