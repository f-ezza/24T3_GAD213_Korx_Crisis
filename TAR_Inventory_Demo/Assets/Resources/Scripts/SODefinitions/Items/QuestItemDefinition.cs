using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TAR/Item/Quest Item")]
public class QuestItemDefinition : ItemDefinition
{
    [Header("Quest Item Specific")]
    [SerializeField] private string questName;

    public override void ObjectInteraction()
    {
        Debug.Log($"{ItemName} is part of the quest: {questName}.");
    }
}
