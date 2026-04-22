using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackMelee : MonoBehaviour
{
    private CarController carController;

    public int damageAmount = 1;

    [SerializeField] PlayerLevelUI pj;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    public void OnDash(InputValue value)
    {   //Shift Key
        if (value.isPressed)
        {
            carController.ActivateDash();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (carController.isDashing)
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);

                    if (enemy.isDead == true)
                    {
                        pj.AddExp(15);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
            }
        }
    }
}
