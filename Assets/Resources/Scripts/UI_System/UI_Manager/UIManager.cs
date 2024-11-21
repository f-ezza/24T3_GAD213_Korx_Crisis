using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Objects - Healthbar")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider armorSlider;

    [Header("UI Objects - Compass")]
    [SerializeField] private RectTransform compassGameobject;

    [Header("UI Objects - Firearm")]
    [SerializeField] private Sprite firearmWeaponIcon;
    [SerializeField] private TMP_Text firearmWeaponLabel;
    [SerializeField] private TMP_Text firearmAmmoLabel;
    [SerializeField] private TMP_Text firearmFiremodeLabel;

    [Header("UI Objects - Objective")]
    [SerializeField] private GameObject objectiveUI;

    [Header("Player Reference")]
    [SerializeField] private Transform playerOrientation;

    private void Update()
    {
        HandleCompass();
    }

    public void HandleFirearmUI(string firearmName, string firemodeName, string ammoCount, Sprite firearmIcon)
    {
        firearmWeaponIcon = firearmIcon;
        firearmWeaponLabel.text = firearmName;
        firearmAmmoLabel.text = ammoCount;
        firearmFiremodeLabel.text = firemodeName;
    }

    public void HandleHealthBar(float newHealth, float newArmor)
    {
        healthSlider.value = newHealth / 100f;
        armorSlider.value = newArmor / 100f;
    }

    public void HandleCompass()
    {
        float playerAngle = Mathf.Atan2(playerOrientation.forward.x, playerOrientation.forward.z) * Mathf.Rad2Deg;
        Debug.Log("Player Rotation: " + playerAngle); 
        compassGameobject.rotation = Quaternion.Euler(0f, 0f, -playerAngle);
    }


}
