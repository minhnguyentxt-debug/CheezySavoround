using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script đơn giản để gán vào nút Daily Reward
/// Tự động ẩn/hiện nút khi hệ thống bật/tắt
/// Hiển thị notification dot khi có thưởng
/// </summary>
public class DailyRewardButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject notificationDot;
    [SerializeField] private DailyRewardUI rewardUI;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnButtonClicked);

        UpdateVisibility();
        
        if (notificationDot != null)
            notificationDot.SetActive(false);
    }

    private void OnEnable()
    {
        GameEventSystem.OnDailyRewardAvailable += OnRewardAvailabilityChanged;
    }

    private void OnDisable()
    {
        GameEventSystem.OnDailyRewardAvailable -= OnRewardAvailabilityChanged;
    }

    private void OnButtonClicked()
    {
        if (rewardUI != null)
        {
            rewardUI.OpenRewardPanel();
        }
        else
        {
            Debug.LogWarning("[DailyRewardButton] Chưa gán DailyRewardUI reference!");
        }
    }

    private void OnRewardAvailabilityChanged(bool available)
    {
        // Hiện/ẩn chấm thông báo
        if (notificationDot != null)
            notificationDot.SetActive(available);
    }

    private void UpdateVisibility()
    {
        // Ẩn nút nếu hệ thống bị tắt
        if (DailyRewardManager.Instance != null)
        {
            gameObject.SetActive(DailyRewardManager.Instance.IsEnabled);
        }
    }

    /// <summary>
    /// Gọi từ Inspector hoặc code để bật/tắt hệ thống
    /// </summary>
    public void ToggleDailyRewardSystem(bool enabled)
    {
        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.SetEnabled(enabled);
            UpdateVisibility();
        }
    }
}
