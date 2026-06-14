using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility script để tự động thêm ButtonClickSound cho tất cả buttons
/// </summary>
public class ButtonSoundUtility : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// Thêm ButtonClickSound cho TẤT CẢ buttons trong scene hiện tại
    /// Cách dùng: Right-click script này trong Inspector → Add Click Sound To All Buttons
    /// </summary>
    [ContextMenu("Add Click Sound To All Buttons In Scene")]
    public void AddClickSoundToAllButtons()
    {
        // Tìm tất cả buttons trong scene (bao gồm cả inactive)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        
        int addedCount = 0;
        int skippedCount = 0;

        foreach (Button button in allButtons)
        {
            // Kiểm tra button có thuộc scene hiện tại không (không phải prefab)
            if (button.gameObject.scene.isLoaded)
            {
                // Kiểm tra đã có ButtonClickSound chưa
                ButtonClickSound existingSound = button.GetComponent<ButtonClickSound>();
                
                if (existingSound == null)
                {
                    // Thêm component
                    Undo.AddComponent<ButtonClickSound>(button.gameObject);
                    addedCount++;
                    Debug.Log($"✓ Đã thêm click sound cho button: {GetButtonPath(button.transform)}");
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        Debug.Log($"<color=green>[ButtonSoundUtility] HOÀN TẤT!</color>");
        Debug.Log($"<color=cyan>→ Đã thêm click sound cho {addedCount} buttons</color>");
        Debug.Log($"<color=yellow>→ Bỏ qua {skippedCount} buttons (đã có click sound)</color>");
        
        if (addedCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Thành Công!", 
                $"Đã thêm click sound cho {addedCount} buttons!\n\n" +
                $"Bỏ qua: {skippedCount} buttons (đã có)\n\n" +
                "Nhớ assign Button Click Sound clip vào AudioManager!",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Không Có Gì Thay Đổi", 
                $"Tất cả {skippedCount} buttons đã có click sound rồi!",
                "OK"
            );
        }
    }

    /// <summary>
    /// Xóa ButtonClickSound khỏi TẤT CẢ buttons trong scene
    /// </summary>
    [ContextMenu("Remove Click Sound From All Buttons In Scene")]
    public void RemoveClickSoundFromAllButtons()
    {
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        int removedCount = 0;

        foreach (Button button in allButtons)
        {
            if (button.gameObject.scene.isLoaded)
            {
                ButtonClickSound existingSound = button.GetComponent<ButtonClickSound>();
                
                if (existingSound != null)
                {
                    Undo.DestroyObjectImmediate(existingSound);
                    removedCount++;
                    Debug.Log($"✗ Đã xóa click sound khỏi button: {GetButtonPath(button.transform)}");
                }
            }
        }

        Debug.Log($"<color=orange>[ButtonSoundUtility] Đã xóa click sound khỏi {removedCount} buttons</color>");
        
        if (removedCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Hoàn Tất", 
                $"Đã xóa click sound khỏi {removedCount} buttons!",
                "OK"
            );
        }
    }

    /// <summary>
    /// Lấy đường dẫn đầy đủ của button trong hierarchy
    /// </summary>
    private string GetButtonPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
#endif
}
