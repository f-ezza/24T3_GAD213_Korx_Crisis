using UnityEngine;

public class ItemDefinition : ScriptableObject, IInteractableObject
{
    [Header("Item Information")]
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    [SerializeField] private Sprite icon;

    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public Sprite Icon => icon;

    public virtual void ObjectInitalisation()
    {

    }

    public virtual void ObjectInteraction()
    {

    }
}