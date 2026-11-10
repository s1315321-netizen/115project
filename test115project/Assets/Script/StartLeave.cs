using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [Header("--- UI Panels ---")]
    public GameObject mainMenuUI;     // 主選單面板 (Start / Options / Leave)
    public GameObject optionsMenuUI;  // OptionsMenu 面板

    [Header("--- Scene Transition ---")]
    public string firstSceneName = "room1"; // 預設要載入的第一關名稱
    public CanvasGroup fadeCanvasGroup;     // 拖入黑幕上的 CanvasGroup
    public float fadeDuration = 1.0f;       // 淡出/淡入過渡時間 (秒)
    public bool fadeInOnStart = true;       // 剛進入此場景時是否從全黑淡入顯示畫面

    private bool isTransitioning = false;   // 防止重複觸發切換
    private SaveSlotsMenu saveSlotsMenu;

    void Start()
    {
        // 進入主選單時，重置遊戲時間與音效
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 初始化面板狀態
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        // Only the configured menu SceneLoader owns the save menu. The scene has
        // legacy SceneLoader components on menu buttons with no menu references.
        if (mainMenuUI != null && FindObjectOfType<SaveSlotsMenu>(true) == null)
        {
            GameObject saveSlotsPrefab = Resources.Load<GameObject>("SaveSlotsMenu");
            if (saveSlotsPrefab != null)
            {
                GameObject saveSlotsObject = Instantiate(saveSlotsPrefab);
                saveSlotsMenu = saveSlotsObject.GetComponent<SaveSlotsMenu>();
                if (saveSlotsMenu != null) saveSlotsMenu.Initialize(optionsMenuUI, this);
                else Debug.LogError("SaveSlotsMenu.prefab 缺少 SaveSlotsMenu 控制器元件。");
            }
            else Debug.LogError("找不到 Assets/Resources/SaveSlotsMenu.prefab，請確認 Prefab 已建立。");
        }
        else if (mainMenuUI != null)
        {
            saveSlotsMenu = FindObjectOfType<SaveSlotsMenu>(true);
            if (saveSlotsMenu != null) saveSlotsMenu.Initialize(optionsMenuUI, this);
        }
        if (mainMenuUI != null) CreateCanvasSaveButton();

        // 如果勾選進入場景淡入：從全黑 (1) 平滑變透明 (0)
        if (fadeInOnStart && fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeRoutine(0f));
        }
        else if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        // 轉場期間不允許按鍵操作
        if (isTransitioning) return;

        // 當設定面板開啟時，按 ESC 鍵可直接返回主選單
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (saveSlotsMenu != null && saveSlotsMenu.IsOpen)
            {
                saveSlotsMenu.Hide();
                return;
            }
            if (optionsMenuUI != null && optionsMenuUI.activeSelf)
            {
                CloseOptions();
            }
        }
    }

    /// <summary>
    /// 點擊【Start Game】進入遊戲（平滑淡出至全黑後載入場景）
    /// </summary>
    public void GoToRoom1()
    {
        if (isTransitioning) return;
        var saves = GameSaveSystem.Instance;
        if (saves != null && saves.CreateSlot() < 0)
        {
            ShowSaveSlots();
            return;
        }
        StartCoroutine(FadeAndLoadRoutine(firstSceneName));
    }

    private void CreateCanvasSaveButton()
    {
        if (mainMenuUI == null) return;
        Transform optionsTransform = mainMenuUI.transform.Find("Options");
        if (optionsTransform == null) return;

        Transform existing = mainMenuUI.transform.Find("SaveLoad");
        if (existing != null) return;

        GameObject saveButtonObject = Instantiate(optionsTransform.gameObject, optionsTransform.parent);
        saveButtonObject.name = "SaveLoad";
        saveButtonObject.transform.SetSiblingIndex(optionsTransform.GetSiblingIndex() + 1);
        RectTransform saveRect = saveButtonObject.GetComponent<RectTransform>();
        saveRect.anchoredPosition = new Vector2(-540f, -200f);
        saveRect.sizeDelta = new Vector2(450f, 125f);

        TMP_Text label = saveButtonObject.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = "存檔";
        Button sourceButton = saveButtonObject.GetComponent<Button>();
        if (sourceButton != null)
        {
            Navigation navigation = sourceButton.navigation;
            Selectable.Transition transition = sourceButton.transition;
            ColorBlock colors = sourceButton.colors;
            SpriteState spriteState = sourceButton.spriteState;
            AnimationTriggers animationTriggers = sourceButton.animationTriggers;
            Graphic targetGraphic = sourceButton.targetGraphic;
            sourceButton.enabled = false;
            Destroy(sourceButton);

            Button button = saveButtonObject.AddComponent<Button>();
            button.navigation = navigation;
            button.transition = transition;
            button.colors = colors;
            button.spriteState = spriteState;
            button.animationTriggers = animationTriggers;
            button.targetGraphic = targetGraphic;
            button.onClick.AddListener(ToggleSavePanel);
        }

        // 留出一個按鈕的位置，讓四個主選單按鈕保持相同間距與尺寸。
        SetMenuButtonLayout("Start", -70f);
        saveRect.anchoredPosition = new Vector2(-540f, -200f);
        SetMenuButtonLayout("Options", -330f);
        SetMenuButtonLayout("Quit", -460f);
    }

    private void SetMenuButtonLayout(string objectName, float y)
    {
        Transform button = mainMenuUI.transform.Find(objectName);
        if (button == null) return;
        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchoredPosition = new Vector2(-540f, y);
        rect.sizeDelta = new Vector2(450f, 125f);
    }

    private void ToggleSavePanel()
    {
        ShowSaveSlots();
    }

    public void OpenSaveSlots()
    {
        ToggleSavePanel();
    }

    public void ShowSaveSlots()
    {
        if (saveSlotsMenu != null) saveSlotsMenu.Show();
    }

    public void BeginNewGameInSlot(int slot)
    {
        if (isTransitioning || GameSaveSystem.Instance == null) return;
        GameSaveSystem.Instance.StartNewGameInSlot(slot);
        StartCoroutine(FadeAndLoadRoutine(firstSceneName));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        isTransitioning = true;

        // 淡出至全黑 (Alpha = 1)
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeRoutine(1f));
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        // 載入新場景
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 平滑控制黑幕透明度協程
    /// </summary>
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.gameObject.SetActive(true);

        // 只要不是完全透明，就阻擋滑鼠射線點擊
        fadeCanvasGroup.blocksRaycasts = (targetAlpha > 0f);

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // 使用 unscaledDeltaTime 確保時間縮放為 0 時轉場依然流暢
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // 完全透明時關閉射線阻擋，允許玩家點擊按鈕
        if (targetAlpha <= 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 點擊【Options】開啟設定
    /// </summary>
    public void OpenOptions()
    {
        if (isTransitioning) return;
        
        if (optionsMenuUI != null) optionsMenuUI.SetActive(true);
    }

    /// <summary>
    /// 點擊【Back】或按 ESC 關閉設定
    /// </summary>
    public void CloseOptions()
    {
        if (isTransitioning) return;
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    /// <summary>
    /// 點擊【Quit】離開遊戲
    /// </summary>
    public void QuitGame()
    {
        if (isTransitioning) return;
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
