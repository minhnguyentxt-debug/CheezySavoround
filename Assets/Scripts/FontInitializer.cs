using UnityEngine;
using TMPro;

public class FontInitializer : MonoBehaviour
{
    public TMP_FontAsset mySDF_Font; // Kéo file Font SDF bạn đã tạo vào đây

    void Awake()
    {
        // Ép font phải load ngay khi game khởi động trên Main Thread
        if (mySDF_Font != null)
        {
            mySDF_Font.ReadFontAssetDefinition();
        }
    }
}