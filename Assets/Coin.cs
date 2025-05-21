using UnityEngine;

public class Coin : MonoBehaviour
{

    public float throwForce = 5f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player") // Ensure it's the player collecting the coin
        {
            // Call the CoinManager to update the coin count
            GameManager.Instance.AddCoin();
            AndroidHapticManager.Instance.Vibrate(VibrationIntensity.VeryLight);
            // Destroy the coin after it's collected
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player") // Ensure it's the player collecting the coin
        {
            // Call the CoinManager to update the coin count
            GameManager.Instance.AddCoin();
            AndroidHapticManager.Instance.Vibrate(VibrationIntensity.VeryLight);
            // Destroy the coin after it's collected
            Destroy(gameObject);
        }
    }

    public void Throw(Vector2 direction)
{
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero; // clear existing velocity if any
        rb.AddForce((direction.normalized + Vector2.up * 0.5f) * throwForce, ForceMode2D.Impulse);
    }
}
}
