using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public string ID;
    public string Name;
    public float Weight;
    public ItemType Type;
    public Sprite Icon;  // Иконка для отображения в инвентаре

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = Weight;
    }

    public void SetInBackpack(bool inBackpack)
    {
        rb.isKinematic = inBackpack;
        rb.useGravity = !inBackpack;
    }

    // Метод для изменения цвета объекта
    public void SetColor(Color newColor)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = newColor;
        }
    }
}

public enum ItemType
{
    Weapon,
    Tool,
    Consumable
}
