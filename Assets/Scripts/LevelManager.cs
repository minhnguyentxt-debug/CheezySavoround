using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý hệ thống màn chơi (30 màn).
/// KHÔNG DontDestroyOnLoad – tạo mới mỗi scene load, đọc level từ SaveManager.
/// Cách này đảm bảo button trong scene luôn trỏ đúng vào instance hợp lệ.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public const int TotalLevels = 30;

    [Header("Win Panel")]
    [Tooltip("Kéo Panel thắng màn vào đây")]
    public GameObject levelCompletePanel;

    private static bool _isSessionInitialized = false;

    public int CurrentLevel
    {
        get => LevelCompleteUI.GetCurrentLevel();
        private set
        {
            int clamped = Mathf.Clamp(value, 1, TotalLevels);
            PlayerPrefs.SetInt("Game_CurrentLevel", clamped);
            PlayerPrefs.Save();
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.PlayerData.currentLevel = clamped;
            }
        }
    }

    public int TargetScore { get; set; }

    public bool IsLastLevel => CurrentLevel >= TotalLevels;

    // ─── Singleton thông thường (KHÔNG DontDestroyOnLoad) ─────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Nếu là lần đầu chạy game trong phiên này, ép về màn 1 và xóa đĩa lưu
        if (!_isSessionInitialized)
        {
            _isSessionInitialized = true;
            LevelCompleteUI.ResetLevel();
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.PlayerData.Plates.Clear();
                SaveManager.Instance.PlayerData.currentLevel = 1;
                SaveManager.Instance.SaveGame();
            }
            Debug.Log("<color=orange>[LevelManager] Khởi động phiên chơi mới: Đã ép về màn 1 và xóa đĩa lưu.</color>");
        }

        // Đồng bộ từ PlayerPrefs (nguồn chính) sang SaveManager
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.PlayerData.currentLevel = CurrentLevel;
        }

        Debug.Log($"<color=cyan>[LevelManager] Màn hiện tại: {CurrentLevel}</color>");
    }

    private void Start()
    {
        // Ẩn panel ngay khi scene bắt đầu
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    private void OnEnable()
    {
        // LevelCompleteUI.cs (đặt trên panel) đã xử lý show/hide panel rồi
        // LevelManager chỉ cần expose SetLevel() và giá trị CurrentLevel
    }

    private void OnDisable() { }

    /// <summary>
    /// Được gọi bởi LevelCompleteUI khi người chơi bấm Next.
    /// Đồng bộ CurrentLevel với giá trị LevelCompleteUI đã tính toán.
    /// </summary>
    public void SetLevel(int level)
    {
        CurrentLevel = level;
    }

    /// <summary>
    /// Reset về màn 1 (dùng khi nhấn "Chơi Mới").
    /// </summary>
    public void ResetToFirstLevel()
    {
        CurrentLevel = 1;
        Debug.Log("[LevelManager] Reset về màn 1.");
    }
}
