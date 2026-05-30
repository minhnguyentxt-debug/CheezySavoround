using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 6;
    [SerializeField] private float cellSize = 1.5f; // Khoảng cách giữa tâm các ô
    [SerializeField] private float spacing = 0.2f;  // Khoảng cách an toàn giữa các viền ô
    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab; 
    private GameObject[,] gridMatrix;

    void Start()
    {
        GenerateGrid();
    }
    private void GenerateGrid()
    {
        gridMatrix = new GameObject[columns, rows];

        // Tính toán độ lệch
        float totalWidth = (columns - 1) * (cellSize + spacing);
        float totalHeight = (rows - 1) * (cellSize + spacing);
        Vector3 originOffset = new Vector3(-totalWidth / 2f, 0f, -totalHeight / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                // Tính toán vị trí 3D chính xác + kéo thả
                float posX = x * (cellSize + spacing);
                float posZ = z * (cellSize + spacing);
                Vector3 spawnPosition = new Vector3(posX, 0f, posZ) + originOffset + transform.position;
                GameObject newSlot = Instantiate(slotPrefab, spawnPosition, Quaternion.identity, transform);
                newSlot.name = $"Slot_[{x},{z}]";
                gridMatrix[x, z] = newSlot;
            }
        }

        Debug.Log($"<color=green>Grid System 4x6 initialized successfully!</color>");
    }
    public GameObject GetSlotAt(int x, int z)
    {
        if (x >= 0 && x < columns && z >= 0 && z < rows)
        {
            return gridMatrix[x, z];
        }
        return null;
    }
}