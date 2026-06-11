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
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < startingItems.Count)
            {
                slots[i].Setup(startingItems[i]);
            }
        }
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
                    break;
                }
            }
        }

        if (!foundItem)
        {
            Debug.LogWarning($"[Inventory] Không tìm thấy ô Slot nào đang chứa Item có hiệu ứng {effectType} để cộng điểm!");
        }
    }
}