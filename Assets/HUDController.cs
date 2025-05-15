using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
[SerializeField] private Image healthBarFillImage; // Assign your HealthBar image here

    [SerializeField] private TextMeshProUGUI healthText;
    private PlayerHealthManager playerHealth;

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
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int current, int max)
    {
        float fill = (float)current / max;
        healthBarFillImage.fillAmount = fill;
        healthText.text = $"{current} / {max}";
        // You could also add tweening here for smooth bar transitions.
    }
}
