using UnityEngine;

public class Thorn : MonoBehaviour
{
    int damageAmount = 30;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(collision.gameObject.TryGetComponent<PlayerHealthManager>(out var playerHealth))
            if(playerHealth != null)
            {
                // Randomly choose left or right direction and normalize it
                Vector2 direction = (Random.value < 0.5f) ? Vector2.left : Vector2.right;

                    // Call the method with damage and direction
                playerHealth.TakeDamage(damageAmount, direction.normalized);
            }
        }      
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(collision.gameObject.TryGetComponent<PlayerHealthManager>(out var playerHealth))
            if(playerHealth != null)
            {
                // Randomly choose left or right direction and normalize it
                Vector2 direction = (Random.value < 0.5f) ? Vector2.left : Vector2.right;

                    // Call the method with damage and direction
                playerHealth.TakeDamage(damageAmount, Vector2.zero);
            }
        }      
    }
}
