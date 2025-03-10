using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [System.Serializable]
    public class InventoryEvent : UnityEvent<string, string> { }

    public InventoryEvent OnInventoryAction = new InventoryEvent();

    [SerializeField] private Transform backpackAttachmentPoint;
    [SerializeField] private float itemMoveSpeed = 5f;

    private Dictionary<ItemType, Vector3> typePositions = new Dictionary<ItemType, Vector3>();
    private List<Item> itemsInBackpack = new List<Item>();

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

        InitializeTypePositions();
        OnInventoryAction.AddListener(HandleServerRequest);
    }

    private void InitializeTypePositions()
    {
        // Задаем позиции для каждого типа предметов
        typePositions.Add(ItemType.Weapon, backpackAttachmentPoint.position + new Vector3(0, 0.5f, 0));
        typePositions.Add(ItemType.Tool, backpackAttachmentPoint.position + new Vector3(-0.3f, 0.2f, 0));
        typePositions.Add(ItemType.Consumable, backpackAttachmentPoint.position + new Vector3(0.3f, 0.2f, 0));
    }

    public void AddItem(Item item)
    {
        StartCoroutine(MoveItemToPosition(item.transform, typePositions[item.Type]));
        itemsInBackpack.Add(item);
        item.SetInBackpack(true);
        OnInventoryAction.Invoke(item.ID, "add");
    }

    public void RemoveItem(Item item)
    {
        StartCoroutine(MoveItemToPosition(item.transform, GetDropPosition()));
        itemsInBackpack.Remove(item);
        item.SetInBackpack(false);
        OnInventoryAction.Invoke(item.ID, "remove");
    }

    private IEnumerator MoveItemToPosition(Transform itemTransform, Vector3 targetPosition)
    {
        while (Vector3.Distance(itemTransform.position, targetPosition) > 0.1f)
        {
            itemTransform.position = Vector3.Lerp(
                itemTransform.position, 
                targetPosition, 
                itemMoveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private void HandleServerRequest(string itemId, string action)
    {
        StartCoroutine(SendInventoryUpdate(itemId, action));
    }

    private IEnumerator SendInventoryUpdate(string itemId, string action)
    {
        WWWForm form = new WWWForm();
        form.AddField("item_id", itemId);
        form.AddField("action", action);

        using (UnityWebRequest www = UnityWebRequest.Post(
            "https://wadahub.manerai.com/api/inventory/status", 
            form))
        {
            www.SetRequestHeader("Authorization", "Bearer KPERnYCWAY46xayS8CezanosAgsWM84Nx7SKM4QBSqPq6c75tWF6xzhxPFb8MaP");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Server error: {www.error}");
            }
            else
            {
                Debug.Log($"Server response: {www.downloadHandler.text}");
            }
        }
    }

    private Vector3 GetDropPosition()
    {
        return backpackAttachmentPoint.position + backpackAttachmentPoint.forward * 2f;
    }
}