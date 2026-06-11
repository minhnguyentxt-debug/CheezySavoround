using UnityEngine;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private DockManager dockManager;

    [Header("Drag Settings")]
    [SerializeField] private float liftHeight = 1.0f;
    [SerializeField] private LayerMask interactableLayer;

    private PizzaPlate selectedPlate = null;
    private DoublePlate selectedDoublePlate = null;
    private Vector3 originalPosition;
    private int sourceDockSlotIndex = -1;

    // BÍ QUYẾT: Tạo một biến tạm để ghi nhớ chính xác Collider của đĩa đang cầm
    private Collider cachedPlateCollider = null;
    private List<Collider> cachedDoublePlateColliders = new List<Collider>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickUpPlate();
        }

        if (Input.GetMouseButton(0) && (selectedPlate != null || selectedDoublePlate != null))
        {
            DragPlate();
        }

        if (Input.GetMouseButtonUp(0) && (selectedPlate != null || selectedDoublePlate != null))
        {
            DropPlate();
        }
    }

    private void TryPickUpPlate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // 1. Kiểm tra xem có trúng DoublePlate trước (bao gồm cả phần ở giữa)
            DoublePlate doublePlate = hit.collider.GetComponentInParent<DoublePlate>();
            if (doublePlate != null)
            {
                // Kiểm tra xem đĩa đôi này có đang nằm ở Dock không (tọa độ của đĩa 1 là -1)
                if (doublePlate.plate1 != null && doublePlate.plate1.CurrentX == -1 && doublePlate.plate1.CurrentZ == -1)
                {
                    sourceDockSlotIndex = dockManager.FindDockSlotForPlate(doublePlate.plate1);

                    if (sourceDockSlotIndex != -1)
                    {
                        selectedDoublePlate = doublePlate;
                        originalPosition = selectedDoublePlate.transform.position;
                        selectedDoublePlate.transform.position += Vector3.up * liftHeight;

                        // Tắt colliders của toàn bộ các con (bao gồm 2 đĩa đơn và collider đĩa đôi cha) để không cản trở kéo thả
                        cachedDoublePlateColliders.Clear();
                        foreach (Collider col in selectedDoublePlate.GetComponentsInChildren<Collider>())
                        {
                            if (col.enabled)
                            {
                                col.enabled = false;
                                cachedDoublePlateColliders.Add(col);
                            }
                        }
                        return; // Đã nhấc đĩa đôi thành công, thoát hàm sớm
                    }
                }
            }

            // 2. Nếu không trúng đĩa đôi, kiểm tra xem có trúng PizzaPlate đơn lẻ bình thường không
            PizzaPlate plate = hit.collider.GetComponentInParent<PizzaPlate>();

            if (plate != null && plate.CurrentX == -1 && plate.CurrentZ == -1)
            {
                sourceDockSlotIndex = dockManager.FindDockSlotForPlate(plate);

                if (sourceDockSlotIndex != -1)
                {
                    selectedPlate = plate;
                    originalPosition = selectedPlate.transform.position;
                    selectedPlate.transform.position += Vector3.up * liftHeight;

                    // Tắt collider để tránh cản trở raycast
                    cachedPlateCollider = selectedPlate.GetComponentInChildren<Collider>();
                    if (cachedPlateCollider != null)
                    {
                        cachedPlateCollider.enabled = false;
                    }
                }
            }
        }
    }

    private void DragPlate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, originalPosition.y + liftHeight, 0));
        float enter;

        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            if (selectedDoublePlate != null)
            {
                selectedDoublePlate.transform.position = hitPoint;
            }
            else if (selectedPlate != null)
            {
                selectedPlate.transform.position = hitPoint;
            }
        }
    }

    private void DropPlate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool placementSuccessful = false;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            Transform currentHitTransform = hit.collider.transform;
            string slotName = "";

            while (currentHitTransform != null)
            {
                if (currentHitTransform.name.StartsWith("Slot_"))
                {
                    slotName = currentHitTransform.name;
                    break;
                }
                currentHitTransform = currentHitTransform.parent;
            }

            if (!string.IsNullOrEmpty(slotName))
            {
                int openBracket = slotName.IndexOf('[');
                int comma = slotName.IndexOf(',');
                int closeBracket = slotName.IndexOf(']');

                if (openBracket != -1 && comma != -1 && closeBracket != -1)
                {
                    int targetX = int.Parse(slotName.Substring(openBracket + 1, comma - openBracket - 1));
                    int targetZ = int.Parse(slotName.Substring(comma + 1, closeBracket - comma - 1));

                    if (selectedDoublePlate != null)
                    {
                        // Xác định ô thứ hai của đĩa đôi dựa trên vị trí thực tế của plate2 (giữ nguyên góc quay kéo thả)
                        gridManager.GetCellCoordinates(selectedDoublePlate.plate2.transform.position, out int secondX, out int secondZ);

                        bool isAdjacent = Mathf.Abs(secondX - targetX) + Mathf.Abs(secondZ - targetZ) == 1;
                        bool slotsAreEmpty = gridManager.CanPlacePlate(targetX, targetZ);
                        bool secondSlotInBounds = secondX >= 0 && secondX < gridManager.Columns && secondZ >= 0 && secondZ < gridManager.Rows;
                        bool secondSlotIsEmpty = secondSlotInBounds && gridManager.CanPlacePlate(secondX, secondZ);

                        if (isAdjacent && slotsAreEmpty && secondSlotIsEmpty)
                        {
                            PizzaPlate p1 = selectedDoublePlate.plate1;
                            PizzaPlate p2 = selectedDoublePlate.plate2;

                            // Bật lại các colliders cho cả hai đĩa con
                            foreach (Collider col in cachedDoublePlateColliders)
                            {
                                if (col != null) col.enabled = true;
                            }

                            // Giữ nguyên góc xoay hiện tại của đĩa đôi cha, snap vị trí cha về điểm chính giữa 2 ô slot thế giới
                            Vector3 p1WorldPos = gridManager.GetSlotWorldPosition(targetX, targetZ);
                            Vector3 p2WorldPos = gridManager.GetSlotWorldPosition(secondX, secondZ);
                            selectedDoublePlate.transform.position = (p1WorldPos + p2WorldPos) * 0.5f;

                            // Đăng ký cả hai đĩa vào lưới
                            gridManager.AddPlateToGrid(p1, targetX, targetZ);
                            gridManager.AddPlateToGrid(p2, secondX, secondZ);

                            // Kích hoạt tính năng gộp bánh tự động
                            gridManager.CheckAndMergePizza(targetX, targetZ);
                            gridManager.CheckAndMergePizza(secondX, secondZ);

                            dockManager.EmptyDockSlot(sourceDockSlotIndex);
                            placementSuccessful = true;

                            selectedDoublePlate = null;

                            dockManager.SpawnNewPlatesToAllSlots();
                        }
                    }
                    else if (selectedPlate != null)
                    {
                        if (gridManager.CanPlacePlate(targetX, targetZ))
                        {
                            gridManager.AddPlateToGrid(selectedPlate, targetX, targetZ);
                            gridManager.CheckAndMergePizza(targetX, targetZ);

                            dockManager.EmptyDockSlot(sourceDockSlotIndex);
                            placementSuccessful = true;

                            if (selectedPlate != null && cachedPlateCollider != null)
                            {
                                cachedPlateCollider.enabled = true;
                            }

                            dockManager.SpawnNewPlatesToAllSlots();
                        }
                    }
                }
            }
        }

        // Trường hợp thả trượt ra ngoài ô lưới
        if (!placementSuccessful)
        {
            if (selectedDoublePlate != null)
            {
                foreach (Collider col in cachedDoublePlateColliders)
                {
                    if (col != null) col.enabled = true;
                }
                StartCoroutine(ReturnToSenderCoroutine(selectedDoublePlate.transform, originalPosition));
            }
            else if (selectedPlate != null)
            {
                if (cachedPlateCollider != null)
                {
                    cachedPlateCollider.enabled = true;
                }
                StartCoroutine(ReturnToSenderCoroutine(selectedPlate.transform, originalPosition));
            }
        }

        // Giải phóng biến tạm sạch sẽ
        selectedPlate = null;
        selectedDoublePlate = null;
        cachedPlateCollider = null;
        cachedDoublePlateColliders.Clear();
        sourceDockSlotIndex = -1;
    }

    private System.Collections.IEnumerator ReturnToSenderCoroutine(Transform targetTransform, Vector3 targetPos)
    {
        if (targetTransform == null) yield break;

        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 startPos = targetTransform.position;

        while (elapsed < duration)
        {
            if (targetTransform == null) yield break;
            elapsed += Time.deltaTime;
            targetTransform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        if (targetTransform != null) targetTransform.position = targetPos;
    }
}