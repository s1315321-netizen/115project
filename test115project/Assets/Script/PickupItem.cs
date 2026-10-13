using UnityEngine;

public class PickupItem : Interactable
{
    [Header("道具設定")]
    public Item itemData; // 在 Inspector 設定這個道具的 ID、名字、圖示和說明

    [Tooltip("撿起時的提示訊息")]
    public string pickupMessage = "撿到了道具！";

    public override void Interact()
    {
        if (InventoryManager.Instance != null)
        {
            // 放入背包
            InventoryManager.Instance.AddItem(itemData);
            Debug.Log($"{pickupMessage}: {itemData.itemName}");

            // 銷毀地上的物件
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("場景中缺少 InventoryManager 物件！");
        }
    }
}