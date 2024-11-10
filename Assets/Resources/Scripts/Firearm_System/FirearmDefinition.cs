using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Korx.Firearms
{
    [CreateAssetMenu(menuName = "Korx Crisis/Firearm/Create New Firearm")]
    public class FirearmDefinition : ScriptableObject
    {
        public enum FireModes { Semi, Burst, Auto, Bolt}

        [Header("Firearm Information")]
        [Tooltip("The firearms visible name")] public string firearmName;
        [Tooltip("The firearms identifier/short name")] public string firearmIDName;
        [Tooltip("The firearms description")] public string firearmDescription;
        [Tooltip("The firearms category e.g. AR or SMG")] public string firearmType;

        [Header("Firearm Stats")]
        [Tooltip("The amount of ammunition a singulare magazine can hold")] public int firearmFullMag;
        [Tooltip("The fire rate of the weapon (in rpm - rounds per minute)")] public int firearmfirerate;
        [Tooltip("The damage each shot does")] public float firearmDamage;
        [Tooltip("The multiplier of damage for a headshot")] public float firearmHSMultiplier;
        [Tooltip("The reload time for the firearm")] public float firearmReloadTime;
        [Tooltip("The weight of the firearm (in kg)")] public float firearmWeight;
        [Tooltip("The time to aim in / aim out")] public float firearmAimSpeed;
        [Tooltip("The weapons range")] public float firearmRange;
        [Tooltip("The amount of shots when swapped to burst")] public int firearmBurstCount;
        [Tooltip("The firemodes available on the firearm")] public FireModes[] firearmFireModes;

        [Header("Firearm Audio")]
        [Tooltip("The firearms fire sound")] public AudioClip[] firearmShootSound;
        [Tooltip("The firearms fire empty sound")] public AudioClip firearmShootEmptySound;
        [Tooltip("The firearms reload sound")] public AudioClip firearmReloadSound;
        [Tooltip("The firearms reload empty sound")] public AudioClip firearmReloadEmptySound;
        [Tooltip("The firearms aim sound")] public AudioClip firearmAimSound;
        [Tooltip("The casing ejection sound")] public AudioClip[] firearmEjectSounds;
        [Tooltip("The fireselector sounds")] public AudioClip[] firearmFireSelectSounds;

        [Header("Firearm Model/Sprite")]
        [Tooltip("The Firearms model prefab")] public GameObject firearmPrefab;
        [Tooltip("The firearms casing prefab")] public GameObject firearmCasingPrefab;
        [Tooltip("The Firearms sprite")] public Sprite firearmSprite;
    }
}
