using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [SerializeField] private InventorySlot[] inventorySlots; // Массив UI-слотов
    private List<Item> itemsInBackpack = new List<Item>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddItem(Item item)
    {
        // Предполагаем, что один и тот же предмет добавлять нельзя
        if (itemsInBackpack.Contains(item))
            return;

        itemsInBackpack.Add(item);
        item.SetInBackpack(true);
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Заполняем слоты предметами из инвентаря
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < itemsInBackpack.Count)
                inventorySlots[i].SetItem(itemsInBackpack[i]);
            else
                inventorySlots[i].ClearSlot();
        }
    }
}
