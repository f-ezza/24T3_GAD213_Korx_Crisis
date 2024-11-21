using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : MonoBehaviour, IDamagableObject
{
    [Header("Damagable Object Definition")]
    [SerializeField] private DamagableObjectDefinition damagableObjectDefinition;

    [Header("Damagable Object Information")]
    [SerializeField] private string objectName;
    [SerializeField] private string objectDescription;
    [SerializeField] private float health;
    [SerializeField] private float armor;

    private void Awake()
    {
        DamagableObjectInitialisation();
    }

    public void DamagableObjectInitialisation()
    {
        if (damagableObjectDefinition != null) 
        {
            objectName = damagableObjectDefinition.objectName;
            objectDescription = damagableObjectDefinition.objectDescription;
            health = damagableObjectDefinition.health;
            armor = damagableObjectDefinition.armor;
        }
        else
        {
            Debug.LogError("No definition file provided for this object");
        }
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }

    public void OnReceivedDamage(float damageRecieved)
    {
        if (armor > 0)
        {
            armor -= damageRecieved;
            health -= (damageRecieved * 0.25f);
        }
        else
        {
            health -= damageRecieved;
        }

        if(health <= 0)
        {
            OnDeath();
        }

        Debug.Log(objectName + " has recieved damage worth " + damageRecieved + ". The object now has: " +  health + " health & " + armor + " armor");
    }
}
