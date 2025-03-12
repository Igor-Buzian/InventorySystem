using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Item _storedItem;
    private Vector3 _startPosition;
    private Transform _originalParent;
    private Image _itemIcon;
    private Text _itemName;
    private Text _itemWeight;
    private Canvas _inventoryCanvas;

    private void Awake()
    {
        _itemIcon = GetComponentInChildren<Image>();
        Text[] texts = GetComponentsInChildren<Text>();
        if (texts.Length >= 2)
        {
            _itemName = texts[0];
            _itemWeight = texts[1];
        }
        _inventoryCanvas = GameObject.Find("InventoryCanvas").GetComponent<Canvas>();

        Inventory.Instance.OnItemAdded.AddListener(OnItemAdded);
        Inventory.Instance.OnItemRemoved.AddListener(OnItemRemoved);
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemAdded.RemoveListener(OnItemAdded);
            Inventory.Instance.OnItemRemoved.RemoveListener(OnItemRemoved);
        }
    }

    private void OnItemAdded(Item item)
    {
        if (item == _storedItem)
        {
            UpdateSlotUI();
        }
    }

    private void OnItemRemoved(Item item)
    {
        if (item == _storedItem)
        {
            ClearSlot();
        }
    }

    public void SetItem(Item item)
    {
        _storedItem = item;
        UpdateSlotUI();
    }

    private void UpdateSlotUI()
    {
        if (_storedItem != null)
        {
            _itemIcon.sprite = _storedItem.Icon;
            _itemName.text = _storedItem.Name;
            _itemWeight.text = $"{_storedItem.Weight} kg";
            _itemIcon.enabled = true;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        _storedItem = null;
        _itemIcon.sprite = null;
        _itemName.text = string.Empty;
        _itemWeight.text = string.Empty;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_storedItem == null) return;
        _startPosition = transform.position;
        _originalParent = transform.parent;
        transform.SetParent(_inventoryCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsOverInventory())
        {
            DropItemInWorld();
            Inventory.Instance.RemoveItem(_storedItem);
        }
        transform.position = _startPosition;
        transform.SetParent(_originalParent);
    }

    private void DropItemInWorld()
    {
        if (_storedItem == null) return;

        string poolName = $"{_storedItem.Type}Pool";
        GameObject worldItem = ObjectPool.Instance.Get(poolName);
        if (worldItem == null) return;

        worldItem.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2;
        worldItem.SetActive(true);

        Rigidbody rb = worldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
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
            if (result.gameObject.CompareTag("InventoryUI")) return true;
        }
        return false;
    }
}