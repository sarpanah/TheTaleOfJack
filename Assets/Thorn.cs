using UnityEngine;

public class Thorn : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if(collision.gameObject.TryGetComponent<PlayerHealthManager>(out var playerHealth))
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(30);    
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
                playerHealth.TakeDamage(30);    
            }
        }      
    }
}
