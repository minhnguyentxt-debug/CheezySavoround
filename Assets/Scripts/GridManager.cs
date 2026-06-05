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
        // LoadLevelFromJSON(1);
        SpawnRandomTestPlates(5);
    }
    private void SpawnRandomTestPlates(int count)
    {
        int spawned = 0;
        while (spawned < count)
        {
            int rx = UnityEngine.Random.Range(0, columns);
            int rz = UnityEngine.Random.Range(0, rows);

            if (CanPlacePlate(rx, rz))
            {
                Vector3 slotPos = gridMatrix[rx, rz].transform.position;
                GameObject plateObj = Instantiate(platePrefab, slotPos, Quaternion.identity, transform);

                PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
                if (pizzaPlateScript != null)
                {
                    // Gọi hàm sinh từ 1-3 vị ngẫu nhiên (Hãy đảm bảo bạn đã viết hàm này trong PizzaPlate.cs)
                    pizzaPlateScript.GenerateRandomSlices();

                    pizzaPlateScript.CurrentX = rx;
                    pizzaPlateScript.CurrentZ = rz;
                    gridPlateMatrix[rx, rz] = pizzaPlateScript;
                    spawned++;
                }
            }
        }
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

        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            int neighborX = startX + dir.x;
            int neighborZ = startZ + dir.y;

            PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
            if (neighborPlate == null || neighborPlate.GetSlices().Count == 0) continue;

            List<ToppingType> centerSlices = centerPlate.GetSlices();
            List<ToppingType> neighborSlices = neighborPlate.GetSlices();

            // LOGIC SMART MERGE: Duyệt ngược để xóa lát bánh an toàn
            for (int i = neighborSlices.Count - 1; i >= 0; i--)
            {
                // Nếu đĩa tâm đã đầy 6 lát thì không hút thêm được nữa
                if (centerSlices.Count >= 6) break;

                ToppingType toppingToMove = neighborSlices[i];

                // Nếu đĩa tâm có chứa loại vị này (hoặc nếu đĩa tâm rỗng, có thể tùy chỉnh thêm), 
                // ta hút lát này về đĩa tâm
                if (centerSlices.Contains(toppingToMove))
                {
                    centerSlices.Add(toppingToMove);
                    neighborSlices.RemoveAt(i);
                }
            }

            // Cập nhật lại hình ảnh sau khi hút
            centerPlate.UpdateVisuals();
            neighborPlate.UpdateVisuals();

            // Xóa đĩa hàng xóm nếu nó đã trống rỗng
            if (neighborSlices.Count == 0)
            {
                Destroy(neighborPlate.gameObject);
                gridPlateMatrix[neighborX, neighborZ] = null;
            }
        }

        // KIỂM TRA COMBO: Chỉ nổ khi đĩa tâm đạt 6 lát VÀ đồng nhất 1 loại vị
        if (centerPlate.GetSlices().Count == 6)
        {
            ToppingType firstType = centerPlate.GetSlices()[0];
            bool isPerfect = true;
            foreach (var s in centerPlate.GetSlices()) if (s != firstType) isPerfect = false;

            if (isPerfect)
            {
                GameEventSystem.OnPizzaCompleted?.Invoke(firstType);

                Debug.Log($"<color=yellow>[Combo] Hoàn thành đĩa {firstType}! Bắn sự kiện thành công.</color>");

                Destroy(gridPlateMatrix[startX, startZ].gameObject);
                gridPlateMatrix[startX, startZ] = null;
            }
        }
    }
}