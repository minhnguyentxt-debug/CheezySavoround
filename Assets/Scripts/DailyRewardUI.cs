using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimButton;
    
    [Header("Reward Display")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private GameObject notificationDot; // Chấm đỏ thông báo có thưởng
    
    [Header("Day Icons (7 ô hiển thị 7 ngày)")]
    [SerializeField] private DailyRewardDayUI[] dayIcons;
    
    private bool canClaim = false;
    private bool isInitialized = false;

    private void Awake()
    {
        // QUAN TRỌNG: Ẩn panel ngay trong Awake() để đảm bảo nó chạy trước mọi thứ
        // Điều này ngăn panel tự động hiện khi chuyển scene
        if (rewardPanel != null) 
        {
            rewardPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Kiểm tra Canvas setup để đảm bảo có thể tương tác
        ValidateCanvasSetup();

        // Gán sự kiện cho các nút
        if (openButton != null)
            openButton.onClick.AddListener(OpenRewardPanel);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseRewardPanel);
        
        if (claimButton != null)
            claimButton.onClick.AddListener(OnClaimButtonClicked);
        
        // Ẩn notification dot ban đầu
        if (notificationDot != null)
            notificationDot.SetActive(false);
        
        // Đánh dấu đã khởi tạo
        isInitialized = true;
        
        // Cập nhật trạng thái UI
        UpdateOpenButtonState();
        
        // Nếu DailyRewardManager đã sẵn sàng, kiểm tra trạng thái thưởng
        if (DailyRewardManager.Instance != null)
        {
            canClaim = DailyRewardManager.Instance.CanClaimToday();
            if (notificationDot != null)
                notificationDot.SetActive(canClaim);
            Debug.Log($"[DailyRewardUI] Khởi tạo xong - Có thể claim: {canClaim}");
        }
    }

    private void OnEnable()
    {
        // Subscribe to events
        GameEventSystem.OnDailyRewardAvailable += OnRewardAvailabilityChanged;
        GameEventSystem.OnDailyRewardClaimed += OnRewardClaimed;
        
        // Cập nhật trạng thái nút mở nếu đã khởi tạo
        if (isInitialized)
        {
            UpdateOpenButtonState();
            UpdateRewardDisplay();
        }
    }

    private void OnDisable()
    {
        GameEventSystem.OnDailyRewardAvailable -= OnRewardAvailabilityChanged;
        GameEventSystem.OnDailyRewardClaimed -= OnRewardClaimed;
    }

    private void OnRewardAvailabilityChanged(bool available)
    {
        canClaim = available;
        
        // Hiện/ẩn notification dot
        if (notificationDot != null)
            notificationDot.SetActive(available);
        
        // Cập nhật trạng thái nút mở
        UpdateOpenButtonState();
        
        Debug.Log($"[DailyRewardUI] Thưởng ngày: {(available ? "CÓ THỂ NHẬN" : "Chưa có")}");
    }

    private void OnRewardClaimed(int amount)
    {
        Debug.Log($"[DailyRewardUI] Đã nhận {amount} coins!");
        
        // Refresh UI sau khi nhận thưởng
        UpdateRewardDisplay();
        
        // Có thể thêm hiệu ứng hoặc thông báo ở đây
    }

    public void OpenRewardPanel()
    {
        if (DailyRewardManager.Instance == null)
        {
            Debug.LogWarning("[DailyRewardUI] DailyRewardManager chưa tồn tại!");
            return;
        }

        if (!DailyRewardManager.Instance.IsEnabled)
        {
            Debug.LogWarning("[DailyRewardUI] Hệ thống thưởng ngày đang tắt!");
            return;
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            UpdateRewardDisplay();
            Debug.Log("[DailyRewardUI] ✓ Đã mở Daily Reward panel");
        }
        else
        {
            Debug.LogError("[DailyRewardUI] Reward Panel reference bị null!");
        }
    }

    public void CloseRewardPanel()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    /// <summary>
    /// Kiểm tra Canvas setup để đảm bảo UI có thể tương tác được
    /// </summary>
    private void ValidateCanvasSetup()
    {
        if (rewardPanel == null)
        {
            Debug.LogError("[DailyRewardUI] Reward Panel chưa được gán! Vui lòng kéo Panel vào Inspector.");
            return;
        }

        // Kiểm tra Canvas và GraphicRaycaster
        Canvas canvas = rewardPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[DailyRewardUI] Không tìm thấy Canvas cho Reward Panel!");
            return;
        }

        // Kiểm tra GraphicRaycaster (cần thiết cho UI interaction)
        UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogWarning("[DailyRewardUI] Canvas thiếu GraphicRaycaster! Tự động thêm component...");
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Kiểm tra EventSystem (cần thiết cho toàn bộ UI)
        UnityEngine.EventSystems.EventSystem eventSystem = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("[DailyRewardUI] Không tìm thấy EventSystem trong scene! UI sẽ không thể tương tác được. Vui lòng thêm EventSystem vào scene.");
        }

        Debug.Log("[DailyRewardUI] ✓ Canvas setup hợp lệ - UI có thể tương tác");
    }

    private void OnClaimButtonClicked()
    {
        if (DailyRewardManager.Instance == null) return;
        
        if (DailyRewardManager.Instance.CanClaimToday())
        {
            DailyRewardManager.Instance.ClaimReward();
            
            // Cập nhật UI ngay sau khi claim
            UpdateRewardDisplay();
            
            // Tùy chọn: Tự động đóng panel sau khi nhận
            // CloseRewardPanel();
        }
        else
        {
            Debug.Log("[DailyRewardUI] Không thể nhận thưởng lúc này!");
        }
    }

    private void UpdateRewardDisplay()
    {
        if (DailyRewardManager.Instance == null) return;

        int currentStreak = DailyRewardManager.Instance.CurrentStreak;
        int currentReward = DailyRewardManager.Instance.GetCurrentReward();
        bool canClaimNow = DailyRewardManager.Instance.CanClaimToday();

        // Cập nhật text
        if (titleText != null)
            titleText.text = canClaimNow ? "THƯỞNG NGÀY!" : "Quay lại ngày mai";
        
        if (rewardAmountText != null)
            rewardAmountText.text = $"{currentReward} Coins";
        
        if (streakText != null)
            streakText.text = $"Ngày {currentStreak + 1}/7";

        // Cập nhật trạng thái nút Claim
        if (claimButton != null)
        {
            claimButton.interactable = canClaimNow;
            
            // Đổi text nút nếu có TextMeshProUGUI
            TextMeshProUGUI buttonText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canClaimNow ? "NHẬN QUÀ" : "Đã nhận hôm nay";
            }
        }

        // Cập nhật 7 ô ngày
        UpdateDayIcons(currentStreak, canClaimNow);
    }

    private void UpdateDayIcons(int currentStreak, bool canClaimNow)
    {
        if (dayIcons == null || dayIcons.Length == 0) return;
        if (DailyRewardManager.Instance == null) return;

        int[] rewards = DailyRewardManager.Instance.DailyRewards;

        for (int i = 0; i < dayIcons.Length && i < 7; i++)
        {
            if (dayIcons[i] == null) continue;

            // Lấy reward cho ngày này
            int rewardAmount = (i < rewards.Length) ? rewards[i] : 0;
            
            // Nếu là ngày thứ 7, thêm weekly bonus
            if (i == 6)
                rewardAmount += 200; // Có thể lấy từ DailyRewardManager nếu cần

            // Xác định trạng thái của ngày này
            DailyRewardDayState state;
            if (i < currentStreak)
            {
                // Đã claim những ngày trước
                state = DailyRewardDayState.Claimed;
            }
            else if (i == currentStreak)
            {
                // Ngày hiện tại
                state = canClaimNow ? DailyRewardDayState.Available : DailyRewardDayState.Claimed;
            }
            else
            {
                // Ngày tương lai
                state = DailyRewardDayState.Locked;
            }

            dayIcons[i].SetupDay(i + 1, rewardAmount, state);
        }
    }

    private void UpdateOpenButtonState()
    {
        if (openButton == null) return;
        
        // Bật/tắt nút theo trạng thái của DailyRewardManager
        bool isEnabled = DailyRewardManager.Instance != null && DailyRewardManager.Instance.IsEnabled;
        openButton.gameObject.SetActive(isEnabled);
    }
}

// Enum cho trạng thái các ngày
public enum DailyRewardDayState
{
    Locked,    // Chưa mở khóa (ngày tương lai)
    Available, // Có thể nhận (ngày hôm nay)
    Claimed    // Đã nhận rồi (ngày quá khứ)
}

// Class phụ để quản lý từng ô ngày (tùy chọn)
[System.Serializable]
public class DailyRewardDayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayNumberText;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    
    [Header("Màu sắc theo trạng thái")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color availableColor = Color.yellow;
    [SerializeField] private Color claimedColor = Color.green;

    public void SetupDay(int dayNumber, int rewardAmount, DailyRewardDayState state)
    {
        // Cập nhật text
        if (dayNumberText != null)
            dayNumberText.text = $"Ngày {dayNumber}";
        
        if (rewardAmountText != null)
            rewardAmountText.text = $"{rewardAmount}";

        // Cập nhật màu sắc theo trạng thái
        Color targetColor = lockedColor;
        switch (state)
        {
            case DailyRewardDayState.Locked:
                targetColor = lockedColor;
                break;
            case DailyRewardDayState.Available:
                targetColor = availableColor;
                break;
            case DailyRewardDayState.Claimed:
                targetColor = claimedColor;
                break;
        }

        if (backgroundImage != null)
            backgroundImage.color = targetColor;
        
        if (iconImage != null)
        {
            // Có thể thêm logic để thay đổi icon
            iconImage.color = targetColor;
        }
    }
}
