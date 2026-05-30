using System.Collections.Generic;
using UnityEngine;

public class PizzaPlate : MonoBehaviour
{
    [Header("Pizza Slices Data")]
    // Đĩa quản lý một danh sách các lát bánh đặt trên nó (tối đa 6 lát)
    [SerializeField] private List<ToppingType> slices = new List<ToppingType>();

    // Tọa độ ma trận trên lưới (-1 nghĩa là đang nằm ở dưới ô Dock)
    public int CurrentX { get; set; } = -1;
    public int CurrentZ { get; set; } = -1;

    private const int MAX_SLOTS = 6; // Một chiếc bánh pizza hoàn chỉnh gồm 6 lát

    /// <summary>
    /// Thiết lập đĩa với danh sách các lát bánh cụ thể và vị trí ma trận tương ứng
    /// </summary>
    public void SetupPlate(List<ToppingType> newSlices, int x, int z)
    {
        this.CurrentX = x;
        this.CurrentZ = z;
        this.slices = new List<ToppingType>(newSlices);

        UpdateVisuals();
    }

    /// <summary>
    /// Trả về danh sách các lát bánh hiện có trên đĩa để phục vụ thuật toán so khớp gộp màu
    /// </summary>
    public List<ToppingType> GetSlices() => slices;

    /// <summary>
    /// Xóa toàn bộ mô hình lát bánh cũ và vẽ lại các lát bánh mới quay tròn quanh tâm đĩa
    /// </summary>
    public void UpdateVisuals()
    {
        // 1. Dọn dẹp sạch sẽ các mô hình lát bánh cũ dựa theo tên định danh "Slice_" để tránh lỗi mọc chồng chất
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Slice_"))
            {
                Destroy(child.gameObject);
            }
        }

        if (slices == null || slices.Count == 0) return;

        // 2. Duyệt qua danh sách dữ liệu để xếp các lát bánh lên đĩa thực thể
        for (int i = 0; i < slices.Count; i++)
        {
            ToppingType sliceColor = slices[i];
            if (sliceColor == ToppingType.None) continue;

            string modelPath = "PizzaModels/Model_" + sliceColor.ToString();
            GameObject modelPrefab = Resources.Load<GameObject>(modelPath);

            // Thuật toán chia góc quay tròn đều quanh đĩa (Lát 0: 0 độ, Lát 1: 60 độ, Lát 2: 120 độ...)
            float angleY = i * (360f / MAX_SLOTS);
            GameObject sliceObj;

            // 3. Nếu tìm thấy Model 3D chuẩn trong thư mục Resources
            if (modelPrefab != null)
            {
                sliceObj = Instantiate(modelPrefab, transform);
                sliceObj.name = $"Slice_{i}_{sliceColor}";

                // Đặt lát bánh nằm TRÊN mặt đĩa Cylinder (Y = 0.2f để không bị chìm xuống đáy đĩa)
                sliceObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                sliceObj.transform.localEulerAngles = new Vector3(0f, angleY, 0f);
            }
            // 4. BIỆN PHÁP PHÒNG THỦ: Tự sinh khối hình học dẹt nhuộm màu nếu hệ thống Load Resources gặp lỗi đường dẫn
            else
            {
                sliceObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sliceObj.name = $"Slice_{i}_Fallback";
                sliceObj.transform.SetParent(transform);

                // Đẩy các khối trụ tạm dạt ra rìa đĩa để xếp thành vòng tròn trực quan
                float radius = 0.5f;
                float rad = angleY * Mathf.Deg2Rad;
                sliceObj.transform.localPosition = new Vector3(Mathf.Cos(rad) * radius, 0.25f, Mathf.Sin(rad) * radius);
                sliceObj.transform.localScale = new Vector3(0.3f, 0.01f, 0.3f);

                Renderer renderer = sliceObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material tempMat = new Material(Shader.Find("Standard"));
                    tempMat.color = GetColorFromEnum(sliceColor);
                    renderer.material = tempMat;
                }
            }
        }
    }

    /// <summary>
    /// Hàm phụ trợ chuyển đổi từ kiểu Enum sang cấu trúc Color của Unity để phục vụ Fallback hệ thống
    /// </summary>
    private Color GetColorFromEnum(ToppingType type)
    {
        switch (type)
        {
            case ToppingType.Red: return Color.red;
            case ToppingType.Yellow: return Color.yellow;
            case ToppingType.Blue: return Color.blue;
            case ToppingType.Orange: return new Color(1f, 0.5f, 0f);
            case ToppingType.Purple: return new Color(0.5f, 0f, 0.5f);
            case ToppingType.Green: return Color.green;
            default: return Color.gray;
        }
    }
}