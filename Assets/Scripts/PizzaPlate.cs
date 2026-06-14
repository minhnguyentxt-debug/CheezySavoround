using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaPlate : MonoBehaviour
{
    [Header("Pizza Slices Data")]
    [SerializeField] private List<ToppingType> slices = new List<ToppingType>();
        
    public int CurrentX { get; set; } = -1;
    public int CurrentZ { get; set; } = -1;

    private const int MAX_SLOTS = 6;
    /// <summary>
    /// Hàm dọn sạch toàn bộ lát bánh trên đĩa (Cả logic lẫn hiển thị)
    /// </summary>
    public void ClearAllSlices()
    {
        slices.Clear();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"Đĩa {gameObject.name} đã được dọn sạch hoàn toàn!");
    }
    public void SetupPlate(List<ToppingType> newSlices, int x, int z)
    {
        this.slices = newSlices;
        this.CurrentX = x;
        this.CurrentZ = z;
        UpdateVisuals();
    }

    public void GenerateRandomSlices()
    {
        slices.Clear();
        int randomCount = UnityEngine.Random.Range(2, 6);

        int uniqueToppingCount = UnityEngine.Random.Range(1, 4);
        List<ToppingType> selectedToppings = new List<ToppingType>();

        System.Array toppingValues = System.Enum.GetValues(typeof(ToppingType));
        for (int i = 0; i < uniqueToppingCount; i++)
        {
            selectedToppings.Add((ToppingType)toppingValues.GetValue(UnityEngine.Random.Range(0, toppingValues.Length)));
        }

        for (int i = 0; i < randomCount; i++)
        {
            ToppingType randomTopping = selectedToppings[UnityEngine.Random.Range(0, selectedToppings.Count)];
            slices.Add(randomTopping);
        }

        UpdateVisuals();
    }

    public List<ToppingType> GetSlices() => slices;

    /// <summary>
    /// CẬP NHẬT TỐI ƯU CHO POOLING: Dọn dẹp Visual ngay lập tức, không để lại đĩa trống
    /// </summary>
    public void UpdateVisuals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Slice_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        if (slices == null || slices.Count == 0) return;

        for (int i = 0; i < slices.Count; i++)
        {
            ToppingType sliceColor = slices[i];
            if (sliceColor == ToppingType.None) continue;

            string modelPath = "PizzaModels/Model_" + sliceColor.ToString();
            GameObject modelPrefab = Resources.Load<GameObject>(modelPath);

            float angleY = i * (360f / MAX_SLOTS);
            GameObject sliceObj;

            if (modelPrefab != null)
            {
                sliceObj = Instantiate(modelPrefab, transform);
                sliceObj.name = $"Slice_{i}_{sliceColor}";

                sliceObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                sliceObj.transform.localEulerAngles = new Vector3(0f, angleY, 0f);
            }
            else
            {
                sliceObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sliceObj.name = $"Slice_{i}_Fallback";
                sliceObj.transform.SetParent(transform);

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

    public IEnumerator AnimateSliceFly(Transform sliceTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = sliceTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = t * t * (3f - 2f * t);

            float bonusY = Mathf.Sin(t * Mathf.PI) * 1.5f;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, t);
            currentPos.y += bonusY;

            sliceTransform.position = currentPos;
            yield return null;
        }

        sliceTransform.position = targetPosition;
    }

    private void OnEnable()
    {
        GameEventSystem.OnSkinSelected += ApplyNewSkinMaterial;
    }

    private void OnDisable()
    {
        GameEventSystem.OnSkinSelected -= ApplyNewSkinMaterial;
    }

    /// <summary>
    /// Bật/Tắt hiển thị của mesh đĩa đơn (ví dụ khi nằm trong đĩa đôi)
    /// </summary>
    public void SetPlateMeshActive(bool active)
    {
        foreach (Transform child in transform)
        {
            if (child != null && !child.name.StartsWith("Slice_"))
            {
                child.gameObject.SetActive(active);
            }
        }
    }
    /// <summary>
    /// Thêm một lát bánh mới vào danh sách và cập nhật lại hình ảnh trên đĩa.
    /// </summary>
    public void AddSlice(ToppingType topping)
    {
        if (slices.Count < MAX_SLOTS)
        {
            slices.Add(topping);
            UpdateVisuals(); // Gọi hàm này để spawn model lát bánh mới hiển thị lên
        }
    }
    private void ApplyNewSkinMaterial(string skinId)
    {
        // Renderer plateRenderer = GetComponent<Renderer>();
        // plateRenderer.material = ...
    }
    // Giả sử trong PizzaPlate.cs bạn đang lưu danh sách lát bánh bằng biến này:
    // public List<ToppingType> currentSlices; 

    /// <summary>
    /// Hàm xuất danh sách các lát bánh hiện có trên đĩa để phục vụ việc Save game
    /// </summary>
    public List<ToppingType> GetSlicesOnPlate()
    {
        // Bạn hãy đổi 'currentSlices' thành đúng tên biến List lát bánh thực tế trong code của bạn
        return slices;
    }
}