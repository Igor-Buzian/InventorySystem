using UnityEngine;
using UnityEngine.EventSystems;

public class BackpackUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _characterBackpack;
    [SerializeField] private GameObject _takeBackpack;
    [SerializeField] private RectTransform _inventoryPanelRect;

    private Collider _backpackCollider;
    private bool _mouseOverUI;

    private void Start()
    {
        _backpackCollider = GetComponent<Collider>();
    }

    public void OnPointerEnter(PointerEventData eventData) => Debug.Log("Mouse entered UI");
    public void OnPointerExit(PointerEventData eventData) => Debug.Log("Mouse exited UI");

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Animal"))
        {
            _takeBackpack.SetActive(false);
            _characterBackpack.SetActive(true);
            _mouseOverUI = true;
            _backpackCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (!_mouseOverUI) return;

        if (Input.GetMouseButton(1))
        {
            _inventoryPanel.SetActive(true);
            Debug.Log("Inventory opened");
        }
        else
        {
            _inventoryPanel.SetActive(false);
        }
    }
}