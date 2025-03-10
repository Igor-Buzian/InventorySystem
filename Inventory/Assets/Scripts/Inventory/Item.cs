using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public string ID; // Уникальный идентификатор предмета
    public string Name; // Название предмета
    public float Weight; // Вес предмета
    public ItemType Type; // Тип предмета (Weapon, Tool, Consumable)

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = Weight; // Устанавливаем массу предмета
    }

    /// <summary>
    /// Устанавливает состояние предмета (в рюкзаке или нет).
    /// </summary>
    public void SetInBackpack(bool inBackpack)
    {
        rb.isKinematic = inBackpack; // Отключаем физику, если предмет в рюкзаке
        rb.useGravity = !inBackpack; // Включаем гравитацию, если предмет не в рюкзаке
    }
}

public enum ItemType
{
    Weapon,
    Tool,
    Consumable
}