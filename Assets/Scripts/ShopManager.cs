using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private PizzaSkinData[] allSlicesSkins; // Kéo các ScriptableObject vào đây

    public void BuyOrSelectSkin(string skinId)
    {
        GameData data = SaveManager.Instance.PlayerData;
        PizzaSkinData selectedSkin = System.Array.Find(allSlicesSkins, s => s.skinId == skinId);

        if (selectedSkin == null) return;

        // Nếu đã mở khóa rồi -> Tiến hành Thay/Đổi skin
        if (data.unlockedSkinIds.Contains(skinId))
        {
            data.currentSkinId = skinId;
            SaveManager.Instance.SaveGame();

            // Bắn sự kiện thông báo đổi skin đĩa bánh ra toàn hệ thống
            GameEventSystem.OnSkinSelected?.Invoke(skinId);
            return;
        }

        // Nếu chưa mở khóa -> Kiểm tra tiền để mua
        if (data.gold >= selectedSkin.price)
        {
            data.gold -= selectedSkin.price;
            data.unlockedSkinIds.Add(skinId);
            data.currentSkinId = skinId;

            SaveManager.Instance.SaveGame();

            // Cập nhật UI Vàng và Đổi skin ngay lập tức
            GameEventSystem.OnGoldChanged?.Invoke(data.gold);
            GameEventSystem.OnSkinSelected?.Invoke(skinId);

            Debug.Log($"[Shop] Đã mua thành công skin: {selectedSkin.skinName}");
        }
        else
        {
            Debug.LogWarning("[Shop] Không đủ vàng để mua skin này!");
        }
    }
}