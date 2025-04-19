using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Ensure it's the player collecting the coin
        {
            // Call the CoinManager to update the coin count
            GameManager.Instance.AddCoin();

            // Destroy the coin after it's collected
            Destroy(gameObject);
        }
    }
}
