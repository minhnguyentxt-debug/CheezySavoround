using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI usesText;
    public Button itemButton;

    public ItemData itemData { get; private set; } // Dữ liệu riêng của ô này
    private ItemManager itemManager;

    private void Start()
    {
        itemManager = FindAnyObjectByType<ItemManager>();
        if (itemButton != null) itemButton.onClick.AddListener(OnItemClick);
    }

    public void Setup(ItemData data)
    {
        // Tạo bản sao để mỗi ô có bộ đếm riêng, không ảnh hưởng ô khác
        itemData = Instantiate(data);
        // KHÔNG set currentUses = maxUses ở đây vì nó sẽ ghi đè dữ liệu đã lưu!
        // InventoryManager sẽ xử lý việc khởi tạo và load giá trị đã lưu

        iconImage.sprite = itemData.icon;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemData == null) return;

        usesText.text = itemData.currentUses.ToString();
        bool canUse = itemData.currentUses > 0;
        usesText.ForceMeshUpdate();
        itemButton.interactable = canUse;
        iconImage.color = canUse ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        Color customColor = new Color(0f, 0.8f, 0f, 1f); // Màu xanh lá hoặc chỉnh theo ý bạn
        usesText.color = (itemData.currentUses <= 1) ? Color.red : customColor;
    }

    private void OnItemClick()
    {
        if (itemData.currentUses > 0)
            itemManager.SelectItem(itemData);
        else
            Debug.Log("Đã hết lượt sử dụng!");
    }
}