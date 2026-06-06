using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Current Stats")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentGold = 0;
    [SerializeField] private int highScore = 0; // Thêm biến lưu điểm cao nhất

    // Key định danh để lưu điểm cao nhất vào PlayerPrefs tạm thời trước khi bạn lên hệ thống JSON
    private const string HIGH_SCORE_KEY = "PizzaGame_HighScore";

    private void Start()
    {
        // 1. Tải điểm cao nhất từ các phiên chơi trước lên
        LoadHighScore();
    }

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
        // 2. Thay đổi logic: Cộng 100 điểm theo yêu cầu mới của bạn (Vàng giữ nguyên +5 hoặc tùy chỉnh)
        currentScore += 100;
        currentGold += 5;

        // 3. Kiểm tra và cập nhật kỷ lục điểm cao nhất
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore(); // Lưu lại ngay lập tức vào máy
        }

        // Phát sự kiện để UI tự động cập nhật theo
        GameEventSystem.OnScoreChanged?.Invoke(currentScore);
        GameEventSystem.OnGoldChanged?.Invoke(currentGold);

        // PHÁT THÊM SỰ KIỆN: Báo cho UI biết Điểm cao nhất đã thay đổi để cập nhật lên màn hình
        // (Bạn nhớ thêm dòng delegate này vào file GameEventSystem.cs của bạn nhé)
        GameEventSystem.OnHighScoreChanged?.Invoke(highScore);

        Debug.Log($"[ScoreManager] Điểm hiện tại: {currentScore} | Kỷ lục: {highScore} | Vàng: {currentGold}");

        // Kế hoạch Ngày 1 - Ngày 2: Hàm Lưu JSON sẽ được gọi ở đây
        // SaveDataToJSON(); 
    }

    /// <summary>
    /// Lưu điểm cao nhất vào bộ nhớ máy (PlayerPrefs)
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Đọc điểm cao nhất khi vừa vào game, đồng thời bắn sự kiện cập nhật UI đầu game luôn
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        // Bắn điểm số ban đầu ra UI khi vừa mở game
        GameEventSystem.OnScoreChanged?.Invoke(currentScore);
        GameEventSystem.OnGoldChanged?.Invoke(currentGold);
        GameEventSystem.OnHighScoreChanged?.Invoke(highScore);
    }
}