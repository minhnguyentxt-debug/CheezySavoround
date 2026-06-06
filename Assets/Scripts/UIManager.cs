using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private void OnEnable()
    {
        // Lắng nghe các event từ GameEventSystem
        GameEventSystem.OnScoreChanged += (score) => scoreText.text = $"Score: {score}";
        GameEventSystem.OnHighScoreChanged += (highScore) => highScoreText.text = $"Best: {highScore}";
    }

    private void OnDisable()
    {
        // Hủy lắng nghe để tránh lỗi
        GameEventSystem.OnScoreChanged -= (score) => scoreText.text = $"Score: {score}";
        GameEventSystem.OnHighScoreChanged -= (highScore) => highScoreText.text = $"Best: {highScore}";
    }
}