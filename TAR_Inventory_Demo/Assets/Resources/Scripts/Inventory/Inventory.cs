using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<ScriptableObject> items = new List<ScriptableObject>();

    private void Start()
    {
        foreach (var item in items)
        {
            if (item is IInteractableObject interactable)
            {
                Debug.Log($"Item Name: {interactable.ItemName}");
                Debug.Log($"Description: {interactable.ItemDescription}");
            }
        }
    }

    public void UseItem(int index)
    {
        if (index >= 0 && index < items.Count && items[index] is IInteractableObject interactable)
        {
            interactable.ObjectInteraction();
        }
        else
        {
            Debug.LogError("Invalid item index or type.");
        }
    }
}
