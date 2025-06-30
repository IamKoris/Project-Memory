using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class StatManager : UdonSharpBehaviour
{
    [Header("Player Stats")]
    public int strength = 0;
    public int defense = 0;
    public int agility = 0;
    public int detection = 0;
    public int dexterity = 0;
    public int vitality = 0;

    [Header("Stat Points")]
    public int unspentPoints = 10;
    public MenuController menuController;

    // -------- HEALTH --------
    [Header("Health")]
    public float baseHealth = 175f;
    public float healthPerVitality = 25f;
    public float currentHealth = 175f;

    [Header("Health Regen")]
    public bool enableHealthRegen = true;
    public float baseRegenRate = 1f;
    public float regenPerVitality = 0.2f;

    [Header("Health UI")]
    public Text healthText;
    public Image healthbarForeground;

    // -------- STAMINA --------
    [Header("Stamina")]
    public float baseStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRegenRate = 5f;
    public float staminaPerAgility = 10f;
    public float staminaCostReductionPerLevel = 0.05f;

    [Header("Stamina UI")]
    public Image staminaBarForeground;

    void Start()
    {
        currentHealth = GetMaxHealth();
        currentStamina = GetMaxStamina();
        UpdateHealthUI();
        UpdateStaminaUI();
    }

    void Update()
    {
        RegenerateStamina();
        RegenerateHealth();
        UpdateHealthUI();
    }

    // --------- STAT MODIFIER ---------
    public void ModifyStat(string statName, int amount)
    {
        if (amount > 0 && unspentPoints <= 0) return;

        switch (statName)
        {
            case "vit":
                if (amount > 0) vitality++;
                else if (vitality > 0) vitality--;
                break;
            case "str":
                if (amount > 0) strength++;
                else if (strength > 0) strength--;
                break;
            case "def":
                if (amount > 0) defense++;
                else if (defense > 0) defense--;
                break;
            case "agi":
                if (amount > 0) agility++;
                else if (agility > 0) agility--;
                break;
            case "stl":
                if (amount > 0) detection++;
                else if (detection > 0) detection--;
                break;
            case "dex":
                if (amount > 0) dexterity++;
                else if (dexterity > 0) dexterity--;
                break;
        }

        if (amount > 0) unspentPoints--;
        else unspentPoints++;

        if (menuController != null)
            menuController.UpdateMenuStats();
    }

    // -------- HEALTH --------
    public float GetMaxHealth()
    {
        return baseHealth + (vitality * healthPerVitality);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0f) currentHealth = 0f;
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        float maxHealth = GetMaxHealth();
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void RegenerateHealth()
    {
        if (!enableHealthRegen) return;
        float maxHealth = GetMaxHealth();
        if (currentHealth < maxHealth)
        {
            float regenAmount = baseRegenRate + (vitality * regenPerVitality);
            currentHealth += regenAmount * Time.deltaTime;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            UpdateHealthUI();
        }
    }

    public void UpdateHealthUI()
    {
        float maxHealth = GetMaxHealth();
        if (healthText != null)
        {
            healthText.text = "HP: " + Mathf.RoundToInt(currentHealth) + " / " + Mathf.RoundToInt(maxHealth);
        }
        if (healthbarForeground != null)
        {
            float fill = currentHealth / maxHealth;
            healthbarForeground.fillAmount = fill;
        }
    }

    // -------- STAMINA --------
    public float GetMaxStamina()
    {
        return baseStamina + (agility * staminaPerAgility);
    }

    public void RegenerateStamina()
    {
        float maxStamina = GetMaxStamina();
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            UpdateStaminaUI();
        }
    }

    public bool TryUseStamina(float baseCost)
    {
        float cost = baseCost * (1f - (agility * staminaCostReductionPerLevel));
        if (currentStamina >= cost)
        {
            currentStamina -= cost;
            UpdateStaminaUI();
            return true;
        }
        return false;
    }

    public void UpdateStaminaUI()
    {
        if (staminaBarForeground != null)
        {
            float fill = currentStamina / GetMaxStamina();
            staminaBarForeground.fillAmount = fill;
        }
    }
}
