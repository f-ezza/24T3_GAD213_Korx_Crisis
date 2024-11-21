using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableObject
{
    string ItemName { get; }
    string ItemDescription { get; }
    Sprite Icon { get; }


    void ObjectInitalisation();
    void ObjectInteraction();
}
