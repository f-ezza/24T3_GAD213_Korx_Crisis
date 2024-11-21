using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TAR/Item/Equippable Item")]
public class EquippableItemDefinition : ItemDefinition
{
    [Header("Equippable Item Specific")]
    [SerializeField] private string equipmentSlot;

    public override void ObjectInteraction()
    {
        Debug.Log($"{ItemName} equipped in {equipmentSlot} slot.");
    }
}
