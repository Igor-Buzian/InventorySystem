using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject CharacterBackPack;
    [SerializeField] private GameObject TakeBackPack;
    [SerializeField] private RectTransform[] typeSlots;
    private Collider BackPakCollider;
    private bool mouseOverUI;

    private void Start()
    {
        BackPakCollider = GetComponent<Collider>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      //  mouseOverUI = true;
        Debug.Log("Mouse entered backpack UI.");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       // mouseOverUI = false;
        Debug.Log("Mouse exited backpack UI.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Animal"))
        {
            TakeBackPack.SetActive(false);
            CharacterBackPack.SetActive(true);
            mouseOverUI = true;
            BackPakCollider.enabled = false;
        }
    }

    private void Update()
    {
        if(!mouseOverUI)return;
        if (Input.GetMouseButton(1))
        {
            inventoryPanel.SetActive(true);
            UpdateUI();
            Debug.Log("Inventory panel opened.");
        }
        else
        {
            inventoryPanel.SetActive(false);
        }
    }
    private void UpdateUI()
    {
        // Implementation for updating UI based on inventory
    }
}