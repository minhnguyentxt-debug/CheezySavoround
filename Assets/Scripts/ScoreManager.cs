using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Current Stats (Tự động reset khi Replay)")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentGold = 0;

    [Header("Persistent Stats (Lưu giữ xuyên suốt qua PlayerPrefs)")]
    [SerializeField] private int highScore = 0;
    [SerializeField] private int coins = 0; // Tiền tệ để mua đồ trong Shop

    // Biến tạm để theo dõi mốc điểm đã đổi coin trong phiên chơi này
    private int lastCoinMilestone = 0;

    public int CurrentScore => currentScore;
    public int CurrentGold => currentGold;
    public int HighScore => highScore;
    public int Coins => coins;

    private const string HIGH_SCORE_KEY = "PizzaGame_HighScore";
    private const string COINS_KEY = "PizzaGame_Coins";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadData();
    }

    private void OnEnable()
    {
        GameEventSystem.OnPizzaCompleted += AddRewards;
    }

    private void OnDisable()
    {
        GameEventSystem.OnPizzaCompleted -= AddRewards;
    }

    // Biến cờ để tránh kích hoạt OnLevelComplete nhiều lần trong cùng một màn
    private bool levelCompleted = false;

    private void AddRewards(ToppingType topping)
    {
        AddScore(100, 5);
    }

    public void AddScore(int scoreAmount, int goldAmount)
    {
        currentScore += scoreAmount;
        currentGold += goldAmount;

        // --- LOGIC ĐỔI COIN TỰ ĐỘNG CỨ MỖI 300 ĐIỂM ---
        int currentMilestone = currentScore / 300;
        if (currentMilestone > lastCoinMilestone)
        {
            int milestonesGained = currentMilestone - lastCoinMilestone;
            int coinsToReward = milestonesGained * 5;
            AddCoins(coinsToReward);

            lastCoinMilestone = currentMilestone;
            Debug.Log($"[ScoreManager] Chúc mừng! Đạt mốc điểm, nhận được {coinsToReward} Coins!");
        }

        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }

        // Cập nhật toàn bộ UI HUD thời gian thực
        TriggerAllUIEvents();

        // --- CHECK ĐIỀU KIỆN THẮNG MÀN ---
        if (!levelCompleted && LevelManager.Instance != null)
        {
            int target = LevelManager.Instance.TargetScore;
            if (target > 0 && currentScore >= target)
            {
                levelCompleted = true;
                Debug.Log($"[ScoreManager] Đạt {currentScore}/{target} điểm – kích hoạt thắng màn!");
                GameEventSystem.OnLevelComplete?.Invoke();
            }
        }
    }

    // Hàm cộng coin công khai
    public void AddCoins(int amount)
    {
        coins += amount;
        SaveCoins();
        GameEventSystem.OnCoinsChanged?.Invoke(coins);
    }

    // Hàm trừ coin khi mua đồ ở Shop
    public bool TrySpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveCoins();
            GameEventSystem.OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    // --- HÀM BỔ SUNG: SỬA LỖI KHÔNG RESET ĐIỂM KHI BẤM REPLAY ---
    public void ResetScoreForNewGame()
    {
        currentScore = 0;
        currentGold = 0; // Reset cả vàng về 0 nếu vàng tính theo từng lượt chơi
        lastCoinMilestone = 0;
        levelCompleted = false; // Cho phép thắng màn lại ở màn mới

        // Ép các Text UI cập nhật ngay lập tức về mốc số 0 đầu game
        TriggerAllUIEvents();
        Debug.Log("[ScoreManager] Đã làm mới điểm số và vàng để bắt đầu ván mới thành công.");
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        coins = PlayerPrefs.GetInt(COINS_KEY, 0);

        lastCoinMilestone = currentScore / 300;

        InvokeInitialEvents();
    }

    public void InvokeInitialEvents()
    {
        TriggerAllUIEvents();
    }

    // Gom nhóm kích hoạt sự kiện để tránh viết lặp đi lặp lại code gán UI
    private void TriggerAllUIEvents()
    {
        GameEventSystem.OnScoreChanged?.Invoke(currentScore);
        GameEventSystem.OnGoldChanged?.Invoke(currentGold);
        GameEventSystem.OnHighScoreChanged?.Invoke(highScore);
        GameEventSystem.OnCoinsChanged?.Invoke(coins);
    }
}