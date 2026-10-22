using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    [Header("--- 場景名稱設定 ---")]
    [Tooltip("你的遊戲主關卡場景名稱")]
    public string gameplaySceneName = "room1";
    [Tooltip("你的主選單場景名稱")]
    public string mainMenuSceneName = "MenuScene";

    /// <summary>
    /// 再玩一次 (重新載入遊戲關卡)
    /// </summary>
    public void RestartGame()
    {
        // 恢復遊戲時間流逝（因為勝利時被設為 0）
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// 返回主選單
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 恢復遊戲時間流逝
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}