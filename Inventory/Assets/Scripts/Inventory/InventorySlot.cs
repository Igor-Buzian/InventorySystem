using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Item storedItem;
    private Vector3 startPosition;
    private Transform originalParent;
    private Image itemIcon;
    private Text itemName;
    private Text itemWeight;

    private void Awake()
    {
        itemIcon = GetComponentInChildren<Image>();
        Text[] texts = GetComponentsInChildren<Text>();

        if (texts.Length >= 2)
        {
            itemName = texts[0];
            itemWeight = texts[1];
        }
    }

    public void SetItem(Item item)
    {
        storedItem = item;
        itemIcon.sprite = item.Icon;
        itemName.text = item.Name;
        itemWeight.text = $"{item.Weight} кг";
        gameObject.SetActive(true);
    }

    public void ClearSlot()
    {
        storedItem = null;
        itemIcon.sprite = null;
        itemName.text = "";
        itemWeight.text = "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (storedItem == null) return;
        startPosition = transform.position;
        originalParent = transform.parent;
        transform.SetParent(GameObject.Find("InventoryCanvas").transform);
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsOverInventory())
        {
            DropItemInWorld();
            Inventory.Instance.RemoveItem(storedItem);
        }
        transform.position = startPosition;
        transform.SetParent(originalParent);
    }

    private void DropItemInWorld()
    {
        if (storedItem == null) return;

        GameObject worldItem = ObjectPool.Instance.Get(storedItem.Type.ToString() + "Pool");
        if (worldItem == null) return;

        // Установка позиции объекта в мир
        worldItem.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2;

        // Активация объекта и настройка физики
        worldItem.SetActive(true);
        Rigidbody rb = worldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Убедитесь, что он не кинематический
            rb.useGravity = true; // Включить гравитацию
        }

        ClearSlot();
    }

    private bool IsOverInventory()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
                return true;
        }
        return false;
    }
}