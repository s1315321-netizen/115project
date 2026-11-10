using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameSaveData
{
    public string slotName;
    public string savedAt;
    public string sceneName;
    public float playerX;
    public float playerY;
    public List<string> itemIDs = new List<string>();
    public List<string> objectStates = new List<string>();
}

/// <summary>三個本機存檔槽；場景物件以 Inspector 設定的穩定 ID 儲存。</summary>
public class GameSaveSystem : MonoBehaviour
{
    public const int SlotCount = 3;
    public static GameSaveSystem Instance { get; private set; }
    public int ActiveSlot { get; private set; } = -1;
    public float autosaveInterval = 180f;
    private float autosaveTimer;
    private bool restorePending;
    private static string FilePath => Path.Combine(Application.persistentDataPath, "saves.json");

    [Serializable] private class SaveFile { public List<GameSaveData> slots = new List<GameSaveData>(); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GameSaveSystem");
        DontDestroyOnLoad(go);
        go.AddComponent<GameSaveSystem>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() { if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Update()
    {
        if (ActiveSlot < 0 || !IsGameplayScene(SceneManager.GetActiveScene().name)) return;
        autosaveTimer += Time.unscaledDeltaTime;
        if (autosaveTimer >= autosaveInterval) { autosaveTimer = 0f; SaveActive(); }
    }

    public static bool IsGameplayScene(string scene) => scene != "Game menu" && scene != "MainMenu";

    public List<GameSaveData> GetSlots()
    {
        SaveFile file = ReadFile();
        if (file.slots == null) file.slots = new List<GameSaveData>();
        foreach (var slot in file.slots)
        {
            if (slot == null) continue;
            if (slot.itemIDs == null) slot.itemIDs = new List<string>();
            if (slot.objectStates == null) slot.objectStates = new List<string>();
        }
        while (file.slots.Count < SlotCount) file.slots.Add(null);
        return file.slots;
    }

    public int CreateSlot(string name = null)
    {
        var file = ReadFile();
        if (file.slots == null) file.slots = new List<GameSaveData>();
        while (file.slots.Count < SlotCount) file.slots.Add(null);
        for (int i = 0; i < SlotCount; i++)
        {
            if (file.slots[i] != null) continue;
            file.slots[i] = new GameSaveData { slotName = string.IsNullOrWhiteSpace(name) ? $"存檔 {i + 1}" : name.Trim(), savedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") };
            WriteFile(file);
            ActiveSlot = i;
            return i;
        }
        return -1;
    }

    public void StartNewGameInSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return;
        var slots = GetSlots();
        string name = slots[slot] == null ? $"存檔 {slot + 1}" : slots[slot].slotName;
        slots[slot] = new GameSaveData { slotName = name, savedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") };
        var file = ReadFile();
        if (file.slots == null) file.slots = new List<GameSaveData>();
        while (file.slots.Count < SlotCount) file.slots.Add(null);
        file.slots[slot] = slots[slot];
        WriteFile(file);
        ActiveSlot = slot;
        autosaveTimer = 0f;
    }

    public void LoadSlot(int slot)
    {
        var slots = GetSlots();
        if (slot < 0 || slot >= SlotCount || slots[slot] == null) return;
        if (string.IsNullOrEmpty(slots[slot].sceneName))
        {
            StartNewGameInSlot(slot);
            SceneManager.LoadScene("room1");
            return;
        }
        ActiveSlot = slot;
        restorePending = true;
        autosaveTimer = 0f;
        Time.timeScale = 1f;
        SceneManager.LoadScene(slots[slot].sceneName);
    }

    public void RenameSlot(int slot, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var file = ReadFile();
        if (file.slots == null) file.slots = new List<GameSaveData>();
        if (slot < 0 || slot >= file.slots.Count || file.slots[slot] == null) return;
        file.slots[slot].slotName = name.Trim();
        WriteFile(file);
    }

    public void DeleteSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return;
        var file = ReadFile();
        if (file.slots == null || slot >= file.slots.Count || file.slots[slot] == null) return;
        file.slots[slot] = null;
        WriteFile(file);
        if (ActiveSlot == slot) ActiveSlot = -1;
    }

    public void SaveActive()
    {
        if (ActiveSlot < 0 || !IsGameplayScene(SceneManager.GetActiveScene().name)) return;
        var file = ReadFile();
        if (file.slots == null) file.slots = new List<GameSaveData>();
        while (file.slots.Count < SlotCount) file.slots.Add(null);
        var data = file.slots[ActiveSlot] ?? new GameSaveData { slotName = $"存檔 {ActiveSlot + 1}" };
        data.sceneName = SceneManager.GetActiveScene().name;
        data.savedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) { data.playerX = player.transform.position.x; data.playerY = player.transform.position.y; }
        if (data.itemIDs == null) data.itemIDs = new List<string>();
        if (data.objectStates == null) data.objectStates = new List<string>();
        data.itemIDs.Clear();
        if (InventoryManager.Instance != null)
            foreach (Item item in InventoryManager.Instance.items) if (item != null) data.itemIDs.Add(item.itemID);
        data.objectStates.Clear();
        foreach (var state in FindObjectsOfType<SaveableObject>(true)) state.WriteTo(data);
        file.slots[ActiveSlot] = data;
        WriteFile(file);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!restorePending) return;
        restorePending = false;
        StartCoroutine(RestoreAfterSceneStart());
    }

    private System.Collections.IEnumerator RestoreAfterSceneStart()
    {
        yield return null;
        var slots = GetSlots();
        if (ActiveSlot < 0 || slots[ActiveSlot] == null) yield break;
        var data = slots[ActiveSlot];
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = new Vector3(data.playerX, data.playerY, player.transform.position.z);
        if (InventoryManager.Instance != null) InventoryManager.Instance.RestoreItems(data.itemIDs);
        foreach (var state in FindObjectsOfType<SaveableObject>(true)) state.ReadFrom(data);
    }

    private void OnApplicationQuit() => SaveActive();

    private SaveFile ReadFile()
    {
        try { return File.Exists(FilePath) ? JsonUtility.FromJson<SaveFile>(File.ReadAllText(FilePath)) ?? new SaveFile() : new SaveFile(); }
        catch (Exception e) { Debug.LogWarning($"讀取存檔失敗：{e.Message}"); return new SaveFile(); }
    }

    private void WriteFile(SaveFile file)
    {
        try { File.WriteAllText(FilePath, JsonUtility.ToJson(file, true)); }
        catch (Exception e) { Debug.LogError($"寫入存檔失敗：{e.Message}"); }
    }
}

