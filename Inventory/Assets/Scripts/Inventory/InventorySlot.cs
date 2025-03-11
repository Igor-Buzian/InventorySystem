using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemName;
    [SerializeField] private Text itemWeight;

    private Item storedItem;
    private Vector3 startPosition;
    private Transform originalParent;

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

        Debug.Log($"🎒 Начали перетаскивать {storedItem.Name}");
        startPosition = transform.position;
        originalParent = transform.parent;
        transform.SetParent(GameObject.Find("InventoryCanvas").transform); // Поднимаем UI
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

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

        Debug.Log($"📦 Выбрасываем {storedItem.Name} в мир!");
        GameObject worldItem = Instantiate(storedItem.gameObject);
        worldItem.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2; // Перед игроком
        worldItem.SetActive(true);

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
            {
                return true;
            }
        }

        return false;
    }
}
