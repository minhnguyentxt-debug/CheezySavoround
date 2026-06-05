using System.Collections.Generic;
using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [SerializeField] private GameObject platePrefab;       // Prefab của chiếc đĩa bánh (Plate_Base_Prefab)
    [SerializeField] private Transform[] spawnPoints;      // Mảng chứa 3 vị trí Transform (vị trí 3 ô Dock trên màn hình)

    [Header("Current Dock State")]
    [SerializeField] private PizzaPlate[] dockSlots = new PizzaPlate[3];

    void Start()
    {
        SpawnNewPlatesToAllSlots();
    }

    /// <summary>
    /// Lấy thông tin đĩa bánh tại một ô Dock cụ thể để kiểm tra lúc nhấc bánh kéo đi
    /// </summary>
    public PizzaPlate GetPlateAtSlot(int index)
    {
        if (index >= 0 && index < dockSlots.Length)
        {
            return dockSlots[index];
        }
        return null;
    }

    /// <summary>
    /// Giải phóng ô Dock khi người chơi đã nhấc bánh và đặt lên lưới thành công
    /// </summary>
    public void EmptyDockSlot(int index)
    {
        if (index >= 0 && index < dockSlots.Length)
        {
            dockSlots[index] = null;
        }
    }

    /// <summary>
    /// Quét qua toàn bộ các ô Dock, ô nào trống sẽ tự động sinh đĩa bánh mới mang từ 1-3 vị hỗn hợp
    /// </summary>
    public void SpawnNewPlatesToAllSlots()
    {
        for (int i = 0; i < dockSlots.Length; i++)
        {
            if (dockSlots[i] == null)
            {
                if (spawnPoints == null || i >= spawnPoints.Length || spawnPoints[i] == null || platePrefab == null)
                {
                    Debug.LogWarning($"[DockManager] Thiếu cấu hình SpawnPoint hoặc PlatePrefab ở ô Slot index {i}!");
                    continue;
                }

                // 1. Khởi tạo Object đĩa bánh
                GameObject plateObj = Instantiate(platePrefab, spawnPoints[i].position, Quaternion.identity, transform);
                plateObj.name = $"Dock_Plate_Slot_[{i}]";

                // 2. Lấy danh sách vị hỗn hợp và setup duy nhất 1 lần
                PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
                if (pizzaPlateScript != null)
                {
                    List<ToppingType> mixedToppings = GetMixedToppingsForDock();
                    pizzaPlateScript.SetupPlate(mixedToppings, -1, -1);

                    // Lưu đĩa vào mảng quản lý Dock luôn
                    dockSlots[i] = pizzaPlateScript;
                }
            }
        }
    }

    /// <summary>
    /// Tạo ra danh sách lát bánh ngẫu nhiên chứa từ 1 đến 3 loại vị khác nhau (Bỏ qua vị None)
    /// </summary>
    private List<ToppingType> GetMixedToppingsForDock()
    {
        List<ToppingType> randomToppings = new List<ToppingType>();
        System.Array allToppings = System.Enum.GetValues(typeof(ToppingType));

        // Phòng trường hợp Enum không có dữ liệu
        if (allToppings.Length <= 1)
        {
            randomToppings.Add(ToppingType.None);
            return randomToppings;
        }

        // 1. Quyết định đĩa này có tổng cộng bao nhiêu lát bánh (ngẫu nhiên từ 2 đến 4 lát)
        int totalSlices = Random.Range(2, 5);

        // 2. Quyết định đĩa này trộn bao nhiêu LOẠI vị khác nhau (từ 1 đến 3 loại vị độc nhất)
        int uniqueToppingCount = Random.Range(1, 4);

        List<ToppingType> chosenPool = new List<ToppingType>();

        // Chọn ra các loại vị độc nhất không trùng nhau (Bắt đầu từ index 1 để né ToppingType.None ở vị trí 0)
        while (chosenPool.Count < uniqueToppingCount && chosenPool.Count < (allToppings.Length - 1))
        {
            ToppingType randomType = (ToppingType)allToppings.GetValue(Random.Range(1, allToppings.Length));
            if (!chosenPool.Contains(randomType))
            {
                chosenPool.Add(randomType);
            }
        }

        // 3. Rải các loại vị đã chọn trong rổ vào số lượng lát bánh của đĩa
        for (int i = 0; i < totalSlices; i++)
        {
            ToppingType finalTopping = chosenPool[Random.Range(0, chosenPool.Count)];
            randomToppings.Add(finalTopping);
        }

        return randomToppings;
    }
}