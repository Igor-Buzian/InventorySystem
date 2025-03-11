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
    public GameObject Prefab; // Reference to the original prefab for spawning

    private Rigidbody rb;
    private Renderer rend;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = Weight;
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    // Enables or disables the world representation of the item.
    public void SetActiveState(bool active)
    {
        // When active, enable physics; when inactive, disable physics
        gameObject.SetActive(active);
        rb.isKinematic = !active;
        rb.useGravity = active;
    }

    // Change color on pointer enter for visual feedback.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rend != null)
            rend.material.color = Color.red;
    }

    // Restore original color on pointer exit.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (rend != null)
            rend.material.color = originalColor;
    }
    public void ResetState()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}

public enum ItemType
{
    Weapon,
    Tool,
    Consumable
}
