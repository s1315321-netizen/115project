using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public TMP_Text promptText;
    public Transform promptCanvas; // 在 Inspector 裡把裝著 Text 的 Canvas 拖進來

    private Interactable currentInteractable;
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
        if (currentInteractable != null && Input.GetKeyDown(currentInteractable.interactKey))
        {
            currentInteractable.Interact();
        }
    }

    void LateUpdate()
    {
        // 抵銷主角的左右翻轉，確保文字永遠維持正面
        if (promptCanvas != null)
        {
            float parentFacingX = Mathf.Sign(transform.localScale.x);
            promptCanvas.localScale = new Vector3(Mathf.Abs(initialCanvasScale.x) * parentFacingX, initialCanvasScale.y, initialCanvasScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null)
        {
            currentInteractable = interactable;

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
        // 移除前綴 '+'，直接顯示純文字
        if (promptText != null)
        {
            promptText.text = itemName;
        }

        // 停留 1.5 秒
        yield return new WaitForSeconds(duration);

        // 還原提示文字
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