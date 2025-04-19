using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveRight : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;

    void Update()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.right * speed;
    }
}
