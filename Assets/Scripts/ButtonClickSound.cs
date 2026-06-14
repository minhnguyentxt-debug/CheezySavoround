using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tự động phát âm thanh khi button được click
/// Attach component này vào bất kỳ Button nào để thêm click sound
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        // Lấy Button component
        button = GetComponent<Button>();

        // Thêm listener để phát âm thanh khi click
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDestroy()
    {
        // Xóa listener khi destroy để tránh memory leak
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    /// <summary>
    /// Phát âm thanh click button
    /// </summary>
    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }
    }
}
