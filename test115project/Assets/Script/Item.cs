using UnityEngine;

[System.Serializable]
public class Item
{
    [Tooltip("道具唯一代號 (例如: Key_101, Battery)")]
    public string itemID;

    [Tooltip("在背包顯示的名稱")]
    public string itemName;

    [Tooltip("背包裡的圖示 (Sprite)")]
    public Sprite icon;

    [TextArea(3, 5)]
    [Tooltip("點擊道具時顯示的說明 (例如紙條上的字)")]
    public string description;
}