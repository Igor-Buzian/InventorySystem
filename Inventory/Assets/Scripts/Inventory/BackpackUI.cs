// BackpackUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private RectTransform[] typeSlots;

    private bool mouseOverUI;

    public void OnPointerEnter(PointerEventData eventData) => mouseOverUI = true;
    public void OnPointerExit(PointerEventData eventData) => mouseOverUI = false;

    private void Update()
    {
        if (Input.GetMouseButton(0) && mouseOverUI)
        {
            inventoryPanel.SetActive(true);
            UpdateUI();
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