using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float criticalSizeMultiplier = 1.2f; // New size multiplier

    private float elapsed;
    private Color startColor;
    private float originalFontSize; // Store original font size

    public void SetDamage(int amount)
    {
        text.text = amount.ToString();
        originalFontSize = text.fontSize; // Capture initial size

        if (amount > 40)
        {
            startColor = Color.red;
            text.fontSize = originalFontSize * criticalSizeMultiplier;
        }
        else
        {
            startColor = text.color;
        }

        text.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        elapsed += Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (elapsed >= fadeDuration)
            Destroy(gameObject);
    }
}