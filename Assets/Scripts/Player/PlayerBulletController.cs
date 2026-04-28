using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private Rigidbody rb;
    PlayerLevelUI pj;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;

        pj = GameObject.FindAnyObjectByType<PlayerLevelUI>();

        Destroy(gameObject, 2f);    //Destroy bullet
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
                //pj.AddExp(30);
                Debug.Log("Adding EXP to: " + pj.gameObject.name);
            }

            //Destroy bullet
            Destroy(gameObject);
        }
            
    }
}
