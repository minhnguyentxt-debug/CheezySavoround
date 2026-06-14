using UnityEngine;

/// <summary>
/// Gắn script này vào bất kỳ GameObject nào trong scene Gameplay.
/// Nó sẽ in ra Console tất cả thông tin quan trọng khi scene bắt đầu.
/// XÓA hoặc tắt script này sau khi test xong.
/// </summary>
public class LevelDebugger : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("===== LEVEL DEBUGGER =====");

        if (SaveManager.Instance != null)
            Debug.Log($"[SaveManager] currentLevel = {SaveManager.Instance.PlayerData.currentLevel} | Plates saved = {SaveManager.Instance.PlayerData.Plates.Count}");
        else
            Debug.LogWarning("[SaveManager] Instance là NULL!");

        if (LevelManager.Instance != null)
            Debug.Log($"[LevelManager] CurrentLevel = {LevelManager.Instance.CurrentLevel} | TargetScore = {LevelManager.Instance.TargetScore}");
        else
            Debug.LogWarning("[LevelManager] Instance là NULL! Chưa thêm LevelManager vào scene?");

        if (ScoreManager.Instance != null)
            Debug.Log($"[ScoreManager] CurrentScore = {ScoreManager.Instance.CurrentScore}");

        Debug.Log("==========================");
    }
}
