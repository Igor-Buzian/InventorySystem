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
    public GameObject Prefab; // Added prefab reference

    private Rigidbody rb;
    private Renderer rend;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = Weight;
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    public void SetInBackpack(bool inBackpack)
    {
        rb.isKinematic = inBackpack;
        rb.useGravity = !inBackpack;
        gameObject.SetActive(!inBackpack);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rend != null)
            rend.material.color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rend != null)
            rend.material.color = originalColor;
    }
}

public enum ItemType
{
    Weapon,
    Tool,
    Consumable
}