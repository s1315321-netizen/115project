using UnityEngine;

public class WindowInteraction : MonoBehaviour
{
    [Header("--- 房間與窗戶物件設定 ---")]
    [Tooltip("房間背景的 SpriteRenderer")]
    public SpriteRenderer roomSpriteRenderer;
    [Tooltip("一開始有窗戶的圖片 (strart roomog)")]
    public Sprite initialWithWindowSprite;
    [Tooltip("窗戶消失後的圖片 (strart room)")]
    public Sprite afterLeaveNoWindowSprite;

    [Header("--- 窗戶物件 ---")]
    [Tooltip("原本可以調查窗戶的 GameObject")]
    public GameObject windowObject;

    [Header("--- 走到原位時的驚訝獨白設定 ---")]
    [TextArea(2, 4)]
    public string[] shockDialogue = new string[]
    {
        "Wait... Where is the window?!",
        "It's gone... The way out is completely sealed!",
        "I'm trapped here..."
    };

    private bool hasLeftRoom = false;
    private bool hasTriggeredShock = false;

    private void Start()
    {
        // 遊戲一開始：顯示有窗戶的圖片，開啟窗戶互動
        if (roomSpriteRenderer != null && initialWithWindowSprite != null)
        {
            roomSpriteRenderer.sprite = initialWithWindowSprite;
        }
        if (windowObject != null) windowObject.SetActive(true);
    }

    /// <summary>
    /// 步驟 1：當玩家離開房間走到門外 (踩到 change 門口觸發區) 時呼叫
    /// </summary>
    public void OnPlayerLeaveRoom()
    {
        if (hasLeftRoom) return;
        hasLeftRoom = true;

        // 悄悄替換背景為沒有窗戶的圖片
        if (roomSpriteRenderer != null && afterLeaveNoWindowSprite != null)
        {
            roomSpriteRenderer.sprite = afterLeaveNoWindowSprite;
        }

        // 關閉原本窗戶的互動
        if (windowObject != null) windowObject.SetActive(false);

        Debug.Log("[WindowInteraction] 玩家已離開房間，窗戶已悄悄消失。");
    }

    /// <summary>
    /// 步驟 2：當玩家再次走回原本窗戶的位置時呼叫 (綁定在原位觸發區)
    /// </summary>
    public void TriggerShockAtWindow()
    {
        // 必須是已經離開過房間，且還沒觸發過震驚對話
        if (!hasLeftRoom || hasTriggeredShock) return;
        hasTriggeredShock = true;

        if (MonologueManager.Instance != null)
        {
            MonologueManager.Instance.ShowMonologue(shockDialogue);
        }

        Debug.Log("[WindowInteraction] 玩家走回原位，觸發窗戶消失的驚訝獨白！");
    }
}