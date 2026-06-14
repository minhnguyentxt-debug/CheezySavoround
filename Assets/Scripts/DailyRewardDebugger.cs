using System;
using UnityEngine;

/// <summary>
/// Script Debug để test hệ thống Daily Reward
/// CHỈ SỬ DỤNG CHO TESTING - XÓA TRƯỚC KHI RELEASE!
/// </summary>
public class DailyRewardDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private bool enableDebugMode = true;

    private void Update()
    {
        if (!enableDebugMode) return;

        // Phím tắt để test
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetRewardTime();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SimulateNextDay();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ForceClaimReward();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowCurrentStatus();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetStreak(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetStreak(6);
        }
    }

    /// <summary>
    /// [R] Reset thời gian claim về 2 ngày trước -> Có thể nhận thưởng ngay
    /// </summary>
    [ContextMenu("Reset Reward Time")]
    public void ResetRewardTime()
    {
        if (SaveManager.Instance == null) return;

        GameData data = SaveManager.Instance.PlayerData;
        data.lastClaimedTime = DateTime.UtcNow.AddDays(-2).ToString();
        data.currentStreak = 0;
        SaveManager.Instance.SaveGame();

        Debug.Log("[DEBUG] ✓ Đã reset thời gian claim. Bây giờ có thể nhận thưởng!");
        
        if (DailyRewardManager.Instance != null)
            DailyRewardManager.Instance.CheckDailyReward();
    }

    /// <summary>
    /// [T] Giả lập sang ngày tiếp theo (thêm 1 ngày vào lastClaimedTime)
    /// </summary>
    [ContextMenu("Simulate Next Day")]
    public void SimulateNextDay()
    {
        if (SaveManager.Instance == null) return;

        GameData data = SaveManager.Instance.PlayerData;
        
        if (string.IsNullOrEmpty(data.lastClaimedTime))
        {
            data.lastClaimedTime = DateTime.UtcNow.AddDays(-1).ToString();
        }
        else
        {
            DateTime lastClaimed = DateTime.Parse(data.lastClaimedTime);
            data.lastClaimedTime = lastClaimed.AddDays(-1).ToString();
        }

        SaveManager.Instance.SaveGame();
        Debug.Log("[DEBUG] ⏰ Đã giả lập sang ngày tiếp theo!");

        if (DailyRewardManager.Instance != null)
            DailyRewardManager.Instance.CheckDailyReward();
    }

    /// <summary>
    /// [C] Buộc claim reward (bỏ qua kiểm tra thời gian)
    /// </summary>
    [ContextMenu("Force Claim Reward")]
    public void ForceClaimReward()
    {
        if (SaveManager.Instance == null || DailyRewardManager.Instance == null) return;

        // Reset thời gian để có thể claim
        GameData data = SaveManager.Instance.PlayerData;
        data.lastClaimedTime = DateTime.UtcNow.AddDays(-1).ToString();
        SaveManager.Instance.SaveGame();

        // Claim
        DailyRewardManager.Instance.ClaimReward();
        Debug.Log("[DEBUG] 🎁 Đã force claim thưởng!");
    }

    /// <summary>
    /// [S] Hiển thị trạng thái hiện tại
    /// </summary>
    [ContextMenu("Show Current Status")]
    public void ShowCurrentStatus()
    {
        if (SaveManager.Instance == null) return;

        GameData data = SaveManager.Instance.PlayerData;
        
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("📊 TRẠNG THÁI DAILY REWARD:");
        Debug.Log($"   Streak hiện tại: Ngày {data.currentStreak + 1}/7");
        Debug.Log($"   Lần claim cuối: {(string.IsNullOrEmpty(data.lastClaimedTime) ? "Chưa có" : data.lastClaimedTime)}");
        
        if (DailyRewardManager.Instance != null)
        {
            bool canClaim = DailyRewardManager.Instance.CanClaimToday();
            int reward = DailyRewardManager.Instance.GetCurrentReward();
            Debug.Log($"   Có thể claim: {(canClaim ? "✓ CÓ" : "✗ KHÔNG")}");
            Debug.Log($"   Phần thưởng hiện tại: {reward} coins");
            Debug.Log($"   Hệ thống: {(DailyRewardManager.Instance.IsEnabled ? "BẬT" : "TẮT")}");
        }

        if (ScoreManager.Instance != null)
        {
            Debug.Log($"   Tổng coins: {ScoreManager.Instance.Coins}");
        }

        Debug.Log("═══════════════════════════════════════");
    }

    /// <summary>
    /// Set streak về ngày cụ thể (0-6)
    /// </summary>
    public void SetStreak(int day)
    {
        if (SaveManager.Instance == null) return;
        
        day = Mathf.Clamp(day, 0, 6);
        GameData data = SaveManager.Instance.PlayerData;
        data.currentStreak = day;
        data.lastClaimedTime = DateTime.UtcNow.AddDays(-1).ToString();
        SaveManager.Instance.SaveGame();

        Debug.Log($"[DEBUG] 📅 Đã set streak về ngày {day + 1}/7");
        
        if (DailyRewardManager.Instance != null)
            DailyRewardManager.Instance.CheckDailyReward();
    }

    /// <summary>
    /// Xóa toàn bộ dữ liệu daily reward
    /// </summary>
    [ContextMenu("Clear All Daily Reward Data")]
    public void ClearAllData()
    {
        if (SaveManager.Instance == null) return;

        GameData data = SaveManager.Instance.PlayerData;
        data.lastClaimedTime = "";
        data.currentStreak = 0;
        SaveManager.Instance.SaveGame();

        Debug.Log("[DEBUG] 🗑️ Đã xóa toàn bộ dữ liệu daily reward!");
    }

    private void OnGUI()
    {
        if (!enableDebugMode) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;

        string debugInfo = "=== DAILY REWARD DEBUG ===\n" +
                          "[R] Reset thời gian\n" +
                          "[T] Giả lập ngày tiếp theo\n" +
                          "[C] Force claim reward\n" +
                          "[S] Show trạng thái\n" +
                          "[1] Set về Ngày 1\n" +
                          "[7] Set về Ngày 7";

        if (SaveManager.Instance != null)
        {
            GameData data = SaveManager.Instance.PlayerData;
            debugInfo += $"\n\nStreak: Ngày {data.currentStreak + 1}/7";
            
            if (DailyRewardManager.Instance != null)
            {
                debugInfo += $"\nCó thể claim: {DailyRewardManager.Instance.CanClaimToday()}";
                debugInfo += $"\nReward: {DailyRewardManager.Instance.GetCurrentReward()}";
            }
        }

        GUI.Box(new Rect(10, 10, 300, 180), debugInfo, style);
    }
}
