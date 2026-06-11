using System.Collections.Generic;
using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [SerializeField] private GameObject platePrefab;       // Prefab của chiếc đĩa bánh (Plate_Base_Prefab)
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
        }
        return null;
    }

    /// <summary>
    /// Tìm vị trí ô Dock tương ứng với đĩa bánh
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

        // Chỉ khi đi qua được điều kiện trên (tức là đã trống sạch 100%), đoạn code dưới mới chạy để hồi cả 3 ô cùng lúc
        for (int i = 0; i < dockSlots.Length; i++)
        {
            if (spawnPoints == null || i >= spawnPoints.Length || spawnPoints[i] == null || platePrefab == null)
            {
                Debug.LogWarning($"[DockManager] Thiếu cấu hình SpawnPoint hoặc PlatePrefab ở ô Slot index {i}!");
                continue;
            }

            // Khởi tạo Object đĩa bánh đơn
            GameObject plateObj = Instantiate(platePrefab, spawnPoints[i].position, Quaternion.identity, transform);
            plateObj.name = $"Dock_Plate_Slot_[{i}]";

            PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
            if (pizzaPlateScript != null)
            {
                List<ToppingType> mixedToppings = GetMixedToppingsForDock();
                pizzaPlateScript.SetupPlate(mixedToppings, -1, -1);

                // Lưu đĩa vào mảng quản lý Dock
                dockSlots[i] = plateObj;
            }
        }
        Debug.Log("<color=cyan>[DockManager] Đã dùng hết sạch bánh cũ! Đang hồi đồng loạt một lượt 3 đĩa mới!</color>");
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