using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Image healthBarFillImage; // Assign your HealthBar image here
    [SerializeField] private Image powerBarFillImage;


    [SerializeField] private TextMeshProUGUI healthText;
    private PlayerHealthManager playerHealth;
    private PlayerAttack playerAttack;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealthManager>();
        if (playerHealth == null)
        {
            Debug.LogError("No PlayerHealthManager in scene!");
            enabled = false;
            return;
        }
        playerHealth.OnHealthChanged += UpdateHealthUI;
        UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        var playerAttack = FindFirstObjectByType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.OnFatigueChanged += UpdatePowerBar;
            UpdatePowerBar(playerAttack.CurrentFatigue, playerAttack.MaxFatigue);
        }
        else
        {
            Debug.LogWarning("No PlayerAttack found in scene.");
        }

    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
            if (playerAttack != null)
    playerAttack.OnFatigueChanged -= UpdatePowerBar;
    }

    private void UpdateHealthUI(int current, int max)
    {
        float fill = (float)current / max;
        healthBarFillImage.fillAmount = fill;
        healthText.text = $"{current} / {max}";
        // You could also add tweening here for smooth bar transitions.
    }
    private void UpdatePowerBar(int current, int max)
{
    float fill = 1f - ((float)current / max);  // Invert since more fatigue = less power
    powerBarFillImage.fillAmount = fill;
    // Optional: Add color lerping, or flash effect on depletion
}
}
