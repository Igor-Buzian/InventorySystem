using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    // List of items currently in the backpack
    public List<Item> ItemsInBackpack { get; private set; } = new List<Item>();

    [SerializeField] private InventorySlot[] inventorySlots; // Array of UI slots

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Adds an item to the inventory if not already present.
    public bool AddItem(Item item)
    {
        if (ItemsInBackpack.Exists(i => i.ID == item.ID))
            return false;

        ItemsInBackpack.Add(item);
        UpdateUI();
        return true;
    }

    // Removes an item from the inventory.
    public void RemoveItem(Item item)
    {
        if (ItemsInBackpack.Remove(item))
        {
            UpdateUI();
        }
    }

    // Updates the UI slots to reflect the items in the backpack.
    private void UpdateUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < ItemsInBackpack.Count)
            {
                if (ItemsInBackpack[i] != null)
                    inventorySlots[i].SetItem(ItemsInBackpack[i]);
                else
                    inventorySlots[i].ClearSlot();
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }
}
