using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>存檔槽 UI 控制器。版面與美術由 Resources/SaveSlotsMenu Prefab 提供。</summary>
public class SaveSlotsMenu : MonoBehaviour
{
    [Header("Prefab UI references")]
    [SerializeField] private TMP_Text[] slotLabels = new TMP_Text[GameSaveSystem.SlotCount];
    [SerializeField] private TMP_Text[] dateLabels = new TMP_Text[GameSaveSystem.SlotCount];
    [SerializeField] private TMP_InputField[] nameInputs = new TMP_InputField[GameSaveSystem.SlotCount];
    [SerializeField] private Button[] renameButtons = new Button[GameSaveSystem.SlotCount];
    [SerializeField] private Button[] loadButtons = new Button[GameSaveSystem.SlotCount];
    [SerializeField] private Button[] deleteButtons = new Button[GameSaveSystem.SlotCount];
    [SerializeField] private Button backButton;
    [Header("Create new game UI")]
    [SerializeField] private Button createGameButton;
    [SerializeField] private GameObject createGamePanel;
    [SerializeField] private TMP_InputField createNameInput;
    [SerializeField] private TMP_Text createGameFeedback;
    [SerializeField] private Button confirmCreateButton;
    [SerializeField] private Button cancelCreateButton;
    [Header("Delete confirmation UI")]
    [SerializeField] private GameObject deleteConfirmPanel;
    [SerializeField] private TMP_Text deleteConfirmText;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;
    [SerializeField] private GameObject slotsPanel;
    [SerializeField, HideInInspector] private int prefabLayoutVersion;

    private SceneLoader sceneLoader;
    private Canvas rootCanvas;
    private int pendingDeleteSlot = -1;
    public bool IsOpen => rootCanvas != null && rootCanvas.gameObject.activeSelf;

    public void Initialize(GameObject unusedOptionsMenuUI, SceneLoader owner)
    {
        sceneLoader = owner;
        rootCanvas = GetComponent<Canvas>();
        WireButtons();
        if (createGamePanel != null) createGamePanel.SetActive(false);
        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(false);
        if (slotsPanel != null) slotsPanel.SetActive(true);
        rootCanvas.gameObject.SetActive(false);
    }

    public void Show()
    {
        Refresh();
        if (slotsPanel != null) slotsPanel.SetActive(true);
        if (createGamePanel != null) createGamePanel.SetActive(false);
        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(false);
        rootCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (rootCanvas != null) rootCanvas.gameObject.SetActive(false);
    }

    private void WireButtons()
    {
        for (int i = 0; i < GameSaveSystem.SlotCount; i++)
        {
            int slot = i;
            renameButtons[slot].onClick.RemoveAllListeners();
            loadButtons[slot].onClick.RemoveAllListeners();
            deleteButtons[slot].onClick.RemoveAllListeners();
            renameButtons[slot].onClick.AddListener(() => Rename(slot));
            loadButtons[slot].onClick.AddListener(() => Load(slot));
            deleteButtons[slot].onClick.AddListener(() => AskDelete(slot));
        }
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(Hide);
        createGameButton.onClick.RemoveAllListeners();
        createGameButton.onClick.AddListener(OpenCreateGame);
        confirmCreateButton.onClick.RemoveAllListeners();
        confirmCreateButton.onClick.AddListener(ConfirmCreateGame);
        cancelCreateButton.onClick.RemoveAllListeners();
        cancelCreateButton.onClick.AddListener(ReturnToSlots);
        confirmDeleteButton.onClick.RemoveAllListeners();
        confirmDeleteButton.onClick.AddListener(DeletePendingSlot);
        cancelDeleteButton.onClick.RemoveAllListeners();
        cancelDeleteButton.onClick.AddListener(CancelDelete);
    }

    private void Refresh()
    {
        if (GameSaveSystem.Instance == null) return;
        var slots = GameSaveSystem.Instance.GetSlots();
        for (int i = 0; i < GameSaveSystem.SlotCount; i++)
        {
            GameSaveData slot = slots[i];
            slotLabels[i].text = $"存檔槽 {i + 1}";
            nameInputs[i].SetTextWithoutNotify(slot == null ? $"存檔 {i + 1}" : slot.slotName);
            dateLabels[i].text = slot == null || string.IsNullOrEmpty(slot.savedAt) ? "空存檔槽" : slot.savedAt;
            loadButtons[i].interactable = slot != null;
            deleteButtons[i].interactable = slot != null;
        }
    }

    private void AskDelete(int slot)
    {
        if (GameSaveSystem.Instance == null || GameSaveSystem.Instance.GetSlots()[slot] == null) return;
        pendingDeleteSlot = slot;
        deleteConfirmText.text = $"確定要刪除「{GameSaveSystem.Instance.GetSlots()[slot].slotName}」嗎？\n此操作無法復原。";
        deleteConfirmPanel.SetActive(true);
    }

    private void DeletePendingSlot()
    {
        if (pendingDeleteSlot >= 0) GameSaveSystem.Instance?.DeleteSlot(pendingDeleteSlot);
        pendingDeleteSlot = -1;
        deleteConfirmPanel.SetActive(false);
        Refresh();
    }

    private void CancelDelete()
    {
        pendingDeleteSlot = -1;
        deleteConfirmPanel.SetActive(false);
    }

    private void Rename(int slot)
    {
        GameSaveSystem.Instance?.RenameSlot(slot, nameInputs[slot].text);
        Refresh();
    }

    private void Load(int slot)
    {
        var saves = GameSaveSystem.Instance;
        if (saves == null || saves.GetSlots()[slot] == null) return;
        GameSaveData data = saves.GetSlots()[slot];
        if (string.IsNullOrEmpty(data.sceneName))
        {
            Hide();
            sceneLoader.BeginNewGameInSlot(slot);
            return;
        }
        Hide();
        saves.LoadSlot(slot);
    }

    private void OpenCreateGame()
    {
        if (slotsPanel != null) slotsPanel.SetActive(false);
        createGamePanel.SetActive(true);
        createNameInput.SetTextWithoutNotify("");
        createGameFeedback.text = "";
        createNameInput.Select();
        createNameInput.ActivateInputField();
    }

    private void ConfirmCreateGame()
    {
        if (GameSaveSystem.Instance == null || sceneLoader == null) return;
        string saveName = createNameInput.text.Trim();
        if (string.IsNullOrEmpty(saveName))
        {
            createGameFeedback.text = "請輸入存檔名稱";
            return;
        }

        int slot = GameSaveSystem.Instance.CreateSlot(saveName);
        if (slot < 0)
        {
            createGameFeedback.text = "存檔槽已滿，請先清出一個槽位";
            return;
        }
        Hide();
        sceneLoader.BeginNewGameInSlot(slot);
    }

    private void ReturnToSlots()
    {
        if (createGamePanel != null) createGamePanel.SetActive(false);
        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(false);
        if (slotsPanel != null) slotsPanel.SetActive(true);
    }
}
