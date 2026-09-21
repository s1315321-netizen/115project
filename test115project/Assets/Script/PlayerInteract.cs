using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public TMP_Text promptText;
    public Transform promptCanvas; // 在 Inspector 裡把裝著 Text 的 Canvas 拖進來

    private Interactable currentInteractable;
    private Vector3 initialCanvasScale;

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
            promptText.text = $"[{currentInteractable.interactKey.ToString()}]";
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Interactable>() != null)
        {
            currentInteractable = null;
            promptText.text = "";
        }
    }
}