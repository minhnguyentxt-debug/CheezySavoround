using UnityEngine;
using TMPro;
using System.Collections; 
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

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
        scoreText.text = "SCORE: " + score.ToString();
    }

    private void UpdateHighScoreUI(int highScore)
    {
        highScoreText.text = "BEST: " + highScore.ToString();
    }
    // Trong script GameOverUI
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        StartCoroutine(AnimateScale(0.5f));
    }

    private IEnumerator AnimateScale(float duration)
    {
        float elapsed = 0f;
        transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Dùng unscaled để chạy được khi Time.timeScale = 0
            float t = elapsed / duration;

            // Công thức tạo hiệu ứng "nảy" (EaseOutBack) thủ công
            float s = 1.70158f;
            float scale = t * t * ((s + 1) * t - s) + 1; // Công thức đơn giản hóa

            transform.localScale = Vector3.one * Mathf.Clamp01(scale);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}