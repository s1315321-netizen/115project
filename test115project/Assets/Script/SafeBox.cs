using System.Collections;
using UnityEngine;
using TMPro;

public class SafeBox : Interactable
{
    [Header("--- UI 設定 ---")]
    public GameObject safeUI;
    public TMP_Text codeDisplayText;

    [Header("--- 密碼設定 ---")]
    public string correctCode = "2486";
    private string currentInput = "";

    [Header("--- 解鎖後的獎勵 ---")]
    [Tooltip("勾選後：密碼正確時直接把鑰匙加入背包；取消勾選：只在地上/保險箱顯示實體鑰匙")]
    public bool giveDirectlyToInventory = true;

    [Tooltip("給予的道具資料 (填入鑰匙的 ID、名稱、Icon、說明)")]
    public Item rewardItemData;

    [Tooltip("若想在地上/保險箱顯示實體鑰匙物件，可拖進來 (需掛載 PickupItem)")]
    public GameObject physicalKeyObject;

    [Header("--- 保險箱外觀切換 (選填) ---")]
    public SpriteRenderer safeRenderer;
    public Sprite openedSprite;

    private bool isOpened = false;
    private bool isInteracting = false;
    private PlayerMovement playerMovement;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        UpdateDisplay();

        // 實體鑰匙一開始先隱藏
        if (physicalKeyObject != null)
        {
            physicalKeyObject.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (isOpened)
        {
            Debug.Log("保險箱已經被打開過了。");
            return;
        }

        isInteracting = !isInteracting;
        ToggleSafeUI(isInteracting);
    }

    public void ToggleSafeUI(bool open)
    {
        isInteracting = open;

        if (safeUI != null)
        {
            safeUI.SetActive(open);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = !open;

            if (open)
            {
                Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;

                if (playerMovement.anim != null)
                {
                    playerMovement.anim.SetFloat("Speed", 0f);
                }
            }
        }

        if (open)
        {
            currentInput = "";
            UpdateDisplay();
        }
    }

    public void PressNumber(string number)
    {
        if (currentInput.Length < 4)
        {
            currentInput += number;
            UpdateDisplay();

            if (currentInput.Length == 4)
            {
                StartCoroutine(CheckCodeRoutine());
            }
        }
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
    }

    private IEnumerator CheckCodeRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (currentInput == correctCode)
        {
            // 【密碼正確】
            codeDisplayText.text = "<color=green>OPEN</color>";
            isOpened = true;

            yield return new WaitForSeconds(0.8f);

            // 1. 保險箱外觀切換為打開狀態
            if (safeRenderer != null && openedSprite != null)
            {
                safeRenderer.sprite = openedSprite;
            }

            // 2. 發放鑰匙獎勵
            if (giveDirectlyToInventory)
            {
                // 方式 A：直接放進背包
                if (rewardItemData != null && InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(rewardItemData);
                    Debug.Log($"[保險箱] 已將 {rewardItemData.itemName} 放入背包！");
                }
            }
            else
            {
                // 方式 B：在地上/保險箱中顯示實體鑰匙，讓玩家按 E 撿走
                if (physicalKeyObject != null)
                {
                    physicalKeyObject.SetActive(true);
                }
            }

            // 關閉保險箱 UI，恢復玩家移動
            ToggleSafeUI(false);
        }
        else
        {
            // 【密碼錯誤】
            codeDisplayText.text = "<color=red>ERROR</color>";
            yield return new WaitForSeconds(0.8f);

            currentInput = "";
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (codeDisplayText != null)
        {
            string display = "";
            for (int i = 0; i < 4; i++)
            {
                if (i < currentInput.Length)
                {
                    display += currentInput[i] + " ";
                }
                else
                {
                    display += "_ ";
                }
            }
            codeDisplayText.text = display.TrimEnd();
        }
    }
}