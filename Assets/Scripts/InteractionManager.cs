using UnityEngine;

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
    private Vector3 originalPosition;
    private int sourceDockSlotIndex = -1;

    // BÍ QUYẾT: Tạo một biến tạm để ghi nhớ chính xác Collider của đĩa đang cầm
    private Collider cachedPlateCollider = null;

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

        if (Input.GetMouseButton(0) && selectedPlate != null)
        {
            DragPlate();
        }

        if (Input.GetMouseButtonUp(0) && selectedPlate != null)
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
            PizzaPlate plate = hit.collider.GetComponentInParent<PizzaPlate>();

            if (plate != null && plate.CurrentX == -1 && plate.CurrentZ == -1)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (dockManager.GetPlateAtSlot(i) == plate)
                    {
                        sourceDockSlotIndex = i;
                        break;
                    }
                }

                if (sourceDockSlotIndex != -1)
                {
                    selectedPlate = plate;
                    originalPosition = selectedPlate.transform.position;
                    selectedPlate.transform.position += Vector3.up * liftHeight;

                    // KHẮC PHỤC 1: Tìm và ép biến ghi nhớ Collider ngay từ lúc này
                    cachedPlateCollider = selectedPlate.GetComponentInChildren<Collider>();
                    if (cachedPlateCollider != null)
                    {
                        cachedPlateCollider.enabled = false; // Tắt đi để tránh cản tia Raycast
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
            selectedPlate.transform.position = hitPoint;
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

                    if (gridManager.CanPlacePlate(targetX, targetZ))
                    {
                        gridManager.AddPlateToGrid(selectedPlate, targetX, targetZ);

                        // Kích hoạt kiểm tra gộp bánh (Có khả năng đĩa sẽ bị Destroy tại đây)
                        gridManager.CheckAndMergePizza(targetX, targetZ);

                        dockManager.EmptyDockSlot(sourceDockSlotIndex);
                        placementSuccessful = true;

                        // KHẮC PHỤC 2: Kiểm tra xem đĩa bánh có còn sống sót sau Combo không thì mới bật lại Collider
                        if (selectedPlate != null && cachedPlateCollider != null)
                        {
                            cachedPlateCollider.enabled = true;
                        }

                        dockManager.SpawnNewPlatesToAllSlots();
                    }
                }
            }
        }

        // Trường hợp thả trượt ra ngoài ô lưới
        if (!placementSuccessful)
        {
            // Bật lại Collider dựa trên biến nhớ tạm (chắc chắn thành công vì đĩa chưa bị hủy)
            if (cachedPlateCollider != null)
            {
                cachedPlateCollider.enabled = true;
            }

            StartCoroutine(ReturnToSenderCoroutine(selectedPlate, originalPosition));
        }

        // KHẮC PHỤC NGOẠI LỆ: Ép giải phóng bộ nhớ sạch sẽ dù bất kỳ kịch bản nào xảy ra
        selectedPlate = null;
        cachedPlateCollider = null;
        sourceDockSlotIndex = -1;
    }

    private System.Collections.IEnumerator ReturnToSenderCoroutine(PizzaPlate plate, Vector3 targetPos)
    {
        if (plate == null) yield break;

        float elapsed = 0f;
        float duration = 0.15f;
        Vector3 startPos = plate.transform.position;

        while (elapsed < duration)
        {
            if (plate == null) yield break; // Phòng thủ nếu đĩa bị hủy đột ngột lúc đang bay
            elapsed += Time.deltaTime;
            plate.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        if (plate != null) plate.transform.position = targetPos;
    }
}