using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Item))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Item item;
    private Vector3 startPosition;
    private Transform originalParent;
    private float minGroundY = 0.5f; // Minimum ground height

    private void Awake() => item = GetComponent<Item>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;
        // Bring the item to the front by setting its parent to the InventoryCanvas
        GameObject canvas = GameObject.Find("InventoryCanvas");
        if (canvas != null)
            transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsOverInventorySlot(out InventorySlot slot))
        {
            if (Inventory.Instance.AddItem(item))
            {
                item.SetActiveState(false); // Отключите только визуальную репрезентацию
                SendToServer(item.ID, true);
                // Не уничтожаем объект, просто возвращаем его в пул
                ObjectPool.Instance.Return(item.Type.ToString() + "Pool", gameObject);
            }
        }
        else
        {
            Inventory.Instance.RemoveItem(item);
            SendToServer(item.ID, false);
            DropItemInWorld(eventData);
        }
        ResetPosition();
    }

    private void DropItemInWorld(PointerEventData eventData)
    {
        string poolName = item.Type.ToString() + "Pool";
        GameObject worldItem = ObjectPool.Instance.Get(poolName);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 dropPos;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            dropPos = hit.point;
            dropPos.y = Mathf.Max(dropPos.y, 0.5f); // Убедитесь, что не ниже уровня земли
        }
        else
        {
            dropPos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        }

        worldItem.transform.position = dropPos;
        worldItem.SetActive(true);

        // Настройка физики
        Rigidbody rb = worldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    // Checks if the pointer is over an inventory slot.
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
                return true;
        }
        return false;
    }

    private void ResetPosition()
    {
        transform.SetParent(originalParent);
        Vector3 newPos = transform.position;
        newPos.y = Mathf.Max(newPos.y, 2f); // Не допускаем падения под землю
        transform.position = newPos;
    }

    private void SendToServer(string itemId, bool added)
    {
        Debug.Log(added ? $"Sending to server: {itemId} added to inventory" : $"Sending to server: {itemId} removed from inventory");
        // Implement server communication here if needed.
    }
}
