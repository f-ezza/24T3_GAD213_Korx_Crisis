using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagableObject
{
    void DamagableObjectInitialisation();
    void OnReceivedDamage(float damageRecieved);
    void OnDeath();
}
