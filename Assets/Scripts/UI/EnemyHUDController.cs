using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHUDController : MonoBehaviour
{
    [SerializeField] private Image healthBarFillImage; // Assign your HealthBar image here
    private Vector3 originalScale;

    public SkeletonEnemyHealthManager skeletonEnemyHealthManager;

    private void Start()
    {
        // Store initial scale
        originalScale = transform.localScale;
        if (skeletonEnemyHealthManager == null)
        {
            Debug.LogError("No PlayerHealthManager in scene!");
            enabled = false;
            return;
        }
        skeletonEnemyHealthManager.OnHealthChanged += UpdateHealthUI;
        UpdateHealthUI(skeletonEnemyHealthManager.CurrentHealth, skeletonEnemyHealthManager.MaxHealth);
    }

    private void OnDestroy()
    {
        if (skeletonEnemyHealthManager != null)
            skeletonEnemyHealthManager.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int current, int max)
    {
        // Prevent divide-by-zero
    if (max <= 0) max = 1;

    // Clamp currentHealth to [0, max]
    current = Mathf.Clamp(current, 0, max);

    float fill = (float)current / max;
    healthBarFillImage.fillAmount = Mathf.Clamp01(fill);
    }
    public void MatchParentDirection(float directionSign)
    {
        // Only flip X axis while preserving original scale
        transform.localScale = new Vector3(
            originalScale.x * directionSign,
            originalScale.y,
            originalScale.z
        );
    }
}
