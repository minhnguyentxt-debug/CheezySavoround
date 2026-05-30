using System.Collections.Generic;
using UnityEngine;

public class PizzaPlate : MonoBehaviour
{
    [Header("Pizza Slices Data")]
    // Đĩa bây giờ quản lý một danh sách các lát bánh đặt trên nó
    [SerializeField] private List<ToppingType> slices = new List<ToppingType>();

    public int CurrentX { get; set; }
    public int CurrentZ { get; set; }

    private const int MAX_SLOTS = 6; // Một chiếc bánh pizza hoàn chỉnh gồm 6 lát

    /// <summary>
    /// Thiết lập đĩa với danh sách các lát bánh cụ thể
    /// </summary>
    public void SetupPlate(List<ToppingType> newSlices, int x, int z)
    {
        this.CurrentX = x;
        this.CurrentZ = z;
        this.slices = new List<ToppingType>(newSlices);

        UpdateVisuals();
    }

    public List<ToppingType> GetSlices() => slices;

    /// <summary>
    /// Xóa các lát bánh cũ và vẽ lại các lát bánh mới quay tròn quanh tâm đĩa
    /// </summary>
    public void UpdateVisuals()
    {
        // 1. Xóa toàn bộ các mô hình lát bánh cũ dựa theo tên định danh "Slice_"
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Slice_"))
            {
                Destroy(child.gameObject);
            }
        }

        if (slices == null || slices.Count == 0) return;

        // 2. Duyệt qua danh sách để xếp bánh lên đĩa đơn
        for (int i = 0; i < slices.Count; i++)
        {
            ToppingType sliceColor = slices[i];
            if (sliceColor == ToppingType.None) continue;

            string modelPath = "PizzaModels/Model_" + sliceColor.ToString();
            GameObject modelPrefab = Resources.Load<GameObject>(modelPath);

            // Tính toán góc quay (Ví dụ: lát thứ 0 góc 0, lát thứ 1 góc 60, lát thứ 2 góc 120...)
            float angleY = i * (360f / MAX_SLOTS);
            GameObject sliceObj;

            if (modelPrefab != null)
            {
                // Sinh mô hình 3D chuẩn làm con của Đĩa
                sliceObj = Instantiate(modelPrefab, transform);
                sliceObj.name = $"Slice_{i}_{sliceColor}";

                // Đặt lát bánh nằm ngay ngắn trên mặt đĩa và xoay theo góc tròn fanning out
                sliceObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                sliceObj.transform.localEulerAngles = new Vector3(0f, angleY, 0f);
            }
            else
            {
                // FALLBACK: Sinh khối trụ dẹt nếu thiếu Asset mô hình 3D
                sliceObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sliceObj.name = $"Slice_{i}_Fallback";
                sliceObj.transform.SetParent(transform);

                // Thuật toán lượng giác đẩy khối trụ dạt ra rìa đĩa để tạo vòng tròn
                float radius = 0.5f;
                float rad = angleY * Mathf.Deg2Rad;
                sliceObj.transform.localPosition = new Vector3(Mathf.Cos(rad) * radius, 0.08f, Mathf.Sin(rad) * radius);
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