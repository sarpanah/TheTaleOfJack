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
    [SerializeField] private GameObject fatigueMessage;  // Reference to your fatigue UI GameObject
    private float messageDisplayDuration = 2f;  // How long the message stays visible
    private float messageHideTime;              // When to hide the message
     private bool isFatigued; // New field to track fatigue state
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
            playerAttack.OnFatigueAttackAttempt += ShowFatigueMessage;  // Subscribe to new event
            UpdatePowerBar(playerAttack.CurrentFatigue, playerAttack.MaxFatigue);
        }
        else
        {
            Debug.LogWarning("No PlayerAttack found in scene.");
        }

    }
private void Update()
    {
        if (fatigueMessage.activeSelf && Time.time >= messageHideTime || !isFatigued)
        {
            fatigueMessage.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
        if (playerAttack != null)
            playerAttack.OnFatigueChanged -= UpdatePowerBar;
    playerAttack.OnFatigueAttackAttempt -= ShowFatigueMessage;  // Unsubscribe  
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
        isFatigued = (current >= 3);
    }
private void ShowFatigueMessage()
    {
        if (!fatigueMessage.activeSelf)
        {
            fatigueMessage.SetActive(true);
        }
        messageHideTime = Time.time + messageDisplayDuration;  // Reset/extend display time
    }

    private void HideFatigueMessage()
    {
        fatigueMessage.SetActive(false);
    }
}
