using UnityEngine;

public class TeleportDoor : Interactable
{
    [Header("傳送目標點")]
    public Transform destination; // 在場景中放一個 Empty Object 當作目的地並拖進來

    public override void Interact()
    {
        Debug.Log("傳送到下一個房間！");
        // 找到主角並將其位置設為目的地的位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && destination != null)
        {
            player.transform.position = destination.position;
        }
    }
}