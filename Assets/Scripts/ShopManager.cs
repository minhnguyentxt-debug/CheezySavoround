using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int price;
        public ItemEffectType effectType;
        public Button buyButton;
        public TextMeshProUGUI priceText;
        // Bạn có thể thêm biến ToppingType hoặc ItemType của bạn ở đây để kích hoạt tính năng vật phẩm sau khi mua
    }

    [Header("Shop UI Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI shopCoinText;

    [Header("4 Items Configuration")]
    [SerializeField] private ShopItem[] shopItems = new ShopItem[4];

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false); // Đầu game ẩn shop đi

        // Tự động gán effectType dựa trên itemName nếu trong Inspector chưa chọn đúng hoặc chưa chọn
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i] != null && !string.IsNullOrEmpty(shopItems[i].itemName))
            {
                string cleanName = shopItems[i].itemName.Replace(" ", "").ToLower();
                foreach (ItemEffectType type in System.Enum.GetValues(typeof(ItemEffectType)))
                {
                    if (type.ToString().ToLower() == cleanName || 
                        type.ToString().ToLower().Contains(cleanName) || 
                        cleanName.Contains(type.ToString().ToLower()))
                    {
                        shopItems[i].effectType = type;
                        break;
                    }
                }
            }
        }

        UpdateShopUI();
        InitItemPrices();
    }

    private void OnEnable()
    {
        GameEventSystem.OnCoinsChanged += OnCoinsUpdated;
    }

    private void OnDisable()
    {
        GameEventSystem.OnCoinsChanged -= OnCoinsUpdated;
    }

    private void OnCoinsUpdated(int currentCoins)
    {
        if (shopCoinText != null) shopCoinText.text = $"Coins: {currentCoins}";
        CheckButtonInteractable(); // Cập nhật xem nút nào bấm được hay bị xám màu
    }

    // Thiết lập hiển thị giá tiền lên các nút bấm trong game
    private void InitItemPrices()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i] != null && shopItems[i].priceText != null)
            {
                shopItems[i].priceText.text = $"{shopItems[i].price} C";
            }

            // Đăng ký sự kiện onClick cho từng nút bấm thông qua code cho đỡ phải kéo tay
            int index = i; // Tránh lỗi closure trong vòng lặp c#
            if (shopItems[i].buyButton != null)
            {
                shopItems[i].buyButton.onClick.RemoveAllListeners();
                shopItems[i].buyButton.onClick.AddListener(() => BuyItem(index));
            }
        }
        CheckButtonInteractable();
    }

    // Kiểm tra xem món nào đủ tiền mua, món nào không đủ thì làm xám nút
    private void CheckButtonInteractable()
    {
        if (ScoreManager.Instance == null) return;
        int currentCoins = ScoreManager.Instance.Coins;

        foreach (var item in shopItems)
        {
            if (item.buyButton != null)
            {
                item.buyButton.interactable = (currentCoins >= item.price);
            }
        }
    }

    // Hàm xử lý khi người chơi bấm nút mua item
    public void BuyItem(int itemIndex)
    {
        ShopItem item = shopItems[itemIndex];

        if (ScoreManager.Instance != null && ScoreManager.Instance.TrySpendCoins(item.price))
        {
            Debug.Log($"[Shop] Mua thành công Item: {item.itemName} với giá {item.price} Coins!");

            // VÙNG VIẾT CODE THƯỞNG ITEM:
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItemUsesDirectly(item.effectType, 1);
            }
            CheckButtonInteractable(); // Cập nhật lại trạng thái các nút sau khi trừ tiền
        }
        else
        {
            Debug.Log("[Shop] Không đủ Coins để mua vật phẩm này!");
        }
    }

    // --- CÁC HÀM CÔNG KHAI ĐỂ ĐÍNH VÀO NÚT BẤM MỞ/ĐÓNG SHOP ---
    public void ToggleShop()
    {
        if (shopPanel != null)
        {
            bool isActive = !shopPanel.activeSelf;
            shopPanel.SetActive(isActive);

            if (isActive)
            {
                UpdateShopUI();
                // Nếu ở scene Gameplay, bạn có thể cân nhắc Pause game khi mở shop bằng dòng dưới:
                Time.timeScale = 0f;
            }
            else
            {
                // Khi đóng shop thì trả lại thời gian chạy game bình thường
                Time.timeScale = 1f;
            }
        }
    }

    private void UpdateShopUI()
    {
        if (ScoreManager.Instance != null && shopCoinText != null)
        {
            shopCoinText.text = $"Coins: {ScoreManager.Instance.Coins}";
        }
    }
}