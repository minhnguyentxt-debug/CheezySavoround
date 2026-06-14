using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [Header("Cấu hình phần thưởng hàng ngày (7 ngày)")]
    [SerializeField] private int[] dailyRewards = new int[] { 50, 100, 150, 200, 300, 400, 500 };
    
    [Header("Phần thưởng tuần (Ngày thứ 7)")]
    [SerializeField] private int weeklyBonus = 200;

    [Header("Bật/Tắt hệ thống thưởng ngày")]
    [SerializeField] private bool isEnabled = true;

    [Header("Developer Mode (CHỈ hoạt động trong Unity Editor)")]
    [SerializeField] private bool resetRewardOnStart = false;
    [SerializeField] private bool showDevLogs = true;

    public bool IsEnabled => isEnabled;
    public int CurrentStreak => SaveManager.Instance?.PlayerData?.currentStreak ?? 0;
    public int[] DailyRewards => dailyRewards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Developer Mode: Tự động reset thưởng khi chạy game (CHỈ trong Unity Editor)
#if UNITY_EDITOR
        if (resetRewardOnStart)
        {
            ResetRewardForTesting();
        }
#endif

        CheckDailyReward();
    }

    /// <summary>
    /// Kiểm tra xem người chơi có thể nhận thưởng hôm nay không
    /// </summary>
    public bool CanClaimToday()
    {
        if (!isEnabled || SaveManager.Instance == null) return false;

        GameData data = SaveManager.Instance.PlayerData;
        DateTime now = DateTime.UtcNow;

        // Nếu chưa từng nhận -> có thể nhận
        if (string.IsNullOrEmpty(data.lastClaimedTime))
        {
            return true;
        }

        try
        {
            DateTime lastClaimed = DateTime.Parse(data.lastClaimedTime);
            TimeSpan difference = now.Date - lastClaimed.Date;
            return difference.TotalDays >= 1;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyReward] Lỗi parse thời gian: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lấy số coin sẽ nhận được dựa trên streak hiện tại
    /// </summary>
    public int GetCurrentReward()
    {
        if (SaveManager.Instance == null) return 0;

        int streak = SaveManager.Instance.PlayerData.currentStreak;
        int baseReward = dailyRewards[Mathf.Clamp(streak, 0, dailyRewards.Length - 1)];

        // Nếu là ngày thứ 7 (tuần hoàn), thêm bonus tuần
        if (streak == 6)
        {
            return baseReward + weeklyBonus;
        }

        return baseReward;
    }

    /// <summary>
    /// Kiểm tra và cập nhật streak khi mở game
    /// </summary>
    public void CheckDailyReward()
    {
        if (!isEnabled || SaveManager.Instance == null) return;

        GameData data = SaveManager.Instance.PlayerData;
        DateTime now = DateTime.UtcNow;

        if (string.IsNullOrEmpty(data.lastClaimedTime))
        {
            Debug.Log("[DailyReward] Lần đầu tiên! Có thể nhận thưởng ngày 1.");
            GameEventSystem.OnDailyRewardAvailable?.Invoke(true);
            return;
        }

        try
        {
            DateTime lastClaimed = DateTime.Parse(data.lastClaimedTime);
            TimeSpan difference = now.Date - lastClaimed.Date;

            if (difference.TotalDays >= 1)
            {
                if (difference.TotalDays == 1)
                {
                    // Đăng nhập liên tiếp -> Tăng streak
                    data.currentStreak = (data.currentStreak + 1) % 7;
                }
                else
                {
                    // Bỏ lỡ ngày -> Reset streak về 0
                    data.currentStreak = 0;
                    Debug.Log("[DailyReward] Đã bỏ lỡ ngày, streak reset về ngày 1.");
                }

                SaveManager.Instance.SaveGame();
                GameEventSystem.OnDailyRewardAvailable?.Invoke(true);
                Debug.Log($"[DailyReward] Có thể nhận quà! Streak hiện tại: Ngày {data.currentStreak + 1}/7");
            }
            else
            {
                GameEventSystem.OnDailyRewardAvailable?.Invoke(false);
                Debug.Log("[DailyReward] Hôm nay đã nhận quà rồi!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DailyReward] Lỗi kiểm tra thời gian: {e.Message}");
        }
    }

    /// <summary>
    /// Nhận thưởng ngày (cộng coin vào ScoreManager)
    /// </summary>
    public void ClaimReward()
    {
        if (!CanClaimToday())
        {
            Debug.LogWarning("[DailyReward] Không thể nhận thưởng lúc này!");
            return;
        }

        GameData data = SaveManager.Instance.PlayerData;
        int rewardAmount = GetCurrentReward();

        // Cộng coin thông qua ScoreManager (lưu vào PlayerPrefs)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCoins(rewardAmount);
            
            // QUAN TRỌNG: Đồng bộ coins từ ScoreManager sang SaveManager
            // Vì ScoreManager dùng PlayerPrefs, SaveManager dùng file mã hóa
            data.coins = ScoreManager.Instance.Coins;
        }

        // Cập nhật thời gian nhận quà
        data.lastClaimedTime = DateTime.UtcNow.ToString();
        
        // Lưu vào file mã hóa (bao gồm cả coins đã đồng bộ)
        SaveManager.Instance.SaveGame();

        // Kích hoạt event
        GameEventSystem.OnDailyRewardClaimed?.Invoke(rewardAmount);
        GameEventSystem.OnDailyRewardAvailable?.Invoke(false);

        Debug.Log($"[DailyReward] ✓ Đã nhận {rewardAmount} Coins! Tổng: {data.coins} coins. Streak: {data.currentStreak + 1}/7");
    }

    /// <summary>
    /// Bật/Tắt hệ thống thưởng ngày
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        Debug.Log($"[DailyReward] Hệ thống thưởng ngày: {(enabled ? "BẬT" : "TẮT")}");
    }

#if UNITY_EDITOR
    /// <summary>
    /// [DEV MODE] Reset thưởng để test - CHỈ hoạt động trong Unity Editor
    /// </summary>
    private void ResetRewardForTesting()
    {
        if (SaveManager.Instance == null) return;

        GameData data = SaveManager.Instance.PlayerData;
        
        // Reset về 2 ngày trước để có thể nhận thưởng ngay
        data.lastClaimedTime = DateTime.UtcNow.AddDays(-2).ToString();
        data.currentStreak = 0;
        
        SaveManager.Instance.SaveGame();

        if (showDevLogs)
        {
            Debug.Log("<color=yellow>[DailyReward - DEV MODE] ✓ Đã tự động reset thưởng ngày! Có thể claim ngay.</color>");
        }
    }
#endif
}