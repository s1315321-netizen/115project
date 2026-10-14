using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal; // 支援 URP 2D 光源

public class FlashlightController : MonoBehaviour
{
    [Header("--- 光源與開關 ---")]
    public Light2D flashlightLight;           // 手電筒的 Light 2D 元件
    public KeyCode toggleKey = KeyCode.F;     // 開關手電筒按鍵
    public bool isLightOn = true;             // 當前手電筒是否開啟

    [Header("--- 電量設定 ---")]
    public float maxBattery = 100f;           // 最大電量
    public float currentBattery = 100f;       // 當前電量
    public float drainRate = 2f;              // 每秒消耗電量 (100 / 2 = 可支撐 50 秒)
    public float rechargeAmount = 100f;       // 每顆電池補充的電量

    [Header("--- 更換電池設定 ---")]
    public KeyCode reloadKey = KeyCode.R;     // 換電池按鍵
    public string batteryItemID = "Battery";  // 背包中電池的道具 ID

    [Header("--- UI 介面 (選填) ---")]
    public Slider batterySlider;              // 畫面上的電量條

    [Header("--- 低電量閃爍效果 ---")]
    public float lowBatteryThreshold = 20f;   // 低於 20% 開始閃爍
    private float defaultIntensity = 1f;      // 記錄原本光照強度

    void Start()
    {
        if (flashlightLight != null)
        {
            defaultIntensity = flashlightLight.intensity;
            flashlightLight.enabled = isLightOn;
        }

        currentBattery = maxBattery;
        UpdateUI();
    }

    void Update()
    {
        if (PauseManager.IsGamePaused) return;
        // 1. 開關手電筒 (F 鍵)
        if (Input.GetKeyDown(toggleKey))
        {
            // 只有在還有電的情況下才能打開
            if (currentBattery > 0)
            {
                isLightOn = !isLightOn;
                ApplyLightState();
            }
        }

        // 2. 更換電池 (R 鍵)
        if (Input.GetKeyDown(reloadKey))
        {
            TryReloadBattery();
        }

        // 3. 消耗電量
        if (isLightOn && currentBattery > 0)
        {
            currentBattery -= drainRate * Time.deltaTime;

            // 低電量隨機閃爍
            if (currentBattery <= lowBatteryThreshold)
            {
                HandleFlicker();
            }
            else if (flashlightLight != null)
            {
                flashlightLight.intensity = defaultIntensity;
            }

            // 電量耗盡
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                isLightOn = false;
                ApplyLightState();
                Debug.Log("手電筒沒電了！");
            }

            UpdateUI();
        }
    }

    // 切換燈光開關狀態
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

    // 嘗試從背包拿電池更換
    private void TryReloadBattery()
    {
        // 滿電時不需要更換
        if (currentBattery >= maxBattery)
        {
            Debug.Log("手電筒電量已經是滿的。");
            return;
        }

        // 檢查背包是否有電池
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(batteryItemID))
        {
            // 消耗一顆電池
            InventoryManager.Instance.RemoveItem(batteryItemID);

            // 補充手電筒電量
            currentBattery = Mathf.Min(currentBattery + rechargeAmount, maxBattery);
            isLightOn = true;
            ApplyLightState();
            UpdateUI();

            Debug.Log($"[手電筒] 已使用電池換電！目前電量: {Mathf.RoundToInt(currentBattery)}%");
        }
        else
        {
            Debug.Log("背包裡沒有電池了！");
        }
    }

    // 低電量時的恐怖閃爍感
    private void HandleFlicker()
    {
        if (flashlightLight == null) return;

        // 30% 機率輕微暗一下，模擬電壓不穩
        if (Random.value < 0.3f)
        {
            flashlightLight.intensity = defaultIntensity * Random.Range(0.2f, 0.7f);
        }
        else
        {
            flashlightLight.intensity = defaultIntensity;
        }
    }

    // 更新電量條 UI
    private void UpdateUI()
    {
        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
            batterySlider.value = currentBattery;
        }
    }
}