using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public List<Item> ItemsInBackpack { get; private set; } = new List<Item>();

    [SerializeField] private InventorySlot[] inventorySlots;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AddItem(Item item)
    {
        if (ItemsInBackpack.Exists(i => i.ID == item.ID)) return false;

        ItemsInBackpack.Add(item);
        UpdateUI();
        return true;
    }

    public void RemoveItem(Item item)
    {
        if (ItemsInBackpack.Remove(item))
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < ItemsInBackpack.Count)
                inventorySlots[i].SetItem(ItemsInBackpack[i]);
            else
                inventorySlots[i].ClearSlot();
        }
    }
}