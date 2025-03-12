using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string ID;
    public string Name;
    public float Weight;
    public ItemType Type;
    public Sprite Icon;
    public GameObject Prefab;

    private Rigidbody _rb;
    private Renderer _renderer;
    private Color _originalColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = Weight;
        _renderer = GetComponent<Renderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;
    }

    public void SetActiveState(bool active)
    {
        gameObject.SetActive(active);
        _rb.isKinematic = !active;
        _rb.useGravity = active;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_renderer != null) _renderer.material.color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    public void ResetState()
    {
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
}

public enum ItemType
{
    Weapon,
    Tool,
    Consumable
}