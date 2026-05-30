using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 6;
    [SerializeField] private float cellSize = 1.5f;
    [SerializeField] private float spacing = 0.2f;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject platePrefab; // Kéo Plate_Prefab vào đây

    private GameObject[,] gridMatrix;
    // Ma trận quản lý các đĩa đang nằm trên lưới
    private PizzaPlate[,] plateMatrix;

    void Start()
    {
        GenerateGrid();
        LoadLevelFromJSON(1); // Thử nghiệm load Màn 1
    }

    private void GenerateGrid()
    {
        gridMatrix = new GameObject[columns, rows];
        plateMatrix = new PizzaPlate[columns, rows]; // Khởi tạo ma trận đĩa trống

        float totalWidth = (columns - 1) * (cellSize + spacing);
        float totalHeight = (rows - 1) * (cellSize + spacing);
        Vector3 originOffset = new Vector3(-totalWidth / 2f, 0f, -totalHeight / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 spawnPosition = new Vector3(x * (cellSize + spacing), 0f, z * (cellSize + spacing)) + originOffset + transform.position;
                GameObject newSlot = Instantiate(slotPrefab, spawnPosition, Quaternion.identity, transform);
                newSlot.name = $"Slot_[{x},{z}]";
                gridMatrix[x, z] = newSlot;
            }
        }
        Debug.Log("<color=green>Grid 4x6 Created.</color>");
    }

    /// <summary>
    /// Hàm đọc file JSON từ thư mục Resources và sinh đĩa bánh tương ứng
    /// </summary>
    private void LoadLevelFromJSON(int levelNumber)
    {
        // Đọc file Level_01.json từ thư mục Resources
        string fileName = $"Level_{levelNumber:D2}"; // Kết quả: Level_01
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(fileName);

        if (jsonTextAsset == null)
        {
            Debug.LogError($"[GridManager] Không tìm thấy file cấu hình: Resources/{fileName}.json");
            return;
        }

        // Giải mã chuỗi JSON thành Class dữ liệu C#
        LevelData levelData = JsonUtility.FromJson<LevelData>(jsonTextAsset.text);
        Debug.Log($"<color=cyan>[JSON] Đang tải Màn chơi số: {levelData.levelNumber} - Điểm mục tiêu: {levelData.targetScore}</color>");

        // Duyệt qua danh sách đĩa ban đầu được định nghĩa trong JSON
        foreach (JSONPlateData plateInfo in levelData.initialPlates)
        {
            int targetX = plateInfo.x;
            int targetZ = plateInfo.z;

            // Kiểm tra xem vị trí có nằm ngoài ma trận 4x6 không
            if (targetX < 0 || targetX >= columns || targetZ < 0 || targetZ >= rows)
            {
                Debug.LogWarning($"[JSON Error] Vị trí đĩa [{targetX},{targetZ}] nằm ngoài biên lưới!");
                continue;
            }

            // Lấy vị trí World Position của ô lưới tương ứng để sinh đĩa đè lên trên nó
            Vector3 slotPosition = gridMatrix[targetX, targetZ].transform.position;
            Vector3 plateSpawnPos = slotPosition + new Vector3(0f, 0.1f, 0f); // Nâng nhẹ lên trục Y để không bị chìm vào ô lưới

            // Sinh đĩa bánh trong không gian
            GameObject plateObj = Instantiate(platePrefab, plateSpawnPos, Quaternion.identity, transform);
            plateObj.name = $"Plate_[{targetX},{targetZ}]";

            // Chuyển danh sách chuỗi (String) thành danh sách Enum (ToppingType)
            List<ToppingType> parsedToppings = new List<ToppingType>();
            foreach (string tStr in plateInfo.toppings)
            {
                if (Enum.TryParse(tStr, true, out ToppingType topping))
                {
                    parsedToppings.Add(topping);
                }
                else
                {
                    parsedToppings.Add(ToppingType.None);
                }
            }

            // Gán dữ liệu vào Script PizzaPlate gắn trên đĩa
            PizzaPlate pizzaPlateScript = plateObj.GetComponent<PizzaPlate>();
            if (pizzaPlateScript != null)
            {
                pizzaPlateScript.SetupPlate(parsedToppings, targetX, targetZ);
                plateMatrix[targetX, targetZ] = pizzaPlateScript; // Lưu vào ma trận quản lý đĩa
            }
        }
    }
}