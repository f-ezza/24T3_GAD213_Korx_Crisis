using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Korx Crisis/Firearms/Damageable Objects")]
public class DamagableObjectDefinition : ScriptableObject
{
    [Header("Object Information")]
    public string objectName;
    public string objectDescription;
    public float health;
    public float armor;
}
