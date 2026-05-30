using System.Collections.Generic;
using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [SerializeField] private GameObject platePrefab;       // Prefab của chiếc đĩa bánh (Plate_Base_Prefab)
    [SerializeField] private Transform[] spawnPoints;      // Mảng chứa 3 vị trí Transform (vị trí 3 ô Dock trên màn hình)

    [Header("Current Dock State")]
    // Mảng lưu trữ thực thể 3 đĩa bánh đang nằm tại 3 ô Dock tương ứng
    [SerializeField] private PizzaPlate[] dockSlots = new PizzaPlate[3];

    void Start()
    {
        // Vừa vào game, tự động lấp đầy cả 3 ô Dock để người chơi có bánh chơi ngay
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
    /// Quét qua toàn bộ các ô Dock, ô nào trống sẽ tự động sinh đĩa bánh mới mang ngẫu nhiên từ 1 đến 4 lát
    /// </summary>
    public void SpawnNewPlatesToAllSlots()
    {
        for (int i = 0; i < dockSlots.Length; i++)
        {
            // Chỉ sinh bánh mới nếu ô Dock đó đang hoàn toàn trống rỗng
            if (dockSlots[i] == null)
            {
                // Biện pháp phòng thủ nếu quên kéo thả Reference trong Inspector
                if (spawnPoints == null || i >= spawnPoints.Length || spawnPoints[i] == null || platePrefab == null)
                {
                    Debug.LogWarning($"[DockManager] Thiếu cấu hình SpawnPoint hoặc PlatePrefab ở ô Slot index {i}!");
                    continue;
                }

                // 1. Khởi tạo thực thể đĩa bánh từ Prefab tại vị trí ô Dock tương ứng
                GameObject plateObj = Instantiate(platePrefab, spawnPoints[i].position, Quaternion.identity, transform);
                plateObj.name = $"Dock_Plate_Slot_[{i}]";

                PizzaPlate plateScript = plateObj.GetComponent<PizzaPlate>();
                if (plateScript != null)
                {
                    // 2. Tạo danh sách lát bánh ngẫu nhiên từ 1 đến 4 miếng cùng một màu vị
                    List<ToppingType> randomSlices = GenerateRandomPizzaSlices();

                    // 3. Thiết lập dữ liệu ban đầu cho đĩa (-1, -1 định danh đĩa đang ở dưới Dock, chưa lên lưới)
                    plateScript.SetupPlate(randomSlices, -1, -1);

                    // 4. Lưu đĩa vào mảng quản lý của ô Dock hiện tại
                    dockSlots[i] = plateScript;
                }
            }
        }
    }

    /// <summary>
    /// Thuật toán tạo ra một danh sách ngẫu nhiên từ 1 đến 4 lát bánh cùng màu vị
    /// </summary>
    private List<ToppingType> GenerateRandomPizzaSlices()
    {
        List<ToppingType> randomSlices = new List<ToppingType>();

        // 1. Lấy tất cả các giá trị hiện có trong Enum ToppingType để chọn ngẫu nhiên
        System.Array toppings = System.Enum.GetValues(typeof(ToppingType));

        // Phòng thủ nếu Enum rỗng hoặc chỉ có vị None
        if (toppings.Length <= 1)
        {
            randomSlices.Add(ToppingType.None);
            return randomSlices;
        }

        // 2. Chọn ngẫu nhiên 1 loại vị (Bắt đầu từ index 1 để loại bỏ vị ToppingType.None nằm ở index 0)
        int randomToppingIndex = Random.Range(1, toppings.Length);
        ToppingType selectedTopping = (ToppingType)toppings.GetValue(randomToppingIndex);

        // 3. Lấy ngẫu nhiên số lượng lát bánh từ 1 đến 4 (Vòng Range lấy từ 1 đến 5 vì số cuối không bao gồm)
        int randomSliceCount = Random.Range(1, 5);

        // 4. Nạp số lượng lát bánh trùng loại đó vào danh sách để gửi cho đĩa vẽ hình visual
        for (int i = 0; i < randomSliceCount; i++)
        {
            randomSlices.Add(selectedTopping);
        }

        return randomSlices;
    }
}