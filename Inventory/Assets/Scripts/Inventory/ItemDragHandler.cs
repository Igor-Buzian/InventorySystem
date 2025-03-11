using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Item))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Item item;
    private Vector3 startPosition;
    private Transform originalParent;

    private void Awake() => item = GetComponent<Item>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Начало перетаскивания: {gameObject.name}");
        startPosition = transform.position;
        originalParent = transform.parent;
        // Перемещаем объект в Canvas для корректного отображения (имя Canvas должно совпадать)
        GameObject canvasObj = GameObject.Find("InventoryCanvas");
        if (canvasObj != null)
        {
            transform.SetParent(canvasObj.transform);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsOverInventorySlot(out InventorySlot slot))
        {
            Debug.Log("Предмет отпущен над слотом инвентаря.");
            Inventory.Instance.AddItem(item);
            // Вместо удаления объекта меняем его цвет на красный
            item.SetColor(Color.red);
            // При желании можно вернуть объект обратно в исходного родителя:
            // transform.SetParent(originalParent);
        }
        else
        {
            Debug.Log("Предмет отпущен вне слота инвентаря.");
            transform.position = startPosition;
            transform.SetParent(originalParent);
        }
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
            if (slot != null)
            {
                Debug.Log($"Обнаружен слот инвентаря: {slot.name}");
                return true;
            }
        }
        Debug.Log("Слот инвентаря не обнаружен под курсором.");
        return false;
    }
}
