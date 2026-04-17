using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, 2f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null) 
        {
            enemy.TakeDamage(damage); 
        }
        Destroy(gameObject);
    }
}
