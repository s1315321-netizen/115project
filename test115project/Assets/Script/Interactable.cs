using UnityEngine;

// 任何需要被互動的物件（門、道具）都要繼承這個腳本
public abstract class Interactable : MonoBehaviour
{
    [Header("互動按鍵設定")]
    public KeyCode interactKey = KeyCode.E; // 預設互動鍵為 E，可以在 Inspector 更改為 F 等等

    // 抽象方法：讓不同的子類別（撿道具、傳送門）自己決定按下去要發生什麼事
    public abstract void Interact();
}