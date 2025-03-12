using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public List<Item> ItemsInBackpack { get; private set; } = new List<Item>();
    [SerializeField] private InventorySlot[] _inventorySlots;

    public UnityEvent<Item> OnItemAdded = new UnityEvent<Item>();
    public UnityEvent<Item> OnItemRemoved = new UnityEvent<Item>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool AddItem(Item item)
    {
        if (ItemsInBackpack.Exists(i => i.ID == item.ID)) return false;
        ItemsInBackpack.Add(item);
        OnItemAdded.Invoke(item);
        UpdateUI();
        return true;
    }

    public void RemoveItem(Item item)
    {
        if (ItemsInBackpack.Remove(item))
        {
            OnItemRemoved.Invoke(item);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            if (i < ItemsInBackpack.Count && ItemsInBackpack[i] != null)
                _inventorySlots[i].SetItem(ItemsInBackpack[i]);
            else
                _inventorySlots[i].ClearSlot();
        }
    }
}