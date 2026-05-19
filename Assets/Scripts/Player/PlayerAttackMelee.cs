using Unity.Netcode;
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

    private void Start()
    {
       // pj = GameObject.FindAnyObjectByType<PlayerLevelUI>();
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
                    enemy.TakeDamageServerRpc(damageAmount, NetworkManager.Singleton.LocalClientId);

                    if (enemy.isDead.Value == true)
                    {
                        //pj.AddExp(15);
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
                enemy.TakeDamageServerRpc(damageAmount, NetworkManager.Singleton.LocalClientId);
            }
        }
    }
}
