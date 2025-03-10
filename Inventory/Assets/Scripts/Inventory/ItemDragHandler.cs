// ItemDragHandler.cs
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
        startPosition = transform.position;
        originalParent = transform.parent;
        item.SetInBackpack(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsOverBackpackUI())
        {
            Inventory.Instance.AddItem(item);
        }
        else
        {
            transform.position = startPosition;
            transform.SetParent(originalParent);
        }
    }

    private bool IsOverBackpackUI()
    {
        return EventSystem.current.IsPointerOverGameObject() && 
               EventSystem.current.currentSelectedGameObject?.GetComponent<BackpackUI>();
    }
}