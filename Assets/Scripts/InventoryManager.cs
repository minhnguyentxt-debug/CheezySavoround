using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public ItemSlotUI[] slots; // Kéo các ô từ Inspector vào
    public List<ItemData> startingItems; // Kéo các file ItemData vào

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < startingItems.Count)
            {
                slots[i].Setup(startingItems[i]);
            }
        }
    }
}