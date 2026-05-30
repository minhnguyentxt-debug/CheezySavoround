using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private DockManager dockManager;

    [Header("Drag Settings")]
    [SerializeField] private float liftHeight = 1.0f; // Độ cao đĩa bánh được nhấc lên khi đang kéo
    [SerializeField] private LayerMask interactableLayer; // Thiết lập Layer để Raycast lọc chính xác Đĩa và Ô lưới

    private PizzaPlate selectedPlate = null;   // Chiếc đĩa đang được chọn để kéo
    private Vector3 originalPosition;          // Vị trí gốc dưới ô Dock đề phòng trường hợp thả lỗi thì bay về
    private int sourceDockSlotIndex = -1;      // Lưu lại vị trí ô Dock xuất phát

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
        // 1. NHẤN CHUỘT: Bắn tia Raycast để tìm và nhấc đĩa bánh từ dưới Dock lên
        if (Input.GetMouseButtonDown(0))
        {
            TryPickUpPlate();
        }

        // 2. GIỮ CHUỘT: Di chuyển đĩa bánh mượt mà theo con trỏ chuột trên mặt phẳng ảo
        if (Input.GetMouseButton(0) && selectedPlate != null)
        {
            DragPlate();
        }

        // 3. THẢ CHUỘT: Tìm ô lưới bên dưới để đặt bánh vào hoặc trả về Dock cũ
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
            // Kiểm tra xem đối tượng va chạm có chứa script PizzaPlate không
            PizzaPlate plate = hit.collider.GetComponentInParent<PizzaPlate>();

            // Điều kiện: Tìm thấy đĩa và đĩa đó phải nằm ở dưới Dock (chưa được đưa lên Lưới)
            if (plate != null && plate.CurrentX == -1 && plate.CurrentZ == -1)
            {
                // Dò tìm xem chiếc đĩa này đang nằm ở slot mấy của Dock
                for (int i = 0; i < 3; i++) // Giả định dock có 3 ô chờ
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

                    // Nhấc nhẹ đĩa bánh lên theo trục Y để tạo hiệu ứng trực quan "đang cầm kéo đi"
                    selectedPlate.transform.position += Vector3.up * liftHeight;
                }
            }
        }
    }

    private void DragPlate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Tạo một mặt phẳng toán học nằm ngang ở độ cao mong muốn (Y = vị trí gốc + độ nhấc)
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, originalPosition.y + liftHeight, 0));
        float enter;

        // Bắn tia xuyên qua mặt phẳng ảo để lấy tọa độ 3D chính xác của chuột
        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            // Cập nhật vị trí đĩa bánh đuổi theo con trỏ chuột mượt mà
            selectedPlate.transform.position = hitPoint;
        }
    }

    private void DropPlate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool placementSuccessful = false;

        // Bắn tia Raycast thẳng từ đĩa bánh xuống dưới xem có trúng ô lưới (Slot) nào không
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Tìm ô lưới dựa vào tên hoặc script được gắn trên ô thớt lưới
            if (hit.collider.name.StartsWith("Slot_"))
            {
                // Trích xuất tọa độ X, Z từ tên ô lưới "Slot_[x,z]" mà GridManager đặt tên
                string slotName = hit.collider.name;
                int openBracket = slotName.IndexOf('[');
                int comma = slotName.IndexOf(',');
                int closeBracket = slotName.IndexOf(']');

                if (openBracket != -1 && comma != -1 && closeBracket != -1)
                {
                    int targetX = int.Parse(slotName.Substring(openBracket + 1, comma - openBracket - 1));
                    int targetZ = int.Parse(slotName.Substring(comma + 1, closeBracket - comma - 1));

                    // Kiểm tra với GridManager xem ô này có trống không
                    if (gridManager.CanPlacePlate(targetX, targetZ))
                    {
                        // Đưa đĩa vào ma trận lưới và cố định vị trí hình học
                        gridManager.AddPlateToGrid(selectedPlate, targetX, targetZ);

                        // Giải phóng ô chờ dưới Dock để chuẩn bị sinh lượt bánh mới
                        dockManager.EmptyDockSlot(sourceDockSlotIndex);

                        placementSuccessful = true;

                        // Tự động kích hoạt cơ chế sinh thêm đĩa mới nếu các ô dưới Dock bị trống trống
                        dockManager.SpawnNewPlatesToAllSlots();
                    }
                }
            }
        }

        // Nếu thả trượt hoặc thả vào ô đã có bánh -> Trả đĩa bay về vị trí cũ dưới Dock
        if (!placementSuccessful)
        {
            StartCoroutine(ReturnToSenderCoroutine(selectedPlate, originalPosition));
        }

        // Reset trạng thái quản lý biến tạm
        selectedPlate = null;
        sourceDockSlotIndex = -1;
    }

    /// <summary>
    /// Hiệu ứng Lerp mượt mà giúp đĩa bánh tự động bay ngược về Dock cũ nếu người chơi thả lỗi
    /// </summary>
    private System.Collections.IEnumerator ReturnToSenderCoroutine(PizzaPlate plate, Vector3 targetPos)
    {
        float elapsed = 0f;
        float duration = 0.15f; // Thời gian bay về là 0.15 giây
        Vector3 startPos = plate.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            plate.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        plate.transform.position = targetPos;
    }
}