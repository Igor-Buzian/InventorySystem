using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private HashSet<Item> ItemsInBackpack = new HashSet<Item>();
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
        if (ItemsInBackpack.Contains(item))
        {
            Debug.Log($"Item with ID {item.ID} already exists in inventory.");
            return false; // Предмет уже существует
        }

        ItemsInBackpack.Add(item);
        OnItemAdded.Invoke(item);
        UpdateUI();
        Debug.Log($"Item with ID {item.ID} added to inventory.");
        return true;
    }

    public void RemoveItem(Item item)
    {
        if (ItemsInBackpack.Remove(item))
        {
            OnItemRemoved.Invoke(item);
            UpdateUI();
            Debug.Log($"Item with ID {item.ID} removed from inventory.");
        }
    }

    private void UpdateUI()
    {
        int index = 0;
        foreach (var item in ItemsInBackpack)
        {
            if (index < _inventorySlots.Length)
            {
                _inventorySlots[index].SetItem(item);
                index++;
            }
        }

        // Очищаем оставшиеся слоты
        for (; index < _inventorySlots.Length; index++)
        {
            _inventorySlots[index].ClearSlot();
        }
    }
}