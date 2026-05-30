using System.Collections.Generic;
using UnityEngine;

public class DockManager : MonoBehaviour
{
    [Header("Dock Settings")]
    [SerializeField] private int dockSlotCount = 3;
    [SerializeField] private float slotSpacing = 4.0f;
    [SerializeField] private Vector3 dockOffset = new Vector3(0f, 0.1f, -10.0f);

    [Header("Prefabs")]
    [SerializeField] private GameObject platePrefab; // Plate_Base_Prefab có chứa đĩa nền mặc định

    private Vector3[] dockPositions;
    // QUAN TRỌNG: Mảng một chiều quản lý duy nhất 1 Đĩa tại mỗi ô chờ
    private PizzaPlate[] dockPlates;

    void Start()
    {
        InitializeDock();
        SpawnNewPlatesToAllSlots();
    }

    private void InitializeDock()
    {
        dockPositions = new Vector3[dockSlotCount];
        dockPlates = new PizzaPlate[dockSlotCount];

        float totalWidth = (dockSlotCount - 1) * slotSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < dockSlotCount; i++)
        {
            dockPositions[i] = transform.position + dockOffset + new Vector3(startX + (i * slotSpacing), 0f, 0f);
        }
    }

    public void SpawnNewPlatesToAllSlots()
    {
        for (int i = 0; i < dockSlotCount; i++)
        {
            if (dockPlates[i] == null)
            {
                SpawnRandomPlateAtSlot(i);
            }
        }
    }

    private void SpawnRandomPlateAtSlot(int slotIndex)
    {
        // 1. Sinh duy nhất 1 chiếc đĩa tại ô dock
        GameObject plateObj = Instantiate(platePrefab, dockPositions[slotIndex], Quaternion.identity, transform);
        plateObj.name = $"Dock_Plate_{slotIndex}";

        PizzaPlate plateScript = plateObj.GetComponent<PizzaPlate>();
        if (plateScript != null)
        {
            // 2. Ngẫu nhiên số lượng lát bánh đặt lên đĩa từ 1 đến 6
            int randomSliceCount = Random.Range(1, 7);

            // 3. Chọn ngẫu nhiên 1 màu/vị cho toàn bộ lát bánh trên đĩa này (Bỏ qua vị None ở index 0)
            ToppingType randomColor = (ToppingType)Random.Range(1, 7);

            List<ToppingType> randomSlices = new List<ToppingType>();
            for (int i = 0; i < randomSliceCount; i++)
            {
                randomSlices.Add(randomColor);
            }

            // 4. Đẩy danh sách lát bánh vào đĩa xử lý trực quan
            plateScript.SetupPlate(randomSlices, -1, -1);
            dockPlates[slotIndex] = plateScript;
        }
    }

    public PizzaPlate GetPlateAtSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < dockSlotCount) return dockPlates[slotIndex];
        return null;
    }

    public void EmptyDockSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < dockSlotCount)
        {
            dockPlates[slotIndex] = null;
        }
    }
}