using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD (Góc màn hình)")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI hudScoreText;
    [SerializeField] private TextMeshProUGUI hudHighScoreText;

    [Header("GameOver UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalHighScoreText;

    private void Start()
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Nếu ScoreManager đã tồn tại từ trước, lấy thẳng giá trị của nó để hiển thị lên HUD
        if (ScoreManager.Instance != null)
        {
            UpdateScoreUI(ScoreManager.Instance.CurrentScore);
            UpdateHighScoreUI(ScoreManager.Instance.HighScore);
        }
        else
        {
            // Phòng hờ nếu chạy Scene độc lập không có ScoreManager sẵn
            if (hudScoreText != null) hudScoreText.text = "Score: 0";
            if (hudHighScoreText != null) hudHighScoreText.text = "Best: 0";
        }
    }

    private void OnEnable()
    {
        // Đăng ký nhận sự kiện khi điểm thay đổi
        GameEventSystem.OnScoreChanged += UpdateScoreUI;
        GameEventSystem.OnHighScoreChanged += UpdateHighScoreUI;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh lỗi rò rỉ bộ nhớ (Memory Leak)
        GameEventSystem.OnScoreChanged -= UpdateScoreUI;
        GameEventSystem.OnHighScoreChanged -= UpdateHighScoreUI;
    }

    private void UpdateScoreUI(int score)
    {
        // Cập nhật điểm hiện tại đồng thời cho cả HUD và Bảng Game Over
        if (hudScoreText != null) hudScoreText.text = $"Score: {score}";
        if (finalScoreText != null) finalScoreText.text = $"SCORE: {score}";
    }

    private void UpdateHighScoreUI(int highScore)
    {
        // Cập nhật điểm cao nhất đồng thời cho cả HUD và Bảng Game Over
        if (hudHighScoreText != null) hudHighScoreText.text = $"Best: {highScore}";
        if (finalHighScoreText != null) finalHighScoreText.text = $"BEST: {highScore}";
    }

    public void ShowGameOver()
    {
        // Khi thua: Ẩn HUD góc màn hình đi và Hiện bảng GameOver lên
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}