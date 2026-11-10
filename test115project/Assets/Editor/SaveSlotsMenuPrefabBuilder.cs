using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SaveSlotsMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/Resources/SaveSlotsMenu.prefab";

    static SaveSlotsMenuPrefabBuilder()
    {
        EditorApplication.delayCall += EnsurePrefabExists;
    }

    private static void EnsurePrefabExists()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) CreatePrefab();
        EnsureCreateGameUI();
    }

    private static void EnsureCreateGameUI()
    {
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabAsset == null) return;
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        SaveSlotsMenu controller = root.GetComponent<SaveSlotsMenu>();
        if (controller == null) { PrefabUtility.UnloadPrefabContents(root); return; }

        var serialized = new SerializedObject(controller);
        SerializedProperty versionProperty = serialized.FindProperty("prefabLayoutVersion");
        SerializedProperty dialogProperty = serialized.FindProperty("createGamePanel");
        bool needsDialog = dialogProperty.objectReferenceValue == null;
        if (!needsDialog && versionProperty.intValue >= 3)
        {
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        Transform overlay = root.transform.Find("Dim Background");
        Transform slotsPanel = overlay != null ? overlay.Find("Save Slots Panel") : null;
        if (overlay == null || slotsPanel == null)
        {
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        TMP_FontAsset font = FindMenuFont();
        Button backButton = serialized.FindProperty("backButton").objectReferenceValue as Button;
        Button createButton = serialized.FindProperty("createGameButton").objectReferenceValue as Button;
        GameObject dialog = dialogProperty.objectReferenceValue as GameObject;
        TMP_InputField nameInput = serialized.FindProperty("createNameInput").objectReferenceValue as TMP_InputField;
        TMP_Text feedback = serialized.FindProperty("createGameFeedback").objectReferenceValue as TMP_Text;
        Button confirm = serialized.FindProperty("confirmCreateButton").objectReferenceValue as Button;
        Button cancel = serialized.FindProperty("cancelCreateButton").objectReferenceValue as Button;
        GameObject deleteDialog = serialized.FindProperty("deleteConfirmPanel").objectReferenceValue as GameObject;
        TMP_Text deleteText = serialized.FindProperty("deleteConfirmText").objectReferenceValue as TMP_Text;
        Button confirmDelete = serialized.FindProperty("confirmDeleteButton").objectReferenceValue as Button;
        Button cancelDelete = serialized.FindProperty("cancelDeleteButton").objectReferenceValue as Button;
        var deleteButtons = new Button[GameSaveSystem.SlotCount];

        for (int i = 0; i < GameSaveSystem.SlotCount; i++)
        {
            Transform row = slotsPanel.Find($"Save Slot {i + 1}");
            Transform oldNewGameButton = row != null ? row.Find("New Game Button") : null;
            if (oldNewGameButton != null) Object.DestroyImmediate(oldNewGameButton.gameObject);

            if (row == null) continue;
            Place(row.Find("Save Name Input") as RectTransform, new Vector2(-60, 0), new Vector2(300, 64));
            Place(row.Find("Rename Button") as RectTransform, new Vector2(190, 0), new Vector2(145, 70));
            Place(row.Find("Load Button") as RectTransform, new Vector2(365, 0), new Vector2(145, 70));
            deleteButtons[i] = row.Find("Delete Button") != null ? row.Find("Delete Button").GetComponent<Button>() :
                CreateButton("Delete", "刪除", row, new Vector2(540, 0), new Vector2(145, 70), font);
            Place(deleteButtons[i].transform as RectTransform, new Vector2(540, 0), new Vector2(145, 70));
        }

        if (needsDialog)
        {
            createButton = CreateButton("Create New Game", "建立新遊戲", slotsPanel, new Vector2(-285, -365), new Vector2(520, 130), font);
            dialog = CreateObject("Create New Game Panel", overlay);
            Center(dialog.GetComponent<RectTransform>(), new Vector2(1100, 650));
            dialog.AddComponent<Image>().color = new Color(0.055f, 0.065f, 0.075f, 0.99f);
            dialog.SetActive(false);
            CreateText("Create Title", "建立新遊戲", dialog.transform, new Vector2(0, 220), new Vector2(800, 90), 54, TextAlignmentOptions.Center, font);
            CreateText("Name Prompt", "存檔名稱", dialog.transform, new Vector2(0, 105), new Vector2(500, 60), 34, TextAlignmentOptions.Center, font);
            nameInput = CreateInputField(dialog.transform, Vector2.zero, new Vector2(650, 84), font);
            feedback = CreateText("Create Feedback", "", dialog.transform, new Vector2(0, -95), new Vector2(900, 60), 26, TextAlignmentOptions.Center, font);
            confirm = CreateButton("Confirm Create", "建立", dialog.transform, new Vector2(-190, -220), new Vector2(300, 90), font);
            cancel = CreateButton("Cancel Create", "返回", dialog.transform, new Vector2(190, -220), new Vector2(300, 90), font);
        }

        if (deleteDialog == null)
        {
            deleteDialog = CreateObject("Delete Confirmation Panel", overlay);
            Center(deleteDialog.GetComponent<RectTransform>(), new Vector2(900, 430));
            deleteDialog.AddComponent<Image>().color = new Color(0.055f, 0.065f, 0.075f, 0.99f);
            deleteText = CreateText("Delete Confirmation Text", "確定要刪除這個存檔嗎？\n此操作無法復原。", deleteDialog.transform, new Vector2(0, 75), new Vector2(780, 150), 34, TextAlignmentOptions.Center, font);
            confirmDelete = CreateButton("Confirm Delete", "刪除", deleteDialog.transform, new Vector2(-175, -125), new Vector2(280, 82), font);
            cancelDelete = CreateButton("Cancel Delete", "取消", deleteDialog.transform, new Vector2(175, -125), new Vector2(280, 82), font);
            deleteDialog.SetActive(false);
        }

        Place(backButton != null ? backButton.transform as RectTransform : null, new Vector2(285, -365), new Vector2(320, 130));
        Place(createButton != null ? createButton.transform as RectTransform : null, new Vector2(-285, -365), new Vector2(520, 130));
        ApplyHomeTextButton(backButton, 72);
        ApplyHomeTextButton(createButton, 48);

        serialized.Update();
        serialized.FindProperty("createGameButton").objectReferenceValue = createButton;
        serialized.FindProperty("createGamePanel").objectReferenceValue = dialog;
        serialized.FindProperty("createNameInput").objectReferenceValue = nameInput;
        serialized.FindProperty("createGameFeedback").objectReferenceValue = feedback;
        serialized.FindProperty("confirmCreateButton").objectReferenceValue = confirm;
        serialized.FindProperty("cancelCreateButton").objectReferenceValue = cancel;
        serialized.FindProperty("slotsPanel").objectReferenceValue = slotsPanel.gameObject;
        versionProperty = serialized.FindProperty("prefabLayoutVersion");
        versionProperty.intValue = 3;
        var deleteArray = serialized.FindProperty("deleteButtons");
        deleteArray.arraySize = deleteButtons.Length;
        for (int i = 0; i < deleteButtons.Length; i++) deleteArray.GetArrayElementAtIndex(i).objectReferenceValue = deleteButtons[i];
        serialized.FindProperty("deleteConfirmPanel").objectReferenceValue = deleteDialog;
        serialized.FindProperty("deleteConfirmText").objectReferenceValue = deleteText;
        serialized.FindProperty("confirmDeleteButton").objectReferenceValue = confirmDelete;
        serialized.FindProperty("cancelDeleteButton").objectReferenceValue = cancelDelete;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        Debug.Log("Updated SaveSlotsMenu.prefab button styling and slot action positions.");
    }

    private static void ApplyHomeTextButton(Button button, float fontSize)
    {
        if (button == null) return;
        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        Image background = button.GetComponent<Image>();
        if (text == null) return;

        if (background != null)
        {
            background.color = new Color(1, 1, 1, 0);
            background.raycastTarget = true;
        }
        text.color = Color.white;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        // The text itself is the button's target graphic, so it must receive raycasts.
        text.raycastTarget = true;
        button.targetGraphic = text;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.red;
        colors.pressedColor = Color.red;
        colors.selectedColor = Color.red;
        colors.disabledColor = new Color(1, 1, 1, 0.5f);
        colors.colorMultiplier = 1;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void CreatePrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;
        string directory = Path.GetDirectoryName(PrefabPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        TMP_FontAsset font = FindMenuFont();
        var root = new GameObject("SaveSlotsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject overlay = CreateObject("Dim Background", root.transform);
        Stretch(overlay.GetComponent<RectTransform>());
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0.015f, 0.02f, 0.025f, 0.88f);

        GameObject panel = CreateObject("Save Slots Panel", overlay.transform);
        Center(panel.GetComponent<RectTransform>(), new Vector2(1500, 850));
        panel.AddComponent<Image>().color = new Color(0.055f, 0.065f, 0.075f, 0.98f);
        CreateText("Title", "存檔", panel.transform, new Vector2(0, 350), new Vector2(700, 90), 58, TextAlignmentOptions.Center, font);
        Button back = CreateButton("Back", "返回", panel.transform, new Vector2(0, -365), new Vector2(320, 100), font);

        TMP_Text[] slotLabels = new TMP_Text[GameSaveSystem.SlotCount];
        TMP_Text[] dateLabels = new TMP_Text[GameSaveSystem.SlotCount];
        TMP_InputField[] inputs = new TMP_InputField[GameSaveSystem.SlotCount];
        Button[] renameButtons = new Button[GameSaveSystem.SlotCount];
        Button[] loadButtons = new Button[GameSaveSystem.SlotCount];

        for (int i = 0; i < GameSaveSystem.SlotCount; i++)
        {
            float y = 205 - i * 175;
            GameObject row = CreateObject($"Save Slot {i + 1}", panel.transform);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            Center(rowRect, new Vector2(1370, 150));
            rowRect.anchoredPosition = new Vector2(0, y);
            row.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.16f, 1f);

            slotLabels[i] = CreateText("Slot Label", $"存檔槽 {i + 1}", row.transform, new Vector2(-520, 32), new Vector2(300, 48), 34, TextAlignmentOptions.Left, font);
            dateLabels[i] = CreateText("Save Date", "空存檔槽", row.transform, new Vector2(-520, -32), new Vector2(400, 38), 23, TextAlignmentOptions.Left, font);
            inputs[i] = CreateInputField(row.transform, new Vector2(-150, 0), new Vector2(300, 64), font);
            renameButtons[i] = CreateButton("Rename", "改名", row.transform, new Vector2(95, 0), new Vector2(155, 74), font);
            loadButtons[i] = CreateButton("Load", "載入", row.transform, new Vector2(285, 0), new Vector2(170, 74), font);
        }

        SaveSlotsMenu controller = root.AddComponent<SaveSlotsMenu>();
        var serialized = new SerializedObject(controller);
        SetArray(serialized, "slotLabels", slotLabels);
        SetArray(serialized, "dateLabels", dateLabels);
        SetArray(serialized, "nameInputs", inputs);
        SetArray(serialized, "renameButtons", renameButtons);
        SetArray(serialized, "loadButtons", loadButtons);
        serialized.FindProperty("backButton").objectReferenceValue = back;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Save slots prefab created at {PrefabPath}");
    }

    private static TMP_FontAsset FindMenuFont()
    {
        string[] matches = AssetDatabase.FindAssets("Cubic_11 SDF t:TMP_FontAsset");
        if (matches.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(matches[0]));
    }

    private static GameObject CreateObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text CreateText(string name, string value, Transform parent, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        GameObject go = CreateObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        Center(rect, size);
        rect.anchoredPosition = position;
        TMP_Text text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static TMP_InputField CreateInputField(Transform parent, Vector2 position, Vector2 size, TMP_FontAsset font)
    {
        GameObject go = CreateObject("Save Name Input", parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        Center(rect, size);
        rect.anchoredPosition = position;
        Image background = go.AddComponent<Image>();
        background.color = new Color(0.2f, 0.22f, 0.24f, 1f);
        TMP_InputField field = go.AddComponent<TMP_InputField>();

        GameObject textObject = CreateObject("Text", go.transform);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        Stretch(textRect);
        textRect.offsetMin = new Vector2(12, 5);
        textRect.offsetMax = new Vector2(-12, -5);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;

        GameObject placeholderObject = CreateObject("Placeholder", go.transform);
        RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
        Stretch(placeholderRect);
        placeholderRect.offsetMin = new Vector2(12, 5);
        placeholderRect.offsetMax = new Vector2(-12, -5);
        TMP_Text placeholder = placeholderObject.AddComponent<TextMeshProUGUI>();
        placeholder.font = font;
        placeholder.fontSize = 22;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = new Color(1, 1, 1, 0.45f);
        placeholder.text = "輸入存檔名稱";

        field.textComponent = text;
        field.textViewport = textRect;
        field.placeholder = placeholder;
        field.targetGraphic = background;
        field.text = "";
        return field;
    }

    private static Button CreateButton(string name, string label, Transform parent, Vector2 position, Vector2 size, TMP_FontAsset font)
    {
        GameObject go = CreateObject(name + " Button", parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        Center(rect, size);
        rect.anchoredPosition = position;
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.22f, 0.24f, 0.26f, 1f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.22f, 0.24f, 0.26f, 1f);
        colors.highlightedColor = new Color(0.65f, 0.1f, 0.1f, 1f);
        colors.pressedColor = new Color(0.82f, 0.12f, 0.12f, 1f);
        button.colors = colors;
        CreateText("Label", label, go.transform, Vector2.zero, size, 26, TextAlignmentOptions.Center, font);
        return button;
    }

    private static void SetArray<T>(SerializedObject serialized, string propertyName, T[] values) where T : Object
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void Center(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
