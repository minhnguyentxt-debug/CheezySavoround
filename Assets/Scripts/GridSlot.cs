using UnityEngine;

public class GridSlot : MonoBehaviour
{
    private int x;
    private int z;
    private GridManager gridManager;

    // Hàm này giúp GridManager gán tọa độ khi khởi tạo
    public void Setup(int x, int z, GridManager manager)
    {
        this.x = x;
        this.z = z;
        this.gridManager = manager;
    }

    private void OnMouseDown()
    {
        // Khi click vào ô này, nó gọi ngược lại GridManager
        if (gridManager != null)
        {
            gridManager.OnCellClick(x, z);
        }
    }
}