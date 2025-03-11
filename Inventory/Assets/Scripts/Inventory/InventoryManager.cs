using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
   /* [SerializeField] private List<Item> itemsInBackpack = new List<Item>(); // Список предметов в рюкзаке
    public static InventoryManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddItem(Item item)
    {
        if (itemsInBackpack.Contains(item))
        {
            Debug.LogWarning($"Item {item.Name} is already in the backpack.");
            return;
        }

        itemsInBackpack.Add(item);
        item.SetInBackpack(true);
        Debug.Log($"Item {item.Name} added to the backpack.");
    }

    public void RemoveItem(Item item)
    {
        if (!itemsInBackpack.Contains(item))
        {
            Debug.LogWarning($"Item {item.Name} is not in the backpack.");
            return;
        }

        itemsInBackpack.Remove(item);
        item.SetInBackpack(false);
        Debug.Log($"Item {item.Name} removed from the backpack.");
    }

    public List<Item> GetItems()
    {
        return itemsInBackpack;
    }*/
}