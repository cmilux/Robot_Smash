using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private Rigidbody rb;
    PlayerLevel pj;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, 2f);

        pj = GameObject.FindGameObjectWithTag("ExpUI").GetComponent<PlayerLevel>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null) 
        {
            enemy.TakeDamage(damage); 

            if (enemy.isDead == true)
            {
                pj.AddExp(30);
            }
        }
        Destroy(gameObject);
    }
}
