using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Current Stats")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentGold = 0;

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện khi có bánh hoàn thành
        GameEventSystem.OnPizzaCompleted += AddRewards;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh tràn bộ nhớ (Memory Leak)
        GameEventSystem.OnPizzaCompleted -= AddRewards;
    }

    private void AddRewards(ToppingType topping)
    {
        currentScore += 10; // Mỗi đĩa hoàn thành +10 điểm
        currentGold += 5;   // Mỗi đĩa hoàn thành +5 vàng

        // Phát sự kiện để UI tự động cập nhật theo
        GameEventSystem.OnScoreChanged?.Invoke(currentScore);
        GameEventSystem.OnGoldChanged?.Invoke(currentGold);

        Debug.Log($"[ScoreManager] Điểm hiện tại: {currentScore} | Vàng: {currentGold}");

        // Kế hoạch Ngày 1 - Ngày 2: Hàm Lưu JSON sẽ được gọi ở đây
        // SaveDataToJSON(); 
    }
}