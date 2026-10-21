using UnityEngine;

public class PickupItem : Interactable
{
    [Header("--- 道具資料 ---")]
    public Item itemData;

    [Header("--- 提示文字設定 ---")]
    public string interactMessage = "按 E 拾取";

    // 當玩家靠近並按下 E 鍵時觸發
    public override void Interact()
    {
        if (itemData != null && InventoryManager.Instance != null)
        {
            // 1. 將道具加入背包
            InventoryManager.Instance.AddItem(itemData);

            // 2. 播放音效或銷毀世界物件
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[PickupItem] 缺少 itemData 或 InventoryManager.Instance 不存在！");
        }
    }
}