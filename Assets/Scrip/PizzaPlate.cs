using System.Collections.Generic;
using UnityEngine;

public class PizzaPlate : MonoBehaviour
{
    public const int MAX_SLICES = 6; // Quy chuẩn tối đa 6 miếng

    [Header("Plate Data")]
    [SerializeField] private List<ToppingType> slices = new List<ToppingType>();

    // Tọa độ của đĩa này trên lưới 4x6
    public int CurrentX { get; set; }
    public int CurrentZ { get; set; }

    /// <summary>
    /// Khởi tạo dữ liệu ban đầu cho đĩa bánh từ file JSON
    /// </summary>
    public void SetupPlate(List<ToppingType> initialSlices, int x, int z)
    {
        this.CurrentX = x;
        this.CurrentZ = z;
        this.slices = new List<ToppingType>(initialSlices);

        UpdateVisuals();
    }

    public List<ToppingType> GetSlices() => slices;
    public bool IsFull() => slices.Count >= MAX_SLICES;
    public bool IsEmpty() => slices.Count == 0;

    public ToppingType GetTopTopping()
    {
        if (slices.Count > 0) return slices[slices.Count - 1];
        return ToppingType.None;
    }

    /// <summary>
    /// Hàm cập nhật hiển thị (Sẽ ráp Asset 3D thật và hiệu ứng Bezier ở Tuần 2)
    /// </summary>
    public void UpdateVisuals()
    {
        if (slices.Count > 0)
        {
            Debug.Log($"[Debug] Đĩa tại [{CurrentX},{CurrentZ}] có {slices.Count} lát vị {GetTopTopping()}");
        }
    }
}