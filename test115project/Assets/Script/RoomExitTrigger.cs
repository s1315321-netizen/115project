using UnityEngine;

public class RoomExitTrigger : MonoBehaviour
{
    [Tooltip("拖入掛有 WindowInteraction 的物件 (例如你的 Manager 或房管物件)")]
    public WindowInteraction windowInteraction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (windowInteraction != null)
            {
                // 觸發窗戶消失與背景替換
                windowInteraction.OnPlayerLeaveRoom();
            }
        }
    }
}