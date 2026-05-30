using System.Collections.Generic;
using UnityEngine;

public class PizzaPlate : MonoBehaviour
{
    public const int MAX_SLICES = 6;
    [SerializeField] private List<ToppingType> slices = new List<ToppingType>();
    public int CurrentX { get; set; }
    public int CurrentZ { get; set; }
    private GameObject currentVisualObject;

    public void SetupPlate(List<ToppingType> initialSlices, int x, int z)
    {
        this.CurrentX = x;
        this.CurrentZ = z;
        this.slices = new List<ToppingType>(initialSlices);

        UpdateVisuals();
    }
    public ToppingType GetTopTopping()
    {
        if (slices.Count > 0) return slices[slices.Count - 1];
        return ToppingType.None;
    }
    public void UpdateVisuals()
    {
        if (currentVisualObject != null)
        {
            Destroy(currentVisualObject);
        }
        ToppingType topTopping = GetTopTopping();
        if (topTopping == ToppingType.None) return;
        string modelPath = $"PizzaModels/Model_{topTopping}";
        GameObject modelPrefab = Resources.Load<GameObject>(modelPath);
        if (modelPrefab != null)
        {
            currentVisualObject = Instantiate(modelPrefab, transform.position, Quaternion.identity, transform);
            currentVisualObject.name = "Visual_Child";
            currentVisualObject.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogError($"[Visual Error] Không tìm thấy Model 3D tại đường dẫn: Resources/{modelPath}");
        }
    }
}