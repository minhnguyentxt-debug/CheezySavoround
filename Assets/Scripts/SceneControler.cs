using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject confirmPopupPanel; // Kéo thả Panel bảng hỏi vào đây

    private void Start()
    {
        // Khi game vừa chạy, tự động ẩn bảng hỏi đi
        if (confirmPopupPanel != null)
        {
            confirmPopupPanel.SetActive(false);
        }
    }
    /// <summary>
    /// Hàm gắn vào nút PLAY chính ở Main Menu
    /// </summary>
    public void OnClickPlayButton()
    {
        // Kiểm tra xem trong file Save đã có đĩa bánh nào chưa
        if (SaveManager.Instance != null && SaveManager.Instance.PlayerData.Plates.Count > 0)
        {
            // Nếu CÓ dữ liệu cũ -> Hiện bảng hỏi "Bạn muốn chơi tiếp không?"
            if (confirmPopupPanel != null)
            {
                confirmPopupPanel.SetActive(true);
            }
        }
        else
        {
            // Nếu KHÔNG CÓ dữ liệu cũ -> Vào thẳng game luôn (Chơi mới)
            LoadGameScene();
        }
    }

    /// <summary>
    /// Hàm gắn vào nút "TIẾP TỤC" trên bảng hỏi
    /// </summary>
    public void OnClickContinue()
    {
        LoadGameScene();
    }

    /// <summary>
    /// Hàm gắn vào nút "CHƠI MỚI" trên bảng hỏi
    /// </summary>
    public void OnClickNewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.PlayerData.Plates.Clear();

            ScoreManager.Instance.ResetScoreForNewGame();

            SaveManager.Instance.SaveGame(); // Lưu lại file trống trước khi vào game
        }

        LoadGameScene();
    }

    /// <summary>
    /// Hàm gắn vào nút đóng (X) nếu người chơi muốn quay lại Main Menu
    /// </summary>
    public void OnClickClosePopup()
    {
        if (confirmPopupPanel != null)
        {
            confirmPopupPanel.SetActive(false);
        }
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadHomeScene()
    {
        if (SaveManager.Instance != null)
        {
            GridManager gridManager = FindAnyObjectByType<GridManager>();
            if (gridManager != null)
            {
                SaveManager.Instance.PlayerData.Plates = gridManager.GetCurrentGridState();
            }
            SaveManager.Instance.SaveGame();
            Debug.Log("[SceneController] Đã lấy dữ liệu lưới và lưu thành công!");
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}