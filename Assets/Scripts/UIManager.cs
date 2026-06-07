using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD (Góc màn hình)")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI hudScoreText;
    [SerializeField] private TextMeshProUGUI hudHighScoreText; // Thêm biến này

    [Header("GameOver UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalHighScoreText;

    private void OnEnable()
    {
        GameEventSystem.OnScoreChanged += UpdateScoreUI;
        GameEventSystem.OnHighScoreChanged += UpdateHighScoreUI;
    }

    private void OnDisable()
    {
        GameEventSystem.OnScoreChanged -= UpdateScoreUI;
        GameEventSystem.OnHighScoreChanged -= UpdateHighScoreUI;
    }

    private void UpdateScoreUI(int score)
    {
        // Cập nhật điểm hiện tại cho cả HUD và Bảng Game Over
        if (hudScoreText != null) hudScoreText.text = $"Score: {score}";
        if (finalScoreText != null) finalScoreText.text = $"SCORE: {score}";
    }

    private void UpdateHighScoreUI(int highScore)
    {
        // Cập nhật điểm cao nhất cho cả HUD và Bảng Game Over
        if (hudHighScoreText != null) hudHighScoreText.text = $"Best: {highScore}";
        if (finalHighScoreText != null) finalHighScoreText.text = $"BEST: {highScore}";
    }

    public void ShowGameOver()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}