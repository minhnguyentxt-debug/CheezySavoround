using UnityEngine;

public class ItemMenuHandler : MonoBehaviour
{
    // Kéo ItemManager vào đây trong Inspector
    public ItemManager itemManager;

    // Các biến ItemData để gán trong Inspector
    public ItemData removeData;
    public ItemData addSauceData;
    public ItemData swapData;
    public ItemData createData;

    // 4 hàm này dùng để gán vào dấu (+) của 4 Button, lấy từ Inventory để đồng bộ số lượt dùng
    public void UseRemoveItem() => SelectItemFromInventory(ItemEffectType.RemovePizza, removeData);
    public void UseAddSauceItem() => SelectItemFromInventory(ItemEffectType.AddSauce, addSauceData);
    public void UseSwapItem() => SelectItemFromInventory(ItemEffectType.SwapPosition, swapData);
    public void UseCreateItem() => SelectItemFromInventory(ItemEffectType.CreatePizza, createData);

    private void SelectItemFromInventory(ItemEffectType effectType, ItemData fallbackData)
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.slots != null)
        {
            foreach (var slot in InventoryManager.Instance.slots)
            {
                if (slot != null && slot.itemData != null && slot.itemData.effectType == effectType)
                {
                    if (slot.itemData.currentUses > 0)
                    {
                        itemManager.SelectItem(slot.itemData);
                    }
                    else
                    {
                        Debug.Log("Đã hết lượt sử dụng!");
                    }
                    return;
                }
            }
        }

        // Fallback if InventoryManager is not found or has no matching item
        if (fallbackData != null)
        {
            if (fallbackData.currentUses > 0)
            {
                itemManager.SelectItem(fallbackData);
            }
            else
            {
                Debug.Log("Đã hết lượt sử dụng!");
            }
        }
    }
}