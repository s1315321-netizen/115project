using UnityEngine;

// 繼承自 Interactable
public class PickupItem : Interactable
{
    public override void Interact()
    {
        Debug.Log("撿起道具了！");
        // 這裡可以加入將道具放入背包的邏輯

        Destroy(gameObject); // 撿起後刪除場景上的物件
    }
}