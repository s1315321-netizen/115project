using UnityEngine;

public class RoomChangeTrigger : MonoBehaviour
{
    [Header("--- 房間背景更換設定 ---")]
    [Tooltip("拖入起始房間的 SpriteRenderer 物件")]
    public SpriteRenderer roomSpriteRenderer;

    [Tooltip("一開始有窗戶的圖片 (strart roomog)")]
    public Sprite initialWithWindowSprite;

    [Tooltip("離開後窗戶消失的圖片 (strart room)")]
    public Sprite afterLeaveNoWindowSprite;

    [Header("--- 額外隱藏物件 (選填) ---")]
    [Tooltip("如果窗戶還有獨立的互動碰撞框或光源，離開後一併關閉")]
    public GameObject[] objectsToDisableOnLeave;

    [Header("--- 音效設定 (選填) ---")]
    [Tooltip("離開時是否播放低沉的異變音效")]
    public AudioSource audioSource;
    public AudioClip changeSound;

    private bool hasChanged = false;

    private void Start()
    {
        // 遊戲一開始，確保房間顯示的是「有窗戶」的第二張圖
        if (roomSpriteRenderer != null && initialWithWindowSprite != null)
        {
            roomSpriteRenderer.sprite = initialWithWindowSprite;
        }
    }

    // 當玩家走到門外踩到這個 Trigger 時觸發
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasChanged) return;

        if (other.CompareTag("Player"))
        {
            TriggerRoomChange();
        }
    }

    public void TriggerRoomChange()
    {
        if (hasChanged) return;
        hasChanged = true;

        // 1. 換成第一張圖（窗戶與地上碎玻璃消失）
        if (roomSpriteRenderer != null && afterLeaveNoWindowSprite != null)
        {
            roomSpriteRenderer.sprite = afterLeaveNoWindowSprite;
        }

        // 2. 如果窗戶原本有掛 ExamineObject 調查框或 2D 光源，一併關掉
        if (objectsToDisableOnLeave != null)
        {
            foreach (GameObject obj in objectsToDisableOnLeave)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 3. 播放音效 (如果有設定的話)
        if (audioSource != null && changeSound != null)
        {
            audioSource.PlayOneShot(changeSound);
        }

        Debug.Log("[RoomChangeTrigger] 玩家已離開起始房間，窗戶已消失！");
    }
}