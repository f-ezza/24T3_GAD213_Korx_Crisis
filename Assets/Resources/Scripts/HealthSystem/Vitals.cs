using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Vitals : MonoBehaviour, IDamagableObject
{
    [Header("Start Values")]
    [SerializeField] private float playerStartHealth;
    [SerializeField] private float playerStartArmor;

    [Header("Health")]
    [SerializeField] private float currentPlayerHealth;
    [SerializeField] private float maxPlayerHealth;
    [SerializeField] private float minPlayerHealth;

    [Header("Armor")]
    [SerializeField] private float currentPlayerArmor;
    [SerializeField] private float maxPlayerArmor;
    [SerializeField] private float minPlayerArmor;

    [Header("Debug Options")]
    [SerializeField] private bool canHealthRegen;
    [SerializeField] private bool canArmorRegen;
    [SerializeField] private bool armorTakesAll;
    [SerializeField] private float armorMitigation;
    [SerializeField] private float healthRegenRate;
    [SerializeField] private float timeBeforeHealthRegen;
    [SerializeField] private float armorRegenRate;
    [SerializeField] private float timeBeforeArmorRegen;
    [SerializeField] private float currentHealthRegenTimer;
    [SerializeField] private float currentArmorRegenTimer;

    [Header("External Components")]
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        DamagableObjectInitialisation();
    }

    private void Update()
    {
        HandleHealthRegeneration();
        HandleArmorRegeneration();
    }

    private void HandleHealthRegeneration()
    {
        if (!canHealthRegen || currentPlayerHealth >= maxPlayerHealth) return;

        if (currentHealthRegenTimer < timeBeforeHealthRegen)
        {
            currentHealthRegenTimer += Time.deltaTime;
        }

        if (currentHealthRegenTimer >= timeBeforeHealthRegen)
        {
            currentPlayerHealth += healthRegenRate * Time.deltaTime;
            if (currentPlayerHealth > maxPlayerHealth) currentPlayerHealth = maxPlayerHealth;

            uiManager.HandleHealthBar(currentPlayerHealth, currentPlayerArmor);
        }
    }

    private void HandleArmorRegeneration()
    {
        if (!canArmorRegen || currentPlayerArmor >= maxPlayerArmor) return;

        if (currentArmorRegenTimer < timeBeforeArmorRegen)
        {
            currentArmorRegenTimer += Time.deltaTime;
        }

        if (currentArmorRegenTimer >= timeBeforeArmorRegen)
        {
            currentPlayerArmor += armorRegenRate * Time.deltaTime;
            if (currentPlayerArmor > maxPlayerArmor) currentPlayerArmor = maxPlayerArmor;

            uiManager.HandleHealthBar(currentPlayerHealth, currentPlayerArmor);
        }
    }

    public void DamagableObjectInitialisation()
    {
        currentPlayerHealth = playerStartHealth;
        currentPlayerArmor = playerStartArmor;
        uiManager.HandleHealthBar(currentPlayerHealth, currentPlayerArmor);
    }

    public void OnDeath()
    {
        Debug.Log($"Player Died with these stats: \nHealth: {currentPlayerHealth} \nArmor: {currentPlayerArmor}");
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void OnReceivedDamage(float damageRecieved)
    {
        currentArmorRegenTimer = 0f;
        currentHealthRegenTimer = 0f;

        if (currentPlayerArmor > 0f)
        {
            currentPlayerArmor -= damageRecieved;
            if(currentPlayerArmor < minPlayerArmor) { currentPlayerArmor = minPlayerArmor; }
            if (!armorTakesAll)
            {
                currentPlayerHealth -= damageRecieved * armorMitigation;
            }
        }
        else
        {
            currentPlayerHealth -= damageRecieved;
        }

        if (currentPlayerHealth <= minPlayerHealth)
        {
            OnDeath();
        }
        else
        {
            uiManager.HandleHealthBar(currentPlayerHealth, currentPlayerArmor);
        }
    }
}
