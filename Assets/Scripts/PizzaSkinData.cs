using UnityEngine;

[CreateAssetMenu(fileName = "NewPizzaSkin", menuName = "PizzaGame/SkinData")]
public class PizzaSkinData : ScriptableObject
{
    public string skinId;
    public string skinName;
    public int price;
    public Sprite shopIcon;
    public Material plateMaterial; // Thay đổi diện mạo 3D của đĩa bánh
}