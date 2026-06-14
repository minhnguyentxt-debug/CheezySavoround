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
            // Kiểm tra xem có trúng PizzaPlate đơn lẻ bình thường không
            PizzaPlate plate = hit.collider.GetComponentInParent<PizzaPlate>();

            if (plate != null && plate.CurrentX == -1 && plate.CurrentZ == -1)
            {
                sourceDockSlotIndex = dockManager.FindDockSlotForPlate(plate);

                if (sourceDockSlotIndex != -1)
                {
                    selectedPlate = plate;
                    originalPosition = selectedPlate.transform.position;
                    selectedPlate.transform.position += Vector3.up * liftHeight;

                    // Phát âm thanh cầm đĩa lên
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPickupSound();
                    }

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
            if (selectedPlate != null)
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

                    if (selectedPlate != null)
                    {
                        if (gridManager.CanPlacePlate(targetX, targetZ))
                        {
                            gridManager.AddPlateToGrid(selectedPlate, targetX, targetZ);
                            
                            // Phát âm thanh đặt đĩa xuống
                            if (AudioManager.Instance != null)
                            {
                                AudioManager.Instance.PlayPlaceSound();
                            }
                            
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
            if (selectedPlate != null)
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
        cachedPlateCollider = null;
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