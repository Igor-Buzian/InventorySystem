using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemName;
    [SerializeField] private Text itemWeight;

    public void SetItem(Item item)
    {
        itemIcon.sprite = item.Icon; // ✅ Берем иконку из `Item`
        itemName.text = item.Name;
        itemWeight.text = $"{item.Weight} кг";
        gameObject.SetActive(true);  // ✅ Делаем слот активным
    }

    public void ClearSlot()
    {
        itemIcon.sprite = null;
        itemName.text = "";
        itemWeight.text = "";
       
    }
}
