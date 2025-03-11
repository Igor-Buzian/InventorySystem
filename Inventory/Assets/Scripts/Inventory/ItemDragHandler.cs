using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Item))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Item item;
    private Vector3 startPosition;
    private Transform originalParent;
    float minHeight = 2f;
    private void Awake() => item = GetComponent<Item>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;
        transform.SetParent(GameObject.Find("InventoryCanvas").transform);
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        bool isOverInventory = IsOverInventorySlot(out InventorySlot slot);

        if (isOverInventory)
        {
            HandleInventoryDrop();
        }
        else
        {
            HandleWorldDrop(eventData); // Передаем eventData, чтобы использовать его для получения позиции
        }

        ResetPosition();
    }

    private void HandleInventoryDrop()
    {
        if (!Inventory.Instance.ItemsInBackpack.Contains(item))
        {
            Inventory.Instance.AddItem(item);
         //   gameObject.SetActive(false); // Отключаем оригинал
        }
    }

    private void HandleWorldDrop(PointerEventData eventData)
    {
        Debug.Log($"Dropped {item.Name} in world");
        Inventory.Instance.RemoveItem(item);
        InstantiateWorldItem(eventData); // Передаем eventData
    }

    private void InstantiateWorldItem(PointerEventData eventData)
    {
        string poolName = "";

        switch (item.Type)
        {
            case ItemType.Weapon:
                poolName = "WeaponPool";
                break;
            case ItemType.Tool:
                poolName = "ToolPool";
                break;
            case ItemType.Consumable:
                poolName = "ConsumablePool";
                break;
        }

        GameObject worldItem = ObjectPool.Instance.Get(poolName);

        if (worldItem == null)
        {
            Debug.LogError("No available objects in pool: " + poolName);
            return;
        }

        // Получаем позицию, где была отпущена кнопка
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 dropPosition = hit.point;
            dropPosition.y += 1.0f; // Поднимаем над землей

            worldItem.transform.position = dropPosition; // Устанавливаем новую позицию
            worldItem.SetActive(true); // Убедитесь, что объект активен

            Rigidbody rb = worldItem.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(Vector3.up * 2, ForceMode.Impulse);
            }
        }
    }
    private void ResetPosition()
    {
        transform.SetParent(originalParent); // Возвращаем объект к его оригинальному родителю

        Vector3 newPosition = transform.position;
        newPosition.y += 1.0f; // Поднимаем над землей
        transform.position = newPosition;
    }
    private bool IsOverInventorySlot(out InventorySlot slot)
    {
        slot = null;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null) return true;
        }
        return false;
    }
}