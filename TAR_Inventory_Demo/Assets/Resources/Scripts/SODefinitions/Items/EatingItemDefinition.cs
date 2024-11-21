using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TAR/Item/Eating Item")]
public class EatingItemDefinition : ItemDefinition
{
    [Header("Eating Item Specific")]
    [SerializeField] private int hungerRestore;

    public override void ObjectInteraction()
    {
        Debug.Log($"{ItemName} used. Restores {hungerRestore} hunger.");
    }
}
