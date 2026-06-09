using UnityEngine;

public enum ItemEffectType { CreatePizza, AddSauce, SwapPosition, RemovePizza }

[CreateAssetMenu(fileName = "NewItem", menuName = "PizzaGame/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemEffectType effectType;
    public int maxUses = 3; // Số lần sử dụng mặc định
    [HideInInspector] public int currentUses; // Số lần còn lại
}