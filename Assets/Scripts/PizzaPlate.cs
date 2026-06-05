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
        // 1. DẠ TRÚC SỬA ĐỔI: Dùng DestroyImmediate thay vì Destroy thông thường 
        // để ép các lát bánh cũ biến mất NGAY LẬP TỨC trong cùng 1 frame, tránh lỗi Pooling mang Visual cũ sang đĩa mới.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Slice_"))
            {
                DestroyImmediate(child.gameObject); // Xóa ngay lập tức, không chờ đợi cuối frame
            }
        }

        // Nếu đĩa không có bánh, dừng lại luôn (Đĩa hoàn toàn trống sạch sẽ)
        if (slices == null || slices.Count == 0) return;

        // 2. Duyệt qua danh sách dữ liệu để xếp các lát bánh lên đĩa thực thể
        for (int i = 0; i < slices.Count; i++)
        {
            ToppingType sliceColor = slices[i];
            if (sliceColor == ToppingType.None) continue;

            string modelPath = "PizzaModels/Model_" + sliceColor.ToString();
            GameObject modelPrefab = Resources.Load<GameObject>(modelPath);

            float angleY = i * (360f / MAX_SLOTS);
            GameObject sliceObj;

            // 3. Nếu tìm thấy Model 3D chuẩn trong thư mục Resources
            if (modelPrefab != null)
            {
                sliceObj = Instantiate(modelPrefab, transform);
                sliceObj.name = $"Slice_{i}_{sliceColor}";

                sliceObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                sliceObj.transform.localEulerAngles = new Vector3(0f, angleY, 0f);
            }
            // 4. BIỆN PHÁP PHÒNG THỦ: Tự sinh khối hình học dẹt nhuộm màu
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

    private void ApplyNewSkinMaterial(string skinId)
    {
        // Renderer plateRenderer = GetComponent<Renderer>();
        // plateRenderer.material = ...
    }
}