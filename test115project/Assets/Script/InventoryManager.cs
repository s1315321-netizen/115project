using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("--- 背包內部資料 ---")]
    public List<Item> items = new List<Item>();

    [Header("--- UI 介面設定 ---")]
    public GameObject inventoryUI;             // 整體背包的 Panel
    public Transform slotsParent;              // 放置所有格子的容器 (例如 SlotGrid)
    public GameObject slotPrefab;              // 單個格子的 Prefab
    public int maxSlotCount = 12;              // 背包總格子數

    [Header("--- 道具詳細資訊欄 (選填) ---")]
    public TMP_Text detailNameText;            // 顯示點選道具的名稱
    public TMP_Text detailDescText;            // 顯示點選道具的說明 (例如紙條內容)
    public Image detailIconImage;              // 顯示點選道具的放大圖

    private bool isInventoryOpen = false;
    private PlayerMovement playerMovement;
    private List<InventorySlot> slotList = new List<InventorySlot>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 抓取主角
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        // 初始化產生固定數量的格子
        InitSlots();

        // 預設關閉背包 UI
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }

    void Update()
    {
        // 按 TAB 鍵切換背包開關
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }

        // 鎖定/解鎖角色移動與煞車
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerMovement = player.GetComponent<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = !isInventoryOpen;

            if (isInventoryOpen)
            {
                // 打開背包時強制煞車定格
                Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;

                if (playerMovement.anim != null)
                {
                    playerMovement.anim.SetFloat("Speed", 0f);
                }

                // 重新刷新介面格子
                RefreshUI();
            }
        }
    }

    // 動態生成所有格子
    private void InitSlots()
    {
        if (slotsParent == null || slotPrefab == null) return;

        // 清空原本容器內的殘留物件
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        slotList.Clear();

        // 產生指定數量的格子
        for (int i = 0; i < maxSlotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slotList.Add(slot);
                slot.ClearSlot();
            }
        }
    }

    // 重新繪製格子裡的道具
    public void RefreshUI()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < items.Count)
            {
                slotList[i].SetItem(items[i]);
            }
            else
            {
                slotList[i].ClearSlot();
            }
        }

        // 清空說明欄
        ClearDetails();
    }

    // 點擊格子時顯示詳細資訊
    public void ShowItemDetails(Item item)
    {
        if (item == null) return;

        if (detailNameText != null) detailNameText.text = item.itemName;
        if (detailDescText != null) detailDescText.text = item.description;
        if (detailIconImage != null)
        {
            detailIconImage.sprite = item.icon;
            detailIconImage.gameObject.SetActive(item.icon != null);
        }
    }

    private void ClearDetails()
    {
        if (detailNameText != null) detailNameText.text = "None";
        if (detailDescText != null) detailDescText.text = "None";
        if (detailIconImage != null) detailIconImage.gameObject.SetActive(false);
    }

    // --- 給其他系統（例如門 DoorPair）呼叫的判定方法 ---
    public void AddItem(Item newItem)
    {
        items.Add(newItem);
        Debug.Log($"[背包] 獲得道具: {newItem.itemName} (ID: {newItem.itemID})");
    }

    public bool HasItem(string id)
    {
        return items.Exists(x => x.itemID == id);
    }

    public void RemoveItem(string id)
    {
        Item target = items.Find(x => x.itemID == id);
        if (target != null)
        {
            items.Remove(target);
            RefreshUI();
        }
    }
}