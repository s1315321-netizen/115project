using System.Collections;
using UnityEngine;
using TMPro; // 引入 TextMeshPro

public class SafeBox : Interactable
{
    [Header("--- UI 設定 ---")]
    public GameObject safeUI;          // 保險箱操作介面的 Canvas/Panel
    public TMP_Text codeDisplayText;   // 顯示目前輸入數字的文字 (例如 "248_")

    [Header("--- 密碼設定 ---")]
    public string correctCode = "2486"; // 正確密碼
    private string currentInput = "";   // 玩家當前輸入的字串

    [Header("--- 解鎖後的獎勵/外觀 ---")]
    public GameObject rewardItem;       // 解鎖後掉出/顯示的物品 (例如鑰匙)
    public SpriteRenderer safeRenderer; // 保險箱本體的圖片
    public Sprite openedSprite;         // 保險箱打開後的圖片 (選填)

    private bool isOpened = false;      // 是否已經解鎖過
    private bool isInteracting = false; // 是否正在操作保險箱
    private PlayerMovement playerMovement;

    void Start()
    {
        // 自動找到主角與移動腳本
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        UpdateDisplay();
    }

    // 繼承自 Interactable：靠近按 E 觸發
    public override void Interact()
    {
        // 如果已經打開過了，就不用再開密碼盤
        if (isOpened)
        {
            Debug.Log("保險箱已經是打開的狀態了。");
            return;
        }

        // 切換介面開關
        isInteracting = !isInteracting;
        ToggleSafeUI(isInteracting);
    }

    // 開關 UI 與鎖定/解鎖角色
    public void ToggleSafeUI(bool open)
    {
        isInteracting = open;

        if (safeUI != null)
        {
            safeUI.SetActive(open);
        }

        // 鎖定或解鎖角色移動（跟 ViewPainting 完全相同）
        if (playerMovement != null)
        {
            playerMovement.enabled = !open;

            if (open)
            {
                // 打開瞬間完全煞車
                playerMovement.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                if (playerMovement.anim != null)
                {
                    playerMovement.anim.SetFloat("Speed", 0f);
                }
            }
        }

        // 打開介面時清空之前的輸入
        if (open)
        {
            currentInput = "";
            UpdateDisplay();
        }
    }

    // --- 給 UI 數字按鈕點擊呼叫的函式 (0~9) ---
    public void PressNumber(string number)
    {
        // 限制最多只能輸入 4 位數
        if (currentInput.Length < 4)
        {
            currentInput += number;
            UpdateDisplay();

            // 輸入滿 4 位數時自動判定
            if (currentInput.Length == 4)
            {
                StartCoroutine(CheckCodeRoutine());
            }
        }
    }

    // --- 清除輸入 (C 鍵) ---
    public void ClearInput()
    {
        currentInput = "";
        UpdateDisplay();
    }

    // 檢查密碼協程
    private IEnumerator CheckCodeRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (currentInput == correctCode)
        {
            // 【密碼正確】
            codeDisplayText.text = "OPEN!";
            isOpened = true;

            yield return new WaitForSeconds(0.8f);

            // 1. 換成打開的圖片 (如果有設定)
            if (safeRenderer != null && openedSprite != null)
            {
                safeRenderer.sprite = openedSprite;
            }

            // 2. 顯示裡面的獎勵 (例如鑰匙)
            if (rewardItem != null)
            {
                rewardItem.SetActive(true);
            }

            Debug.Log("保險箱解鎖成功！");
            ToggleSafeUI(false); // 自動關閉保險箱介面
        }
        else
        {
            // 【密碼錯誤】
            codeDisplayText.text = "ERROR!";
            yield return new WaitForSeconds(0.8f);

            currentInput = "";
            UpdateDisplay();
        }
    }

    // 更新螢幕上的顯示文字
    private void UpdateDisplay()
    {
        if (codeDisplayText != null)
        {
            string display = "";

            // 固定跑 4 格：有輸入就顯示該數字，沒輸入就補底線
            for (int i = 0; i < 4; i++)
            {
                if (i < currentInput.Length)
                {
                    display += currentInput[i] + " "; // 顯示輸入的數字並加空格
                }
                else
                {
                    display += "_ "; // 顯示底線並加空格
                }
            }

            codeDisplayText.text = display.TrimEnd();
        }
    }
}