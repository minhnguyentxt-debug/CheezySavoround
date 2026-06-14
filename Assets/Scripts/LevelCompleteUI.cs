using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn script này TRỰC TIẾP lên GameObject LevelCompletePanel.
/// Button "Tiếp theo" → wired vào OnClickNext() của script này (cùng object).
/// Dùng PlayerPrefs để lưu level — đáng tin cậy 100% giữa các scene reload.
/// </summary>
public class LevelCompleteUI : MonoBehaviour
{
    private const string LEVEL_KEY = "Game_CurrentLevel";

    private void Start()
    {
        // Tự ẩn khi scene bắt đầu
        gameObject.SetActive(false);

        // Subscribe event (phải làm ở Start vì object inactive → OnEnable không chạy)
        GameEventSystem.OnLevelComplete += Show;
    }

    private void OnDestroy()
    {
        GameEventSystem.OnLevelComplete -= Show;
    }

    private void Show()
    {
        // Không hiện khi đã game over
        GridManager gm = FindAnyObjectByType<GridManager>();
        if (gm != null && gm.gameOverPanel != null && gm.gameOverPanel.activeSelf)
            return;

        gameObject.SetActive(true);
        Debug.Log($"<color=lime>[LevelCompleteUI] Hoàn thành màn {GetCurrentLevel()}!</color>");
    }

    /// <summary>
    /// Gắn vào Button "Tiếp theo" trong Inspector.
    /// </summary>
    public void OnClickNext()
    {
        int current = GetCurrentLevel();
        int next = (current >= LevelManager.TotalLevels) ? 1 : current + 1;

        Debug.Log($"<color=cyan>[LevelCompleteUI] {current} → {next}</color>");

        // ① Lưu level mới vào PlayerPrefs (đáng tin cậy nhất)
        PlayerPrefs.SetInt(LEVEL_KEY, next);
        PlayerPrefs.Save();

        // ② Đồng bộ vào SaveManager để các hệ thống khác cũng biết
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.PlayerData.currentLevel = next;
            SaveManager.Instance.PlayerData.Plates.Clear();
            SaveManager.Instance.SaveGame();
        }

        // ③ Reset điểm cho màn mới
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScoreForNewGame();

        if (LevelManager.Instance != null)
            LevelManager.Instance.SetLevel(next);

        GameEventSystem.OnLevelChanged?.Invoke(next);

        // ④ Reload scene — GridManager sẽ đọc LEVEL_KEY từ PlayerPrefs
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Đọc level hiện tại — PlayerPrefs là nguồn chính.
    /// </summary>
    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(LEVEL_KEY, 1);
    }

    /// <summary>
    /// Gọi khi "Chơi Mới" để reset về màn 1.
    /// </summary>
    public static void ResetLevel()
    {
        PlayerPrefs.SetInt(LEVEL_KEY, 1);
        PlayerPrefs.Save();
    }
}
