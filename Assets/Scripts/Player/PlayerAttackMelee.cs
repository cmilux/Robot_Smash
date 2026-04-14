using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackMelee : MonoBehaviour
{   
    private CarController carController;

    public int damageAmount = 1;

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
                EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);
                }
            }
        }
    }
}
