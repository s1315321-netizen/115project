using UnityEngine;

public class ExamineObject : Interactable
{
    [Header("--- 主角自言自語設定 (可設定多句) ---")]
    [TextArea(2, 4)]
    [Tooltip("主角調查此物品時說的話，Element 0 講完按 E 會講 Element 1")]
    public string[] monologueLines = new string[]
    {
        "The safe's code might be related to the paintings in the corridor."
    };

    [Tooltip("是否只觸發一次？勾選後調查過一次就不再觸發")]
    public bool triggerOnce = false;

    [Header("--- 音效設定 (選填) ---")]
    public AudioSource audioSource;
    public AudioClip examineSound;

    private bool hasTriggered = false;

    public override void Interact()
    {
        // 如果對話框正在顯示中，或者已經觸發過單次事件，就不執行
        if (MonologueManager.Instance != null && MonologueManager.Instance.IsShowing) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;

        if (audioSource != null && examineSound != null)
        {
            audioSource.PlayOneShot(examineSound);
        }

        if (MonologueManager.Instance != null)
        {
            MonologueManager.Instance.ShowMonologue(monologueLines);
        }
        else
        {
            Debug.LogWarning("[ExamineObject] 場景中找不到 MonologueManager！");
        }
    }
}