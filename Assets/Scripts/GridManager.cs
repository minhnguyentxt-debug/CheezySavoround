using System;
using System.Collections;
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

    [SerializeField] private GameObject floatingTextPrefab;

    private GameObject[,] gridMatrix;
    private PizzaPlate[,] gridPlateMatrix;

    private Queue<GameObject> platePool = new Queue<GameObject>();
    [Header("Pooling Config")]
    [SerializeField] private int initialPoolSize = 10;

    [System.Serializable]

    public struct PizzaSlicePrefabData
    {
        public ToppingType toppingType;
        public GameObject slicePrefab;
    }

    [Header("--- CẤU HÌNH LÁT BÁNH BAY ---")]
    [SerializeField] private List<PizzaSlicePrefabData> pizzaSlicePrefabs;

    private GameObject GetSlicePrefab(ToppingType topping)
    {
        foreach (var data in pizzaSlicePrefabs)
        {
            if (data.toppingType == topping) return data.slicePrefab;
        }
        return null;
    }

    void Start()
    {
        InitializePlatePool();
        GenerateGrid();
        // LoadLevelFromJSON(1);
        SpawnRandomTestPlates(5);
    }

    private void InitializePlatePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject plateObj = Instantiate(platePrefab, transform);
            plateObj.SetActive(false);
            platePool.Enqueue(plateObj);
        }
    }

    public GameObject GetPlateFromPool(Vector3 position, Quaternion rotation)
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
            plateObj = Instantiate(platePrefab, position, rotation, transform);
        }

        return plateObj;
    }

    private void ReturnPlateToPool(GameObject plateObj)
    {
        if (plateObj == null) return;

        plateObj.SetActive(false);
        plateObj.transform.SetParent(transform);

        PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
        if (pizzaPlateScript != null)
        {
            // Sử dụng ClearAllSlices nếu bạn đã định nghĩa nó trong PizzaPlate, 
            // hoặc dùng Clear() danh sách logic tùy thuộc cấu trúc của bạn.
            pizzaPlateScript.GetSlices().Clear();
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

    /// <summary>
    /// LOGIC GIỮ NGUYÊN BẢN CŨ: Dùng hàng đợi Loang nhưng có màng lọc khống chế khoảng cách nghiêm ngặt trong phạm vi 4 hướng của đĩa mới đặt
    /// </summary>
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

            // BỘ LỌC 1: Nếu ô này bằng cách nào đó vượt quá phạm vi 4 hướng của đĩa ban đầu -> Bỏ qua không xử lý
            if (Mathf.Abs(x - startX) + Mathf.Abs(z - startZ) > 1) continue;

            CheckAndExecuteCombo(x, z);

            PizzaPlate currentPlate = GetPlateAt(x, z);

            if (currentPlate != null && currentPlate.GetSlices().Count == 0)
            {
                DestroyGridPlate(x, z);
                currentPlate = null;
            }
            if (currentPlate == null) continue;

            if (currentPlate.GetSlices().Count > 0)
            {
                List<ToppingType> uniqueToppings = currentPlate.GetSlices()
                    .Where(t => t != ToppingType.None)
                    .Distinct()
                    .ToList();

                foreach (ToppingType topping in uniqueToppings)
                {
                    currentPlate = GetPlateAt(x, z);
                    if (currentPlate == null || currentPlate.GetSlices().Count == 0) break;

                    Vector2Int targetCell = FindBestTargetForTopping(x, z, topping);

                    // BỘ LỌC 2: Chỉ tương tác nếu đĩa mục tiêu tốt nhất cũng nằm trong phạm vi 4 hướng của đĩa mới đặt
                    if (targetCell.x != -1 && (Mathf.Abs(targetCell.x - startX) + Mathf.Abs(targetCell.y - startZ) <= 1))
                    {
                        PizzaPlate targetPlate = GetPlateAt(targetCell.x, targetCell.y);
                        if (targetPlate == null) continue;

                        List<ToppingType> targetSlices = targetPlate.GetSlices();

                        // 1. ĐẨY CÁC LÁT BÁNH KHÁC LOẠI (OUTCAST) KHỎI ĐĨA TIẾP NHẬN
                        for (int i = targetSlices.Count - 1; i >= 0; i--)
                        {
                            ToppingType outcast = targetSlices[i];
                            if (outcast != topping && outcast != ToppingType.None)
                            {
                                Vector2Int dest = FindEvictDestination(targetCell.x, targetCell.y, outcast);

                                // BỘ LỌC 3: Đĩa chứa bánh thừa bị đẩy đi bắt buộc phải nằm trong phạm vi 4 hướng đĩa mới đặt
                                if (dest.x != -1 && (Mathf.Abs(dest.x - startX) + Mathf.Abs(dest.y - startZ) <= 1))
                                {
                                    PizzaPlate destPlate = GetPlateAt(dest.x, dest.y);
                                    if (destPlate != null)
                                    {
                                        destPlate.GetSlices().Add(outcast);
                                        targetSlices.RemoveAt(i);

                                        StartCoroutine(AnimateSliceTransferCoroutine(targetPlate.transform.position, destPlate.transform.position, outcast, destPlate));

                                        destPlate.UpdateVisuals();
                                        CheckAndExecuteCombo(dest.x, dest.y);

                                        if (!alreadyInQueue.Contains(dest))
                                        {
                                            cellsToCheck.Enqueue(dest);
                                            alreadyInQueue.Add(dest);
                                        }
                                    }
                                }
                            }
                        }
                        targetPlate.UpdateVisuals();

                        // 2. THU HÚT CÁC LÁT BÁNH CÙNG LOẠI TỪ HÀNG XÓM VÀO ĐĨA TIẾP NHẬN
                        bool targetPlateChanged = false;

                        foreach (Vector2Int dir in directions)
                        {
                            int neighborX = targetCell.x + dir.x;
                            int neighborZ = targetCell.y + dir.y;
                            Vector2Int neighborCell = new Vector2Int(neighborX, neighborZ);

                            // BỘ LỌC 4: CHẶN ĐĨA THỨ 3 - Không cho phép hút bánh từ những ô nằm ngoài 4 hướng của đĩa mới đặt
                            if (Mathf.Abs(neighborX - startX) + Mathf.Abs(neighborZ - startZ) > 1) continue;

                            PizzaPlate neighborPlate = GetPlateAt(neighborX, neighborZ);
                            if (neighborPlate == null || neighborPlate == targetPlate) continue;

                            List<ToppingType> neighborSlices = neighborPlate.GetSlices();
                            bool isPulled = false;

                            for (int j = neighborSlices.Count - 1; j >= 0; j--)
                            {
                                int targetCount = targetPlate.GetSlices().Count;
                                bool targetIsPure = targetPlate.GetSlices().All(t => t == topping);
                                int maxAllowed = targetIsPure ? 6 : 5;

                                if (targetCount >= maxAllowed) break;

                                if (neighborSlices[j] == topping)
                                {
                                    targetPlate.GetSlices().Add(topping);
                                    neighborSlices.RemoveAt(j);
                                    isPulled = true;
                                    targetPlateChanged = true;

                                    StartCoroutine(AnimateSliceTransferCoroutine(neighborPlate.transform.position, targetPlate.transform.position, topping, targetPlate));

                                    if (!alreadyInQueue.Contains(neighborCell))
                                    {
                                        cellsToCheck.Enqueue(neighborCell);
                                        alreadyInQueue.Add(neighborCell);
                                    }
                                }
                            }

                            if (isPulled)
                            {
                                neighborPlate.UpdateVisuals();
                                if (neighborPlate.GetSlices().Count == 0)
                                {
                                    DestroyGridPlate(neighborX, neighborZ);
                                }
                            }
                        }

                        if (targetPlateChanged)
                        {
                            targetPlate.UpdateVisuals();
                            CheckAndExecuteCombo(targetCell.x, targetCell.y);
                            if (!alreadyInQueue.Contains(targetCell))
                            {
                                cellsToCheck.Enqueue(targetCell);
                                alreadyInQueue.Add(targetCell);
                            }
                        }
                    }
                }
            }
            currentPlate = GetPlateAt(x, z);
            if (currentPlate != null && currentPlate.GetSlices().Count == 0)
            {
                DestroyGridPlate(x, z);
            }
        }
    }

    /// <summary>
    /// SỬA LỖI 2: Chỉ cho phép đĩa hỗn hợp chứa tối đa 5 lát khi nhận bánh thừa để tránh deadlock đầy đĩa hỗn hợp
    /// </summary>
    /// <summary>
    /// Tìm kiếm trong 4 ô hàng xóm xem đĩa nào tối ưu nhất để gom loại Topping này về.
    /// </summary>
    private Vector2Int FindBestTargetForTopping(int startX, int startZ, ToppingType topping)
    {
        // 4 hướng: Trên, Dưới, Trái, Phải
        int[] dx = { 0, 0, -1, 1 };
        int[] dz = { 1, -1, 0, 0 };

        Vector2Int bestTarget = new Vector2Int(-1, -1);
        int maxMatchingSlices = -1;

        for (int i = 0; i < 4; i++)
        {
            int nx = startX + dx[i];
            int nz = startZ + dz[i];

            // Lấy đĩa ở ô hàng xóm
            PizzaPlate neighborPlate = GetPlateAt(nx, nz);
            if (neighborPlate != null)
            {
                // Đếm số lát bánh cùng loại (vị) đang có trên đĩa hàng xóm này
                int matchCount = neighborPlate.GetSlices().Count(t => t == topping);
                int totalSlices = neighborPlate.GetSlices().Count;

                // Nếu đĩa hàng xóm có chứa vị này và đĩa đó chưa bị đầy (dưới 6 lát)
                if (matchCount > 0 && totalSlices < 6)
                {
                    // Ưu tiên chọn đĩa nào đang có nhiều lát cùng vị nhất để gom về một mối
                    if (matchCount > maxMatchingSlices)
                    {
                        maxMatchingSlices = matchCount;
                        bestTarget = new Vector2Int(nx, nz);
                    }
                }
            }
        }

        // Nếu không tìm thấy đĩa hàng xóm nào có sẵn vị này, trả về (-1, -1) để bỏ qua
        return bestTarget;
    }
    /// <summary>
    /// Tìm kiếm ô hàng xóm phù hợp để chuyển (evict) các lát bánh không đồng nhất sang đó.
    /// </summary>
    private Vector2Int FindEvictDestination(int startX, int startZ, ToppingType topping)
    {
        // 4 hướng di chuyển cơ bản: Trên, Dưới, Trái, Phải
        int[] dx = { 0, 0, -1, 1 };
        int[] dz = { 1, -1, 0, 0 };

        Vector2Int bestEvictTarget = new Vector2Int(-1, -1);
        int minSlicesCount = 7; // Khởi tạo lớn hơn số lượng lát tối đa trên một đĩa (6 lát)

        for (int i = 0; i < 4; i++)
        {
            int nx = startX + dx[i];
            int nz = startZ + dz[i];

            // Lấy đĩa ở ô hàng xóm
            PizzaPlate neighborPlate = GetPlateAt(nx, nz);

            if (neighborPlate != null)
            {
                int totalSlices = neighborPlate.GetSlices().Count;

                // Điều kiện 1: Đĩa hàng xóm phải chưa bị đầy (dưới 6 lát)
                if (totalSlices < 6)
                {
                    // Ưu tiên 1a: Đĩa hàng xóm đã có sẵn loại Topping này rồi -> Gom vào rất hợp lý
                    if (neighborPlate.GetSlices().Contains(topping))
                    {
                        return new Vector2Int(nx, nz); // Trả về ngay lập tức vì đây là ô tối ưu nhất
                    }

                    // Ưu tiên 1b: Nếu không có đĩa nào trùng vị, tìm đĩa nào đang trống hoặc ít bánh nhất để gửi tạm
                    if (totalSlices < minSlicesCount)
                    {
                        minSlicesCount = totalSlices;
                        bestEvictTarget = new Vector2Int(nx, nz);
                    }
                }
            }
        }

        // Trả về ô hàng xóm ít bánh nhất tìm được, hoặc (-1, -1) nếu tất cả xung quanh đều kín chỗ
        return bestEvictTarget;
    }

    private void DestroyGridPlate(int x, int z)
    {
        if (gridPlateMatrix[x, z] != null)
        {
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
            Debug.Log($"<color=cyan>[Combo Kích Hoạt] Đĩa tại [{x},{z}] đã đủ 6 lát vị {firstType}!</color>");
            GameEventSystem.OnPizzaCompleted?.Invoke(firstType);

            GameObject comboPlateObj = gridPlateMatrix[x, z].gameObject;
            gridPlateMatrix[x, z] = null; 
            Vector3 spawnPos = comboPlateObj.transform.position + new Vector3(0, 2f, -1f); 
            ShowFloatingScore("+100", spawnPos);
            StartCoroutine(VisualShrinkAndPoolCoroutine(comboPlateObj, 0.22f));
        }
    }
    private void ShowFloatingScore(string text, Vector3 position)
    {
        if (floatingTextPrefab == null) return;

        // Instantiate tại vị trí (x, z) của đĩa bánh
        GameObject go = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        go.GetComponent<FloatingText>().Setup(text, position);
    }
    private IEnumerator VisualBounceCoroutine(Transform target, float intensity, float duration)
    {
        if (target == null) yield break;

        Vector3 originalScale = Vector3.one;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float scaleOffset = Mathf.Sin(progress * Mathf.PI) * intensity;
            target.localScale = originalScale + new Vector3(scaleOffset, scaleOffset, scaleOffset);

            yield return null;
        }

        if (target != null) target.localScale = originalScale;
    }

    private IEnumerator VisualShrinkAndPoolCoroutine(GameObject plateObj, float delay = 0f)
    {
        if (plateObj == null) yield break;
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Transform target = plateObj.transform;
        Vector3 startScale = target.localScale;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            target.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }

        // Đảm bảo Reset lại Scale về 1 trước khi trả về Pool để lần sau lấy ra không bị biến dạng kích thước
        target.localScale = Vector3.one;
        ReturnPlateToPool(plateObj);
    }

    private IEnumerator AnimateSliceTransferCoroutine(Vector3 startPos, Vector3 endPos, ToppingType topping, PizzaPlate targetPlate)
    {
        GameObject dummySlice = new GameObject($"Flying_{topping}");

        GameObject prefab = GetSlicePrefab(topping);
        GameObject visualSlice = null;

        if (prefab != null)
        {
            visualSlice = Instantiate(prefab, dummySlice.transform);
            visualSlice.transform.localPosition = Vector3.zero;
            visualSlice.transform.localRotation = Quaternion.identity;
            visualSlice.transform.localScale = prefab.transform.localScale;
        }
        else
        {
            Debug.LogWarning($"[Cảnh báo] Chưa gán Prefab cho loại bánh: {topping} trong Inspector!");
            visualSlice = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualSlice.transform.SetParent(dummySlice.transform);
            visualSlice.transform.localPosition = Vector3.zero;
            visualSlice.transform.localScale = new Vector3(1f, 0.2f, 1f);
        }

        float duration = 0.22f;
        float elapsed = 0f;
        float peakHeight = 1.8f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, smoothProgress);
            currentPos.y += Mathf.Sin(smoothProgress * Mathf.PI) * peakHeight;

            if (dummySlice != null)
            {
                dummySlice.transform.position = currentPos;
            }
            yield return null;
        }

        Destroy(dummySlice);

        if (targetPlate != null && targetPlate.gameObject.activeInHierarchy)
        {
            targetPlate.UpdateVisuals();
            StartCoroutine(VisualBounceCoroutine(targetPlate.transform, 0.12f, 0.12f));
        }
    }
}