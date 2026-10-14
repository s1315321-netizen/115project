using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public TMP_Text promptText;
    public Transform promptCanvas; // 在 Inspector 裡把裝著 Text 的 Canvas 拖進來

    private Interactable currentInteractable;
    private Vector3 initialCanvasScale;
    private Coroutine pickupRoutine; // 用於控制撿取提示的計時協程

    void Start()
    {
        if (promptText != null) promptText.text = "";

        // 紀錄 Canvas 一開始的正確比例 (例如 0.01)
        if (promptCanvas != null)
        {
            initialCanvasScale = promptCanvas.localScale;
        }
    }

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(currentInteractable.interactKey))
        {
            currentInteractable.Interact();
        }
    }

    void LateUpdate()
    {
        // 【核心：抵銷主角的左右翻轉，確保字體永遠是正的】
        if (promptCanvas != null)
        {
            // 抓取主角當前的朝向 (1 或 -1)
            float parentFacingX = Mathf.Sign(transform.localScale.x);

            // 主角變 -1 時，Canvas 也乘上 -1，負負得正維持正面
            promptCanvas.localScale = new Vector3(Mathf.Abs(initialCanvasScale.x) * parentFacingX, initialCanvasScale.y, initialCanvasScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null)
        {
            currentInteractable = interactable;

            // 如果當前沒有正在顯示撿取提示，才立即顯示 [E]
            if (pickupRoutine == null && promptText != null)
            {
                promptText.text = $"[{currentInteractable.interactKey}]";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;

            // 重要防呆：撿起物品後地上物件會消失並觸發 Exit 判定，
            // 只有在「沒有正在顯示撿取提示」時才清空，避免剛跳出的道具名稱被瞬間洗掉
            if (pickupRoutine == null && promptText != null)
            {
                promptText.text = "";
            }
        }
    }

    /// <summary>
    /// 當撿到物品時呼叫：把 [E] 暫時換成道具名稱，1.5 秒後自動還原
    /// </summary>
    public void ShowPickupPrompt(string itemName, float duration = 1.5f)
    {
        if (pickupRoutine != null)
        {
            StopCoroutine(pickupRoutine);
        }
        pickupRoutine = StartCoroutine(PickupPromptRoutine(itemName, duration));
    }

    private IEnumerator PickupPromptRoutine(string itemName, float duration)
    {
        // 1. 將頭頂文字替換為道具名稱 (前面加 + 號更具辨識度)
        if (promptText != null)
        {
            promptText.text = $"+ {itemName}";
        }

        // 2. 停留 1.5 秒
        yield return new WaitForSeconds(duration);

        // 3. 還原文字：若主角身邊還有其他可互動對象則還原成 [按鍵]，否則清空
        if (promptText != null)
        {
            if (currentInteractable != null)
            {
                promptText.text = $"[{currentInteractable.interactKey}]";
            }
            else
            {
                promptText.text = "";
            }
        }

        pickupRoutine = null;
    }
}