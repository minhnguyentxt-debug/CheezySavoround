using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 6;
    [SerializeField] private float cellSize = 3.5f;
    [SerializeField] private float spacing = 0f;        // Bằng 0 để các ô lưới khít sát lại với nhau

    [Header("Visual Checkerboard Colors")]
    [SerializeField] private Color lightSlotColor = new Color(0.85f, 0.85f, 0.85f); // Màu ô sáng
    [SerializeField] private Color darkSlotColor = new Color(0.65f, 0.65f, 0.65f);  // Màu ô tối

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject platePrefab;

    private GameObject[,] gridMatrix;
    private PizzaPlate[,] gridPlateMatrix;

    void Start()
    {
        GenerateGrid();
        LoadLevelFromJSON(1);
    }

    /// <summary>
    /// Khởi tạo ma trận lưới vuông khít nhau và tự động nhuộm màu so le bàn cờ
    /// </summary>
    private void GenerateGrid()
    {
        gridMatrix = new GameObject[columns, rows];
        gridPlateMatrix = new PizzaPlate[columns, rows];

        float stepX = cellSize + spacing;
        float stepZ = cellSize + spacing;

        float totalWidth = (columns - 1) * stepX;
        float totalHeight = (rows - 1) * stepZ;
        Vector3 originOffset = new Vector3(-totalWidth / 2f, 0f, -totalHeight / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 spawnPosition = new Vector3(x * stepX, 0f, z * stepZ) + originOffset + transform.position;
                GameObject newSlot = Instantiate(slotPrefab, spawnPosition, Quaternion.identity, transform);
                newSlot.name = $"Slot_[{x},{z}]";

                newSlot.transform.localScale = new Vector3(cellSize, newSlot.transform.localScale.y, cellSize);
                gridMatrix[x, z] = newSlot;

                // Nhuộm màu đậm nhạt so le (Bàn cờ)
                Renderer slotRenderer = newSlot.GetComponentInChildren<Renderer>();
                if (slotRenderer != null)
                {
                    Material tempMaterial;
                    if (slotRenderer.sharedMaterial != null)
                    {
                        tempMaterial = new Material(slotRenderer.sharedMaterial);
                    }
                    else
                    {
                        tempMaterial = new Material(Shader.Find("Standard"));
                    }

                    if ((x + z) % 2 == 0)
                    {
                        tempMaterial.color = lightSlotColor;
                    }
                    else
                    {
                        tempMaterial.color = darkSlotColor;
                    }

                    slotRenderer.material = tempMaterial;
                }
            }
        }
        Debug.Log("<color=green>[GridManager] Đã khởi tạo lưới so le màu bàn cờ khít nhau!</color>");
    }

    private void LoadLevelFromJSON(int levelNumber)
    {
        string fileName = $"Level_{levelNumber:D2}";
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(fileName);

        if (jsonTextAsset == null) return;

        LevelData levelData = JsonUtility.FromJson<LevelData>(jsonTextAsset.text);

        foreach (JSONPlateData plateInfo in levelData.initialPlates)
        {
            int targetX = plateInfo.x;
            int targetZ = plateInfo.z;

            if (targetX < 0 || targetX >= columns || targetZ < 0 || targetZ >= rows) continue;

            Vector3 slotCenterPos = gridMatrix[targetX, targetZ].transform.position;

            GameObject plateObj = Instantiate(platePrefab, slotCenterPos, Quaternion.identity, transform);
            plateObj.name = $"Grid_Plate_[{targetX},{targetZ}]";

            PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
            if (pizzaPlateScript != null)
            {
                List<ToppingType> plateSlices = new List<ToppingType>();
                foreach (string colorStr in plateInfo.toppings)
                {
                    if (Enum.TryParse(colorStr, true, out ToppingType toppingColor))
                    {
                        plateSlices.Add(toppingColor);
                    }
                }

                pizzaPlateScript.SetupPlate(plateSlices, targetX, targetZ);
                gridPlateMatrix[targetX, targetZ] = pizzaPlateScript;
            }
        }
    }

    public bool CanPlacePlate(int x, int z)
    {
        if (x < 0 || x >= columns || z < 0 || z >= rows) return false;
        return gridPlateMatrix[x, z] == null;
    }

    public void AddPlateToGrid(PizzaPlate draggedPlate, int targetX, int targetZ)
    {
        if (!CanPlacePlate(targetX, targetZ) || draggedPlate == null) return;

        draggedPlate.CurrentX = targetX;
        draggedPlate.CurrentZ = targetZ;
        draggedPlate.transform.SetParent(transform);

        draggedPlate.transform.position = gridMatrix[targetX, targetZ].transform.position;

        gridPlateMatrix[targetX, targetZ] = draggedPlate;
    }

    public PizzaPlate GetPlateAt(int x, int z)
    {
        if (x >= 0 && x < columns && z >= 0 && z < rows) return gridPlateMatrix[x, z];
        return null;
    }

    /// <summary>
    /// Kiểm tra 4 hướng hàng xóm xung quanh ô vừa thả để tiến hành gộp các lát bánh cùng màu
    /// </summary>
    public void CheckAndMergePizza(int startX, int startZ)
    {
        PizzaPlate centerPlate = GetPlateAt(startX, startZ);
        if (centerPlate == null || centerPlate.GetSlices().Count == 0) return;

        // Định nghĩa ma trận 4 hướng hàng xóm: Trên, Dưới, Trái, Phải
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Trên
            new Vector2Int(0, -1), // Dưới
            new Vector2Int(-1, 0), // Trái
            new Vector2Int(1, 0)   // Phải
        };

        ToppingType centerColor = centerPlate.GetSlices()[0];

        foreach (Vector2Int dir in directions)
        {
            int neighborX = startX + dir.x;
            int neighborZ = startZ + dir.y;

            PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
            if (neighborPlate == null || neighborPlate.GetSlices().Count == 0) continue;

            ToppingType neighborColor = neighborPlate.GetSlices()[0];

            // Nếu trùng màu vị -> Tiến hành hút bánh!
            if (centerColor == neighborColor)
            {
                List<ToppingType> centerSlices = centerPlate.GetSlices();
                List<ToppingType> neighborSlices = neighborPlate.GetSlices();

                // Chuyển các lát bánh từ hàng xóm sang đĩa tâm cho đến khi đĩa tâm đầy (6 lát) hoặc hàng xóm hết bánh
                while (centerSlices.Count < 6 && neighborSlices.Count > 0)
                {
                    centerSlices.Add(neighborColor);
                    neighborSlices.RemoveAt(neighborSlices.Count - 1);
                }

                centerPlate.UpdateVisuals();
                neighborPlate.UpdateVisuals();

                // Nếu đĩa hàng xóm bị hút sạch bánh -> Xóa đĩa trống
                if (neighborSlices.Count == 0)
                {
                    Debug.Log($"[Merge] Ô [{neighborX},{neighborZ}] đã bị hút hết bánh. Xóa đĩa trống!");
                    Destroy(gridPlateMatrix[neighborX, neighborZ].gameObject);
                    gridPlateMatrix[neighborX, neighborZ] = null;
                }

                // Nếu đĩa tâm gom đủ 6 lát thành bánh hoàn chỉnh -> Ăn điểm và xóa đĩa hoàn chỉnh
                if (centerSlices.Count == 6)
                {
                    Debug.Log("<color=yellow>[Combo] Wow! 1 chiếc bánh Pizza đã hoàn chỉnh! +10 Điểm</color>");
                    Destroy(gridPlateMatrix[startX, startZ].gameObject);
                    gridPlateMatrix[startX, startZ] = null;
                    break;
                }
            }
        }
    }
}