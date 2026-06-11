using UnityEngine;

public class DoublePlate : MonoBehaviour
{
    public PizzaPlate plate1;
    public PizzaPlate plate2;
    public bool isVertical; // true nếu đĩa đôi xếp dọc (lệch theo trục Z), false nếu xếp ngang (lệch theo trục X)

    private void Start()
    {
        HideSinglePlateMeshes();
    }

    public void HideSinglePlateMeshes()
    {
        if (plate1 != null) plate1.SetPlateMeshActive(false);
        if (plate2 != null) plate2.SetPlateMeshActive(false);
    }

    public void SetupDoublePlate(PizzaPlate p1, PizzaPlate p2, bool vertical, float offsetDist)
    {
        plate1 = p1;
        plate2 = p2;
        isVertical = vertical;

        // Gán làm con của container cha
        plate1.transform.SetParent(transform);
        plate2.transform.SetParent(transform);

        // Đặt vị trí cục bộ (local position) đối xứng qua tâm (0, 0, 0)
        float halfOffset = offsetDist / 2f;

        if (isVertical)
        {
            plate1.transform.localPosition = new Vector3(0f, 0f, -halfOffset);
            plate2.transform.localPosition = new Vector3(0f, 0f, halfOffset);
        }
        else
        {
            plate1.transform.localPosition = new Vector3(-halfOffset, 0f, 0f);
            plate2.transform.localPosition = new Vector3(halfOffset, 0f, 0f);
        }

        HideSinglePlateMeshes();
    }

    public void SnapToGrid(Vector3 p1Pos, Vector3 p2Pos)
    {
        transform.position = p1Pos;
        
        // Tính toán vector hướng từ p1 đến p2
        Vector3 direction = p2Pos - p1Pos;
        direction.y = 0f; // Chỉ xoay quanh trục Y

        if (direction != Vector3.zero)
        {
            // Xoay sao cho trục X của parent (hướng của đĩa 2) chỉ thẳng đến vị trí p2
            transform.rotation = Quaternion.FromToRotation(Vector3.right, direction.normalized);
        }
    }

    public void BreakDoublePlate()
    {
        if (plate1 != null)
        {
            plate1.transform.SetParent(null);
            plate1.SetPlateMeshActive(true);
        }
        if (plate2 != null)
        {
            plate2.transform.SetParent(null);
            plate2.SetPlateMeshActive(true);
        }
        Destroy(gameObject);
    }
}
