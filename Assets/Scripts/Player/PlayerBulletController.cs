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

        Destroy(gameObject, 2f);    //Destroy bullet

        pj = GameObject.FindGameObjectWithTag("ExpUI").GetComponent<PlayerLevel>();         //Gets player level script (esta en el canvas ups)
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Get enemy script
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null) 
        {
            enemy.TakeDamage(damage);           //Apply damage to enemy

            if (enemy.isDead == true)
            {
                //Add this amount of experience to player if enemy died
                pj.AddExp(30);          
            }
        }

        //Destroy bullet
        Destroy(gameObject);                
    }
}
