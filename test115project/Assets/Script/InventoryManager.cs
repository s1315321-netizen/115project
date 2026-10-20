using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public TMP_Text detailDescText;            // 顯示點選道具的說明
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

    void OnEnable()
    {
        // 註冊場景切換事件，避免跨場景時主角或 UI 參照遺失
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        FindPlayer();
        InitSlots();

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 進入新場景時重新定位主角與狀態
        FindPlayer();
        isInventoryOpen = false;
        if (inventoryUI != null) inventoryUI.SetActive(false);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        // 按 TAB 鍵切換背包開關 (若暫停選單開著則不允許開背包)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PauseManager.IsGamePaused) return;
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

        if (playerMovement == null)
        {
            FindPlayer();
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

                RefreshUI();
            }
        }
    }

    private void InitSlots()
    {
        if (slotsParent == null || slotPrefab == null) return;

        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        slotList.Clear();

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

    public void RefreshUI()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] == null) continue;

            if (i < items.Count)
            {
                slotList[i].SetItem(items[i]);
            }
            else
            {
                slotList[i].ClearSlot();
            }
        }

        ClearDetails();
    }

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
        if (detailNameText != null) detailNameText.text = "";
        if (detailDescText != null) detailDescText.text = "";
        if (detailIconImage != null) detailIconImage.gameObject.SetActive(false);
    }

    public void AddItem(Item newItem)
    {
        if (newItem == null) return;

        items.Add(newItem);
        Debug.Log($"[背包] 獲得道具: {newItem.itemName} (ID: {newItem.itemID})");

        // 通知主角頭上的提示文字顯示剛撿到的道具名稱
        PlayerInteract interact = FindObjectOfType<PlayerInteract>();
        if (interact != null)
        {
            interact.ShowPickupPrompt(newItem.itemName);
        }
    }

    /// <summary>
    /// 支援 ID 或 名稱 雙重檢查
    /// </summary>
    public bool HasItem(string idOrName)
    {
        if (string.IsNullOrEmpty(idOrName)) return true;
        return items.Exists(x => x.itemID == idOrName || x.itemName == idOrName);
    }

    /// <summary>
    /// 支援 ID 或 名稱 移除道具
    /// </summary>
    public void RemoveItem(string idOrName)
    {
        Item target = items.Find(x => x.itemID == idOrName || x.itemName == idOrName);
        if (target != null)
        {
            items.Remove(target);
            RefreshUI();
        }
    }
}