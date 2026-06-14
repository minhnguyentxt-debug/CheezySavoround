using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script debug để reset dữ liệu trong quá trình test
/// Attach vào bất kỳ GameObject nào trong scene
/// </summary>
public class DebugResetHelper : MonoBehaviour
{
    [Header("Phím Tắt Debug")]
    [Tooltip("Nhấn để reset TẤT CẢ data (coins + items + progress)")]
    public KeyCode resetAllKey = KeyCode.F9;
    
    [Tooltip("Nhấn để reset CHỈ items, giữ coins")]
    public KeyCode resetItemsKey = KeyCode.F10;
    
    [Tooltip("Nhấn để xóa file save vật lý")]
    public KeyCode deleteFileKey = KeyCode.F11;
    
    [Header("Tùy Chọn")]
    [Tooltip("Tự động reload scene sau khi reset để thấy kết quả ngay")]
    public bool autoReloadSceneAfterReset = true;

    void Update()
    {
        // Chỉ hoạt động trong Unity Editor hoặc Development Build
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        if (Input.GetKeyDown(resetAllKey))
        {
            ResetAllData();
        }
        
        if (Input.GetKeyDown(resetItemsKey))
        {
            ResetItemsOnly();
        }
        
        if (Input.GetKeyDown(deleteFileKey))
        {
            DeleteSaveFile();
        }
        
        #endif
    }

    /// <summary>
    /// Gọi từ Inspector hoặc code để reset TẤT CẢ
    /// </summary>
    [ContextMenu("Reset ALL Data")]
    public void ResetAllData()
    {
        Debug.LogWarning("[DebugHelper] Đang reset TOÀN BỘ dữ liệu...");
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetAllData();
            
            if (autoReloadSceneAfterReset)
            {
                Debug.Log("[DebugHelper] Reload scene sau 0.5 giây...");
                Invoke(nameof(ReloadCurrentScene), 0.5f);
            }
        }
        else
        {
            Debug.LogError("[DebugHelper] Không tìm thấy SaveManager!");
        }
    }

    /// <summary>
    /// Gọi từ Inspector hoặc code để reset CHỈ items
    /// </summary>
    [ContextMenu("Reset Items Only")]
    public void ResetItemsOnly()
    {
        Debug.LogWarning("[DebugHelper] Đang reset CHỈ items...");
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetItemsOnly();
            
            if (autoReloadSceneAfterReset)
            {
                Debug.Log("[DebugHelper] Reload scene sau 0.5 giây...");
                Invoke(nameof(ReloadCurrentScene), 0.5f);
            }
        }
        else
        {
            Debug.LogError("[DebugHelper] Không tìm thấy SaveManager!");
        }
    }

    /// <summary>
    /// Gọi từ Inspector hoặc code để xóa file save
    /// </summary>
    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        Debug.LogWarning("[DebugHelper] Đang xóa file save...");
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile();
            
            if (autoReloadSceneAfterReset)
            {
                Debug.Log("[DebugHelper] Reload scene sau 0.5 giây...");
                Invoke(nameof(ReloadCurrentScene), 0.5f);
            }
        }
        else
        {
            Debug.LogError("[DebugHelper] Không tìm thấy SaveManager!");
        }
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
