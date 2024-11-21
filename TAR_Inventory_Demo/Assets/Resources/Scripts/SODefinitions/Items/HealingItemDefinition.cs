using UnityEngine;

[CreateAssetMenu(menuName = "TAR/Item/Healing Item")]
public class HealingItem : ItemDefinition
{
    [Header("Healing Item Specific")]
    [SerializeField] private int healAmount;

    public override void ObjectInteraction()
    {
        Debug.Log($"{ItemName} used. Restores {healAmount} HP.");
    }
}