using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public ItemData selectedItem { get; private set; }
    private bool isSelectingTarget = false;
    public Button cancelButton;

    private Vector2Int? firstSwapPos = null;
    private Vector2Int? firstCreatePos = null;

    public void SelectItem(ItemData item)
    {
        selectedItem = item;
        isSelectingTarget = true;
        if (cancelButton != null) cancelButton.gameObject.SetActive(true);
        Cursor.SetCursor(item.icon.texture, Vector2.zero, CursorMode.Auto);
    }

    public void ExecuteEffect(int x, int z)
    {
        if (!isSelectingTarget || selectedItem == null) return;

        GridManager grid = FindAnyObjectByType<GridManager>();
        bool effectSuccessful = false;

        switch (selectedItem.effectType)
        {
            case ItemEffectType.RemovePizza:
                grid.RemovePlateAt(x, z);
                effectSuccessful = true;
                break;
            case ItemEffectType.AddSauce:
                grid.AddSauceToPlate(x, z);
                effectSuccessful = true;
                break;
            case ItemEffectType.CreatePizza:
                // Xử lý hiệu ứng 2 bước...
                if (firstCreatePos != null && grid.CanPlacePlate(x, z))
                {
                    if (Mathf.Abs(x - firstCreatePos.Value.x) + Mathf.Abs(z - firstCreatePos.Value.y) == 1)
                    {
                        grid.CreatePerfectFitPlateAt(firstCreatePos.Value.x, firstCreatePos.Value.y, x, z);
                        firstCreatePos = null; effectSuccessful = true;
                    }
                }
                else if (firstCreatePos == null)
                {
                    var plate = grid.GetPlateAt(x, z);
                    if (plate != null && plate.GetSlices().Count < 6) firstCreatePos = new Vector2Int(x, z);
                    return;
                }
                break;
            case ItemEffectType.SwapPosition:
                if (firstSwapPos != null) { grid.SwapPlates(firstSwapPos.Value.x, firstSwapPos.Value.y, x, z); firstSwapPos = null; effectSuccessful = true; }
                else { firstSwapPos = new Vector2Int(x, z); return; }
                break;
        }

        if (effectSuccessful)
        {
            selectedItem.currentUses--;
            UpdateAllItemUI();
            ResetItemSelection();
        }
    }

    private void UpdateAllItemUI()
    {
        foreach (var slot in FindObjectsByType<ItemSlotUI>(FindObjectsSortMode.None))
            slot.UpdateUI();
    }
    public void CancelSelection() // Phải có từ khóa 'public'
    {
        Debug.Log("Nút Hủy đã được nhấn!"); // Thêm log này để test
        ResetItemSelection();
    }
    public void ResetItemSelection()
    {
        selectedItem = null;
        isSelectingTarget = false;
        firstSwapPos = null; firstCreatePos = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
    }
}