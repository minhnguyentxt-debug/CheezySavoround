using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 6;
    [SerializeField] private float cellSize = 3.5f;
    [SerializeField] private float spacing = 0f;

    [Header("Visual Checkerboard Colors")]
    [SerializeField] private Color lightSlotColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color darkSlotColor = new Color(0.65f, 0.65f, 0.65f);

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject platePrefab;

    private GameObject[,] gridMatrix;
    private PizzaPlate[,] gridPlateMatrix;

    private Queue<GameObject> platePool = new Queue<GameObject>();
    [Header("Pooling Config")]
    [SerializeField] private int initialPoolSize = 10; // Khởi tạo sẵn 10 đĩa ẩn trong bộ nhớ

    void Start()
    {
        InitializePlatePool(); // Khởi tạo Pool trước
        GenerateGrid();
        // LoadLevelFromJSON(1);
        SpawnRandomTestPlates(5);
    }

    /// <summary>
    /// Tạo sẵn một lượng đĩa bánh và ẩn chúng đi để nạp vào Pool
    /// </summary>
    private void InitializePlatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject plateObj = Instantiate(platePrefab, transform);
            plateObj.SetActive(false);
            platePool.Enqueue(plateObj);
        }
    }

    /// <summary>
    /// Hàm lấy đĩa bánh ra từ Pool (Thay thế cho Instantiate)
    /// </summary>
    private GameObject GetPlateFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject plateObj;

        if (platePool.Count > 0)
        {
            plateObj = platePool.Dequeue();
            plateObj.transform.position = position;
            plateObj.transform.rotation = rotation;
            plateObj.SetActive(true);
        }
        else
        {
            // Nếu người chơi gộp quá nhiều đĩa vượt mức tính toán ban đầu -> Tự sinh thêm để bù vào Pool
            plateObj = Instantiate(platePrefab, position, rotation, transform);
        }

        return plateObj;
    }

    /// <summary>
    /// Hàm thu hồi đĩa bánh về Pool (Thay thế cho Destroy)
    /// </summary>
    private void ReturnPlateToPool(GameObject plateObj)
    {
        if (plateObj == null) return;

        plateObj.SetActive(false);
        plateObj.transform.SetParent(transform); // Đưa về làm con của GridManager cho gọn Hierarchy

        // Reset dữ liệu đĩa bánh trước khi cất đi (Tránh việc đĩa cũ mang vị sang đĩa mới)
        PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
        if (pizzaPlateScript != null)
        {
            pizzaPlateScript.GetSlices().Clear(); // Xóa sạch các lát bánh cũ trên đĩa
        }

        platePool.Enqueue(plateObj);
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

                GameObject plateObj = GetPlateFromPool(slotPos, Quaternion.identity);

                PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
                if (pizzaPlateScript != null)
                {
                    pizzaPlateScript.GetSlices().Clear();
                    int randomSliceCount = UnityEngine.Random.Range(2, 6);
                    int enumLength = System.Enum.GetValues(typeof(ToppingType)).Length;
                    ToppingType randomTopping = (ToppingType)UnityEngine.Random.Range(1, enumLength);
                    for (int i = 0; i < randomSliceCount; i++)
                    {
                        pizzaPlateScript.GetSlices().Add(randomTopping);
                    }
                    pizzaPlateScript.UpdateVisuals();

                    pizzaPlateScript.CurrentX = rx;
                    pizzaPlateScript.CurrentZ = rz;
                    gridPlateMatrix[rx, rz] = pizzaPlateScript;
                    spawned++;
                }
            }
        }
    }

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

            // ĐÃ SỬA: Lấy từ Pool thay vì Instantiate
            GameObject plateObj = GetPlateFromPool(slotCenterPos, Quaternion.identity);
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

    // Cấu trúc phụ trợ để lưu thông tin hàng xóm phục vụ thuật toán sắp xếp ưu tiên
    private struct NeighborInfo
    {
        public PizzaPlate plate;
        public int x;
        public int z;
    }
    public void CheckAndMergePizza(int startX, int startZ)
    {
        Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();
        HashSet<Vector2Int> alreadyInQueue = new HashSet<Vector2Int>();

        Vector2Int startCell = new Vector2Int(startX, startZ);
        cellsToCheck.Enqueue(startCell);
        alreadyInQueue.Add(startCell);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Trên
            new Vector2Int(0, -1), // Dưới
            new Vector2Int(-1, 0), // Trái
            new Vector2Int(1, 0)   // Phải
        };

        int safetyLoopCounter = 0;
        int maxSafetyLoops = 200;

        while (cellsToCheck.Count > 0)
        {
            safetyLoopCounter++;
            if (safetyLoopCounter > maxSafetyLoops)
            {
                Debug.LogWarning("<color=yellow>[GridManager] Cảnh báo: Đã ngắt vòng lặp loang để bảo vệ FPS.</color>");
                break;
            }

            Vector2Int currentCell = cellsToCheck.Dequeue();
            alreadyInQueue.Remove(currentCell);

            int x = currentCell.x;
            int z = currentCell.y;

            PizzaPlate currentPlate = GetPlateAt(x, z);

            if (currentPlate != null && currentPlate.GetSlices().Count == 0)
            {
                DestroyGridPlate(x, z);
                currentPlate = null;
            }
            if (currentPlate == null) continue;

            // --- PHASE 1: ĐĨA HIỆN TẠI "ĐẨY" BÁNH (PHÂN TÁCH THEO VỊ) ---
            if (currentPlate.GetSlices().Count > 0)
            {
                // Lấy danh sách các vị DUY NHẤT đang có mặt trên đĩa hiện tại
                List<ToppingType> uniqueToppings = currentPlate.GetSlices()
                    .Where(t => t != ToppingType.None)
                    .Distinct()
                    .ToList();

                // Duyệt qua TỪNG VỊ MỘT để xử lý đẩy đi riêng biệt
                foreach (ToppingType topping in uniqueToppings)
                {
                    if (currentPlate == null || currentPlate.GetSlices().Count == 0) break;

                    List<NeighborInfo> validPushNeighbors = new List<NeighborInfo>();

                    // Tìm những hàng xóm có chứa CHÍNH VỊ NÀY
                    foreach (Vector2Int dir in directions)
                    {
                        int neighborX = x + dir.x;
                        int neighborZ = z + dir.y;

                        PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
                        if (neighborPlate == null || neighborPlate.GetSlices().Count == 0 || neighborPlate.GetSlices().Count >= 6) continue;

                        if (neighborPlate.GetSlices().Contains(topping))
                        {
                            validPushNeighbors.Add(new NeighborInfo { plate = neighborPlate, x = neighborX, z = neighborZ });
                        }
                    }

                    // ƯU TIÊN THÔNG MINH: Đĩa nào có nhiều lát của CHÍNH VỊ NÀY hơn thì xếp lên đầu để nhanh nổ Combo
                    validPushNeighbors.Sort((a, b) =>
                        b.plate.GetSlices().Count(t => t == topping).CompareTo(a.plate.GetSlices().Count(t => t == topping))
                    );

                    // Tiến hành đẩy DUY NHẤT vị đang xét sang các hàng xóm tương ứng
                    foreach (var targetNeighbor in validPushNeighbors)
                    {
                        // Nếu đĩa hiện tại đã bị đẩy hết sạch vị này rồi thì đổi vị khác
                        if (!currentPlate.GetSlices().Contains(topping)) break;

                        int neighborX = targetNeighbor.x;
                        int neighborZ = targetNeighbor.z;
                        PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
                        if (neighborPlate == null || neighborPlate.GetSlices().Count >= 6) continue;

                        List<ToppingType> currentSlices = currentPlate.GetSlices();
                        List<ToppingType> neighborSlices = neighborPlate.GetSlices();
                        bool isPushed = false;

                        // Chỉ lọc và đẩy đúng lát bánh có vị `topping` hiện tại
                        for (int i = currentSlices.Count - 1; i >= 0; i--)
                        {
                            if (neighborSlices.Count >= 6) break;

                            if (currentSlices[i] == topping)
                            {
                                neighborSlices.Add(topping);
                                currentSlices.RemoveAt(i);
                                isPushed = true;
                            }
                        }

                        if (isPushed)
                        {
                            currentPlate.UpdateVisuals();
                            neighborPlate.UpdateVisuals();
                            CheckAndExecuteCombo(neighborX, neighborZ);

                            neighborPlate = GetPlateAt(neighborX, neighborZ);
                            if (neighborPlate != null && neighborPlate.GetSlices().Count == 0)
                            {
                                DestroyGridPlate(neighborX, neighborZ);
                            }
                            else if (neighborPlate != null)
                            {
                                Vector2Int neighborCell = new Vector2Int(neighborX, neighborZ);
                                if (!alreadyInQueue.Contains(neighborCell))
                                {
                                    cellsToCheck.Enqueue(neighborCell);
                                    alreadyInQueue.Add(neighborCell);
                                }
                            }
                        }
                    }
                }
            }

            if (currentPlate == null || currentPlate.GetSlices().Count == 0)
            {
                DestroyGridPlate(x, z);
                continue;
            }

            // --- PHASE 2: ĐĨA HIỆN TẠI "HÚT" BÁNH (PHÂN TÁCH THEO VỊ) ---
            if (currentPlate != null && currentPlate.GetSlices().Count > 0 && currentPlate.GetSlices().Count < 6)
            {
                List<ToppingType> uniqueToppings = currentPlate.GetSlices()
                    .Where(t => t != ToppingType.None)
                    .Distinct()
                    .ToList();

                // Duyệt qua từng vị trên đĩa tâm để đi hút từ hàng xóm về một cách có chọn lọc
                foreach (ToppingType topping in uniqueToppings)
                {
                    if (currentPlate == null || currentPlate.GetSlices().Count >= 6) break;

                    List<NeighborInfo> validPullNeighbors = new List<NeighborInfo>();

                    foreach (Vector2Int dir in directions)
                    {
                        int neighborX = x + dir.x;
                        int neighborZ = z + dir.y;

                        PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
                        if (neighborPlate == null || neighborPlate.GetSlices().Count == 0) continue;

                        if (neighborPlate.GetSlices().Contains(topping))
                        {
                            validPullNeighbors.Add(new NeighborInfo { plate = neighborPlate, x = neighborX, z = neighborZ });
                        }
                    }

                    // Ưu tiên hút từ đĩa có chứa nhiều lát của CHÍNH VỊ NÀY nhất trước
                    validPullNeighbors.Sort((a, b) =>
                        b.plate.GetSlices().Count(t => t == topping).CompareTo(a.plate.GetSlices().Count(t => t == topping))
                    );

                    foreach (var targetNeighbor in validPullNeighbors)
                    {
                        if (currentPlate == null || currentPlate.GetSlices().Count >= 6) break;

                        int neighborX = targetNeighbor.x;
                        int neighborZ = targetNeighbor.z;
                        PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
                        if (neighborPlate == null || neighborPlate.GetSlices().Count == 0) continue;

                        List<ToppingType> currentSlices = currentPlate.GetSlices();
                        List<ToppingType> neighborSlices = neighborPlate.GetSlices();
                        bool isPulled = false;

                        // Chỉ hút lát bánh có đúng vị `topping` đang xét
                        for (int i = neighborSlices.Count - 1; i >= 0; i--)
                        {
                            if (currentSlices.Count >= 6) break;

                            if (neighborSlices[i] == topping)
                            {
                                currentSlices.Add(topping);
                                neighborSlices.RemoveAt(i);
                                isPulled = true;
                            }
                        }

                        if (isPulled)
                        {
                            currentPlate.UpdateVisuals();
                            neighborPlate.UpdateVisuals();

                            if (neighborSlices.Count == 0)
                            {
                                DestroyGridPlate(neighborX, neighborZ);
                            }
                            else
                            {
                                Vector2Int neighborCell = new Vector2Int(neighborX, neighborZ);
                                if (!alreadyInQueue.Contains(neighborCell))
                                {
                                    cellsToCheck.Enqueue(neighborCell);
                                    alreadyInQueue.Add(neighborCell);
                                }
                            }

                            Vector2Int selfCell = new Vector2Int(x, z);
                            if (!alreadyInQueue.Contains(selfCell))
                            {
                                cellsToCheck.Enqueue(selfCell);
                                alreadyInQueue.Add(selfCell);
                            }
                        }
                    }
                }
            }

            // Kiểm tra combo cuối cùng cho đĩa tâm
            if (currentPlate != null)
            {
                CheckAndExecuteCombo(x, z);

                currentPlate = GetPlateAt(x, z);
                if (currentPlate != null && currentPlate.GetSlices().Count == 0)
                {
                    DestroyGridPlate(x, z);
                }
            }
        }
    }

    /// <summary>
    /// ĐVÀO POOL: Đã chuyển đổi hoàn toàn từ Destroy sang ReturnToPool ngầm
    /// </summary>
    private void DestroyGridPlate(int x, int z)
    {
        if (gridPlateMatrix[x, z] != null)
        {
            // ĐÃ SỬA: Thu hồi đĩa về Pool thay vì phá hủy nó
            ReturnPlateToPool(gridPlateMatrix[x, z].gameObject);
            gridPlateMatrix[x, z] = null;
        }
    }

    private void CheckAndExecuteCombo(int x, int z)
    {
        PizzaPlate plate = GetPlateAt(x, z);
        if (plate == null || plate.GetSlices().Count < 6) return;

        List<ToppingType> slices = plate.GetSlices();
        ToppingType firstType = slices[0];
        bool isPerfectCombo = true;

        foreach (var topping in slices)
        {
            if (topping != firstType)
            {
                isPerfectCombo = false;
                break;
            }
        }

        if (isPerfectCombo)
        {
            Debug.Log($"<color=cyan>[Combo] Đĩa bánh tại [{x},{z}] hoàn thành vị {firstType}!</color>");

            // ĐÃ SỬA: Thu hồi về Pool khi nổ Combo bánh
            ReturnPlateToPool(gridPlateMatrix[x, z].gameObject);
            gridPlateMatrix[x, z] = null;
        }
    }
}