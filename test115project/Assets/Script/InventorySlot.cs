using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;       // 格子中間的圖示
    private Item currentItem;     // 當前格子存的道具

    // 設定這個格子的道具
    public void SetItem(Item item)
    {
        currentItem = item;
        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.gameObject.SetActive(true);
        }
        else
        {
            ClearSlot();
        }
    }

    // 清空格子
    public void ClearSlot()
    {
        currentItem = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
    }

    // 當玩家點擊這個格子時呼叫
    public void OnSlotClicked()
    {
        if (currentItem != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ShowItemDetails(currentItem);
        }
    }
}