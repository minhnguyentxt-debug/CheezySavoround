using System.Collections.Generic;
using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [SerializeField] private GameObject platePrefab;       // Prefab của chiếc đĩa bánh (Plate_Base_Prefab)
    [SerializeField] private GameObject doublePlatePrefab; // Prefab của đĩa đôi dài (DoublePlate)
    public GameObject DoublePlatePrefab => doublePlatePrefab;
    [SerializeField] private Transform[] spawnPoints;      // Mảng chứa 3 vị trí Transform (vị trí 3 ô Dock trên màn hình)

    [Header("Current Dock State")]
    private GameObject[] dockSlots = new GameObject[3]; // Loại bỏ [SerializeField] để tránh giữ tham chiếu rác từ Inspector

    private void Awake()
    {
        Debug.Log("[DockManager] Awake được gọi!");
        // Luôn khởi tạo mảng trống mới ở đầu game để xóa sạch tham chiếu cũ
        dockSlots = new GameObject[3];
    }

    void Start()
    {
        Debug.Log("[DockManager] Start được gọi!");
        // Đầu game chưa có bánh nào (trống hoàn toàn) -> Sẽ sinh đủ 3 đĩa
        SpawnNewPlatesToAllSlots();
    }

    /// <summary>
    /// Lấy thông tin đĩa bánh tại một ô Dock cụ thể để kiểm tra lúc nhấc bánh kéo đi
    /// </summary>
    public PizzaPlate GetPlateAtSlot(int index)
    {
        if (index >= 0 && index < dockSlots.Length && dockSlots[index] != null)
        {
            PizzaPlate p = dockSlots[index].GetComponent<PizzaPlate>();
            if (p != null) return p;

            DoublePlate dp = dockSlots[index].GetComponent<DoublePlate>();
            if (dp != null) return dp.plate1;
        }
        return null;
    }

    /// <summary>
    /// Tìm vị trí ô Dock tương ứng với đĩa bánh (hỗ trợ cả đĩa đơn và đĩa đôi)
    /// </summary>
    public int FindDockSlotForPlate(PizzaPlate plate)
    {
        if (plate == null) return -1;
        for (int i = 0; i < dockSlots.Length; i++)
        {
            if (dockSlots[i] != null)
            {
                if (dockSlots[i] == plate.gameObject || plate.transform.IsChildOf(dockSlots[i].transform))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// Giải phóng ô Dock khi người chơi đã nhấc bánh và đặt lên lưới thành công
    /// </summary>
    public void EmptyDockSlot(int index)
    {
        if (index >= 0 && index < dockSlots.Length)
        {
            dockSlots[index] = null; // Đánh dấu ô này trống
        }

        // Tự động kiểm tra: Nếu dùng hết sạch sành sanh cả 3 đĩa thì mới tự gọi hồi loạt mới
        if (CheckIfDockIsEmpty())
        {
            SpawnNewPlatesToAllSlots();
        }
    }

    public void EmptyDockSlot(PizzaPlate plate)
    {
        if (plate == null) return;
        for (int i = 0; i < dockSlots.Length; i++)
        {
            if (dockSlots[i] != null)
            {
                if (dockSlots[i] == plate.gameObject || plate.transform.IsChildOf(dockSlots[i].transform))
                {
                    dockSlots[i] = null;
                    break;
                }
            }
        }

        if (CheckIfDockIsEmpty())
        {
            SpawnNewPlatesToAllSlots();
        }
    }

    /// <summary>
    /// Kiểm tra xem toàn bộ 3 ô dưới dock đã trống hoàn toàn (hết sạch đĩa) hay chưa
    /// </summary>
    private bool CheckIfDockIsEmpty()
    {
        if (dockSlots == null || dockSlots.Length == 0)
        {
            return true;
        }
        foreach (var slot in dockSlots)
        {
            if (slot != null)
            {
                return false; // Vẫn còn ít nhất 1 ô có đĩa bánh -> Dock CHƯA hết sạch
            }
        }
        return true; // Cả 3 ô đều bằng null -> Đã hết sạch đĩa!
    }

    /// <summary>
    /// Hàm hồi đĩa bánh mới - ĐÃ ĐƯỢC THÊM KHÓA BẢO VỆ
    /// </summary>
    public void SpawnNewPlatesToAllSlots()
    {
        Debug.Log($"[DockManager] SpawnNewPlatesToAllSlots được gọi! CheckIfDockIsEmpty: {CheckIfDockIsEmpty()}");
        // ====================================================================
        // KHÓA BẢO VỆ QUAN TRỌNG NHẤT:
        // Nếu TRÊN DOCK VẪN CÒN BÁNH (chưa trống hoàn toàn), CẤM KHÔNG CHO SINH LOẠT MỚI!
        // ====================================================================
        if (!CheckIfDockIsEmpty())
        {
            return; // Thoát hàm ngay lập tức, không chạy đoạn code sinh bánh ở dưới
        }

        GridManager gridManager = FindAnyObjectByType<GridManager>();
        float stepDist = 3.5f; // khoảng cách mặc định giữa 2 đĩa
        if (gridManager != null)
        {
            stepDist = gridManager.CellSize + gridManager.Spacing;
        }

        // Chỉ khi đi qua được điều kiện trên (tức là đã trống sạch 100%), đoạn code dưới mới chạy để hồi cả 3 ô cùng lúc
        for (int i = 0; i < dockSlots.Length; i++)
        {
            if (spawnPoints == null || i >= spawnPoints.Length || spawnPoints[i] == null || platePrefab == null)
            {
                Debug.LogWarning($"[DockManager] Thiếu cấu hình SpawnPoint hoặc PlatePrefab ở ô Slot index {i}!");
                continue;
            }

            float rand = Random.value;
            if (rand < 0.7f)
            {
                // 1. Khởi tạo Object đĩa bánh đơn bình thường
                GameObject plateObj = Instantiate(platePrefab, spawnPoints[i].position, Quaternion.identity, transform);
                plateObj.name = $"Dock_Plate_Slot_[{i}]";

                // 2. Lấy danh sách vị hỗn hợp và setup hiển thị theo logic cũ của bạn
                PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
                if (pizzaPlateScript != null)
                {
                    List<ToppingType> mixedToppings = GetMixedToppingsForDock();
                    pizzaPlateScript.SetupPlate(mixedToppings, -1, -1);

                    // Lưu đĩa vào mảng quản lý Dock
                    dockSlots[i] = plateObj;
                }
            }
            else
            {
                // Chọn ngẫu nhiên hướng Dọc hoặc Ngang
                bool isVertical = Random.value > 0.5f;

                if (doublePlatePrefab != null)
                {
                    // 1. Khởi tạo từ Prefab đĩa đôi dài
                    GameObject doublePlateObj = Instantiate(doublePlatePrefab, spawnPoints[i].position, Quaternion.identity, transform);
                    doublePlateObj.name = $"Dock_DoublePlate_Slot_[{i}]";

                    DoublePlate doublePlateScript = doublePlateObj.GetComponent<DoublePlate>();
                    if (doublePlateScript == null)
                    {
                        doublePlateScript = doublePlateObj.AddComponent<DoublePlate>();
                    }

                    // Tự động tìm kiếm đĩa con nếu trong Prefab chưa gán tham chiếu
                    if (doublePlateScript.plate1 == null || doublePlateScript.plate2 == null)
                    {
                        PizzaPlate[] childPlates = doublePlateObj.GetComponentsInChildren<PizzaPlate>();
                        if (childPlates.Length >= 2)
                        {
                            doublePlateScript.plate1 = childPlates[0];
                            doublePlateScript.plate2 = childPlates[1];
                        }
                    }

                    // Setup vị trí local cho 2 đĩa con của prefab dựa trên stepDist (Prefab gốc là dọc theo Z)
                    if (doublePlateScript.plate1 != null && doublePlateScript.plate2 != null)
                    {
                        doublePlateScript.SetupDoublePlate(
                            doublePlateScript.plate1,
                            doublePlateScript.plate2,
                            true, // Model mặc định nằm dọc theo trục Z
                            stepDist
                        );
                    }

                    // Xoay 90 độ nếu muốn đĩa đôi xếp ngang (giả sử mặc định là nằm dọc)
                    doublePlateScript.isVertical = isVertical;
                    if (!isVertical)
                    {
                        doublePlateObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    }
                    else
                    {
                        doublePlateObj.transform.localEulerAngles = Vector3.zero;
                    }

                    // Setup toppings cho 2 đĩa con đảm bảo có ít nhất 2 vị khác nhau
                    SetupToppingsForDoublePlate(doublePlateScript.plate1, doublePlateScript.plate2);

                    dockSlots[i] = doublePlateObj;
                }
                else
                {
                    // Fallback: Tự động tạo đĩa đôi bằng code nếu chưa kéo Prefab vào
                    GameObject doublePlateObj = new GameObject($"Dock_DoublePlate_Slot_[{i}]");
                    doublePlateObj.transform.position = spawnPoints[i].position;
                    doublePlateObj.transform.rotation = Quaternion.identity;
                    doublePlateObj.transform.SetParent(transform);

                    DoublePlate doublePlateScript = doublePlateObj.AddComponent<DoublePlate>();

                    // Tạo đĩa con 1
                    GameObject childPlateObj1 = Instantiate(platePrefab, Vector3.zero, Quaternion.identity);
                    childPlateObj1.name = "PizzaPlate_1";
                    PizzaPlate p1Script = childPlateObj1.GetComponent<PizzaPlate>();

                    // Tạo đĩa con 2
                    GameObject childPlateObj2 = Instantiate(platePrefab, Vector3.zero, Quaternion.identity);
                    childPlateObj2.name = "PizzaPlate_2";
                    PizzaPlate p2Script = childPlateObj2.GetComponent<PizzaPlate>();

                    // Setup vị trí, hướng và tham chiếu cho đĩa đôi
                    doublePlateScript.SetupDoublePlate(p1Script, p2Script, isVertical, stepDist);

                    // Setup toppings cho 2 đĩa con đảm bảo có ít nhất 2 vị khác nhau
                    SetupToppingsForDoublePlate(p1Script, p2Script);

                    dockSlots[i] = doublePlateObj;
                }
            }
        }
        Debug.Log("<color=cyan>[DockManager] Đã dùng hết sạch bánh cũ! Đang hồi đồng loạt một lượt 3 đĩa mới!</color>");
    }

    private void SetupToppingsForDoublePlate(PizzaPlate p1, PizzaPlate p2)
    {
        if (p1 == null || p2 == null) return;

        List<ToppingType> toppings1 = GetMixedToppingsForDock();
        List<ToppingType> toppings2 = GetMixedToppingsForDock();

        int safetyCount = 0;
        while (safetyCount < 100)
        {
            HashSet<ToppingType> uniqueToppings = new HashSet<ToppingType>();
            foreach (var t in toppings1) if (t != ToppingType.None) uniqueToppings.Add(t);
            foreach (var t in toppings2) if (t != ToppingType.None) uniqueToppings.Add(t);

            if (uniqueToppings.Count >= 2)
            {
                break;
            }

            toppings2 = GetMixedToppingsForDock();
            safetyCount++;
        }

        p1.SetupPlate(toppings1, -1, -1);
        p2.SetupPlate(toppings2, -1, -1);
    }

    /// <summary>
    /// Tạo ra danh sách lát bánh ngẫu nhiên chứa từ 1 đến 3 loại vị khác nhau (Giữ nguyên logic gốc của bạn)
    /// </summary>
    private List<ToppingType> GetMixedToppingsForDock()
    {
        List<ToppingType> randomToppings = new List<ToppingType>();
        System.Array allToppings = System.Enum.GetValues(typeof(ToppingType));

        if (allToppings.Length <= 1)
        {
            randomToppings.Add(ToppingType.None);
            return randomToppings;
        }

        int totalSlices = Random.Range(2, 5);
        int uniqueToppingCount = Random.Range(1, 4);

        List<ToppingType> chosenPool = new List<ToppingType>();

        while (chosenPool.Count < uniqueToppingCount && chosenPool.Count < (allToppings.Length - 1))
        {
            ToppingType randomType = (ToppingType)allToppings.GetValue(Random.Range(1, allToppings.Length));
            if (!chosenPool.Contains(randomType))
            {
                chosenPool.Add(randomType);
            }
        }

        for (int i = 0; i < totalSlices; i++)
        {
            ToppingType finalTopping = chosenPool[Random.Range(0, chosenPool.Count)];
            randomToppings.Add(finalTopping);
        }

        return randomToppings;
    }
}