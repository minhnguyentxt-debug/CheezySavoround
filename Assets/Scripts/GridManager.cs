using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 6;
    [SerializeField] private float cellSize = 3.5f;
    [SerializeField] private float spacing = 0f;        // Đặt bằng 0 để các ô lưới khít sát lại với nhau

    [Header("Visual Checkerboard Colors")]
    [SerializeField] private Color lightSlotColor = new Color(0.85f, 0.85f, 0.85f); // Màu ô sáng (Trắng xám nhẹ)
    [SerializeField] private Color darkSlotColor = new Color(0.65f, 0.65f, 0.65f);  // Màu ô tối (Xám đậm)

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

        // Bước nhảy chuẩn (Khi spacing = 0, step sẽ bằng đúng cellSize giúp các ô khít nhau)
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

                // Ép kích thước ô lưới theo cellSize
                newSlot.transform.localScale = new Vector3(cellSize, newSlot.transform.localScale.y, cellSize);
                gridMatrix[x, z] = newSlot;

                // THUẬT TOÁN: Nhuộm màu đậm nhạt so le (Bàn cờ)
                Renderer slotRenderer = newSlot.GetComponentInChildren<Renderer>();
                if (slotRenderer != null)
                {
                    Material tempMaterial;

                    if (slotRenderer.sharedMaterial != null)
                    {
                        // Nếu có material gốc, nhân bản lại từ material đó (Giữ được cả Shader lẫn các thông số cũ)
                        tempMaterial = new Material(slotRenderer.sharedMaterial);
                    }
                    else
                    {
                        // Nếu không có, tạo mới hoàn toàn dựa trên Shader Standard mặc định
                        tempMaterial = new Material(Shader.Find("Standard"));
                    }

                    // Nếu tổng tọa độ x + z là số chẵn -> màu sáng, ngược lại -> màu tối
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
        Debug.Log("<color=green>[GridManager] Đã khởi tạo lưới so le màu bàn cờ 4x6 khít nhau!</color>");
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
}