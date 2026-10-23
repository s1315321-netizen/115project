using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    public TMP_Text promptText;
    public Transform promptCanvas; // 在 Inspector 裡把裝著 Text 的 Canvas 拖進來
    public Slider promptSlider;
    public Transform promptCanvas1;

    // 改為陣列，支援同一個物件上同時掛載多個 Interactable 腳本 (例如 VictorySystem + ExamineObject)
    private Interactable[] currentInteractables;
    private Collider2D currentCollider;

    private Vector3 initialCanvasScale;
    private Coroutine pickupRoutine;

    void Start()
    {
        if (promptText != null) promptText.text = "";

        if (promptCanvas != null)
        {
            initialCanvasScale = promptCanvas.localScale;
        }
    }

    void Update()
    {
        // 如果獨白對話框正在顯示中，暫停場景物件的 E 鍵互動偵測
        if (MonologueManager.Instance != null && MonologueManager.Instance.IsShowing) return;

        // 當按下互動鍵時，依序觸發該物件身上所有的 Interactable 腳本
        if (currentInteractables != null && currentInteractables.Length > 0)
        {
            if (Input.GetKeyDown(currentInteractables[0].interactKey))
            {
                foreach (Interactable item in currentInteractables)
                {
                    if (item != null && item.enabled)
                    {
                        item.Interact();
                    }
                }
            }
        }
    }

    void LateUpdate()
    {
        // 抵銷主角的左右翻轉，確保提示文字永遠維持正面
        if (promptCanvas != null)
        {
            float parentFacingX = Mathf.Sign(transform.localScale.x);
            promptCanvas.localScale = new Vector3(Mathf.Abs(initialCanvasScale.x) * parentFacingX, initialCanvasScale.y, initialCanvasScale.z);
        }
        if (promptCanvas1 != null)
        {
            float parentFacingX = Mathf.Sign(transform.localScale.x);
            promptCanvas1.localScale = new Vector3(Mathf.Abs(initialCanvasScale.x) * parentFacingX, initialCanvasScale.y, initialCanvasScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 抓取該碰撞體身上所有的 Interactable 腳本
        Interactable[] interactables = collision.GetComponents<Interactable>();
        if (interactables != null && interactables.Length > 0)
        {
            currentInteractables = interactables;
            currentCollider = collision;

            if (pickupRoutine == null && promptText != null)
            {
                promptText.text = $"[{currentInteractables[0].interactKey}]";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == currentCollider)
        {
            currentInteractables = null;
            currentCollider = null;

            if (pickupRoutine == null && promptText != null)
            {
                promptText.text = "";
            }
        }
    }

    /// <summary>
    /// 撿到物品或換電池時呼叫：把 [E] 暫時換成文字，1.5 秒後自動還原
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
        if (promptText != null)
        {
            promptText.text = itemName;
        }

        yield return new WaitForSeconds(duration);

        if (promptText != null)
        {
            if (currentInteractables != null && currentInteractables.Length > 0)
            {
                promptText.text = $"[{currentInteractables[0].interactKey}]";
            }
            else
            {
                promptText.text = "";
            }
        }

        pickupRoutine = null;
    }
}