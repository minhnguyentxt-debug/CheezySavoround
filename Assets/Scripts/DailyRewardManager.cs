using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public void CheckDailyReward()
    {
        GameData data = SaveManager.Instance.PlayerData;

        // Lấy thời gian UTC hiện tại của hệ thống (An toàn hơn giờ Local rất nhiều)
        DateTime now = DateTime.UtcNow;

        if (string.IsNullOrEmpty(data.lastClaimedTime))
        {
            Debug.Log("[DailyReward] Bạn có thể nhận thưởng ngày đầu tiên!");
            return;
        }

        DateTime lastClaimed = DateTime.Parse(data.lastClaimedTime);
        TimeSpan difference = now.Date - lastClaimed.Date;

        if (difference.TotalDays >= 1)
        {
            if (difference.TotalDays == 1)
            {
                // Người chơi đăng nhập liên tiếp ngày tiếp theo -> Tăng chuỗi Streak
                data.currentStreak = (data.currentStreak + 1) % 7;
            }
            else
            {
                // Quá 1 ngày không đăng nhập -> Reset chuỗi về 0 (Ngày 1)
                data.currentStreak = 0;
            }
            Debug.Log($"[DailyReward] Có thể nhận quà! Chuỗi hiện tại: Ngày {data.currentStreak + 1}");
        }
        else
        {
            Debug.Log("[DailyReward] Hôm nay bạn đã nhận quà rồi, hãy quay lại vào ngày mai!");
        }
    }

    public void ClaimReward()
    {
        GameData data = SaveManager.Instance.PlayerData;

        // Thực hiện cộng vàng theo ngày chuỗi (Ví dụ ngày càng cao vàng càng nhiều)
        int rewardGold = (data.currentStreak + 1) * 10;
        data.gold += rewardGold;

        // Cập nhật lại ngày nhận quà cuối cùng theo giờ UTC chuẩn
        data.lastClaimedTime = DateTime.UtcNow.ToString();

        SaveManager.Instance.SaveGame();

        GameEventSystem.OnGoldChanged?.Invoke(data.gold);
        Debug.Log($"[DailyReward] Đã nhận {rewardGold} Vàng thành công!");
    }
}