using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // Tạo cơ chế Singleton giống ScoreManager
    public static InventoryManager Instance { get; private set; }

    public ItemSlotUI[] slots; // Kéo các ô từ Inspector vào
    public List<ItemData> startingItems; // Kéo các file ItemData vào

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[Inventory] Start() - Bắt đầu khởi tạo inventory");
        
        // Bước 1: Setup các slots (tạo clone của ItemData)
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < startingItems.Count)
            {
                slots[i].Setup(startingItems[i]);
                Debug.Log($"[Inventory] Setup slot {i}: {startingItems[i].itemName}, currentUses sau Setup = {slots[i].itemData.currentUses}");
            }
        }

        // Bước 2: Load dữ liệu đã lưu TRƯỚC (quan trọng!)
        // Trả về danh sách tên các item đã load thành công
        HashSet<string> loadedItems = LoadItemUsages();

        // Bước 3: Khởi tạo CÁC ITEM CHƯA CÓ DATA về maxUses
        // KHÔNG khởi tạo item đã load (kể cả nếu currentUses = 0 hợp lệ)
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemData != null && 
                !loadedItems.Contains(slots[i].itemData.itemName) && 
                slots[i].itemData.currentUses == 0)
            {
                slots[i].itemData.currentUses = slots[i].itemData.maxUses;
                Debug.Log($"[Inventory] Khởi tạo slot {i} ({slots[i].itemData.itemName}) về maxUses: {slots[i].itemData.maxUses}");
            }
        }

        // Bước 4: Cập nhật UI để hiển thị số lượng đúng
        UpdateAllItemUI();
        
        Debug.Log("[Inventory] Start() - Hoàn tất khởi tạo inventory");
    }

    /// <summary>
    /// Hàm công khai để ShopManager gọi thẳng vào khi mua thành công
    /// </summary>
    public void AddItemUsesDirectly(ItemEffectType effectType, int amount)
    {
        Debug.Log($"[Inventory] Shop vừa gọi lệnh cộng đồ cho loại hiệu ứng: {effectType}");

        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("[Inventory] LỖI: Mảng 'slots' đang trống rỗng! Bạn đã kéo các ô UI vào Inspector chưa?");
            return;
        }

        bool foundItem = false;

        // Duyệt trực tiếp qua mảng các ô slots hiển thị trên UI
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemData != null)
            {
                // So sánh loại hiệu ứng của bản Clone trong ô
                if (slots[i].itemData.effectType == effectType)
                {
                    slots[i].itemData.currentUses += amount; // Cộng lượt dùng
                    slots[i].UpdateUI(); // Ép ô UI vẽ lại số mới

                    Debug.Log($"[Inventory] THÀNH CÔNG! Đã cộng {amount} lần dùng vào ô số {i}. Số lượt mới: {slots[i].itemData.currentUses}");
                    foundItem = true;
                    
                    // Lưu ngay sau khi thay đổi
                    SaveItemUsages();
                    break;
                }
            }
        }

        if (!foundItem)
        {
            Debug.LogWarning($"[Inventory] Không tìm thấy ô Slot nào đang chứa Item có hiệu ứng {effectType} để cộng điểm!");
        }
    }

    /// <summary>
    /// Cập nhật UI của tất cả các slot
    /// </summary>
    private void UpdateAllItemUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].UpdateUI();
            }
        }
    }

    /// <summary>
    /// Lưu số lần sử dụng hiện tại của tất cả items và coins
    /// </summary>
    public void SaveItemUsages()
    {
        Debug.Log("[Inventory] ========== BẮT ĐẦU LƯU ITEMS ==========");
        
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[Inventory] ❌ SaveManager.Instance = NULL! Không thể lưu!");
            return;
        }

        GameData data = SaveManager.Instance.PlayerData;
        
        // Xóa dữ liệu cũ
        data.itemUsages.Clear();

        // Lưu số lần sử dụng của từng item
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemData != null)
            {
                string itemName = slots[i].itemData.itemName;
                int currentUses = slots[i].itemData.currentUses;
                
                ItemUsageData usageData = new ItemUsageData(itemName, currentUses);
                data.itemUsages.Add(usageData);
                
                Debug.Log($"[Inventory] 💾 Lưu slot {i}: {itemName} = {currentUses} uses");
            }
        }

        // Lưu số coins từ ScoreManager
        if (ScoreManager.Instance != null)
        {
            data.coins = ScoreManager.Instance.Coins;
        }

        // Ghi vào file
        SaveManager.Instance.SaveGame();
        
        Debug.Log($"[Inventory] ✓ Đã lưu {data.itemUsages.Count} items, {data.coins} coins vào file!");
        Debug.Log("[Inventory] ========== KẾT THÚC LƯU ITEMS ==========");
    }

    /// <summary>
    /// Load số lần sử dụng đã lưu của các items và coins
    /// Trả về HashSet chứa tên các item đã load thành công
    /// </summary>
    private HashSet<string> LoadItemUsages()
    {
        Debug.Log("[Inventory] ========== BẮT ĐẦU LOAD ITEMS ==========");
        HashSet<string> loadedItems = new HashSet<string>();
        
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[Inventory] ❌ SaveManager.Instance = NULL! Không thể load!");
            return loadedItems;
        }

        GameData data = SaveManager.Instance.PlayerData;

        if (data.itemUsages == null || data.itemUsages.Count == 0)
        {
            Debug.LogWarning("[Inventory] ⚠️ data.itemUsages RỖNG! Không có dữ liệu để load.");
            return loadedItems;
        }

        Debug.Log($"[Inventory] 📂 Tìm thấy {data.itemUsages.Count} items trong save file:");
        foreach (var saved in data.itemUsages)
        {
            Debug.Log($"[Inventory]   - {saved.itemName}: {saved.currentUses} uses");
        }

        // Khôi phục số lần sử dụng cho từng item theo tên
        foreach (var savedUsage in data.itemUsages)
        {
            bool found = false;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].itemData != null)
                {
                    if (slots[i].itemData.itemName == savedUsage.itemName)
                    {
                        int oldValue = slots[i].itemData.currentUses;
                        slots[i].itemData.currentUses = savedUsage.currentUses;
                        loadedItems.Add(savedUsage.itemName);
                        Debug.Log($"[Inventory] ✓ Khôi phục slot {i}: {savedUsage.itemName} từ {oldValue} → {savedUsage.currentUses}");
                        found = true;
                        break;
                    }
                }
            }
            
            if (!found)
            {
                Debug.LogWarning($"[Inventory] ⚠️ KHÔNG tìm thấy slot cho {savedUsage.itemName}!");
            }
        }

        Debug.Log($"[Inventory] ✓ Đã load {loadedItems.Count}/{data.itemUsages.Count} items, {data.coins} coins!");
        Debug.Log("[Inventory] ========== KẾT THÚC LOAD ITEMS ==========");
        return loadedItems;
    }
}