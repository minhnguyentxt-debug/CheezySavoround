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

    // 4 hàm này dùng để gán vào dấu (+) của 4 Button
    public void UseRemoveItem() => itemManager.SelectItem(removeData);
    public void UseAddSauceItem() => itemManager.SelectItem(addSauceData);
    public void UseSwapItem() => itemManager.SelectItem(swapData);
    public void UseCreateItem() => itemManager.SelectItem(createData);
}