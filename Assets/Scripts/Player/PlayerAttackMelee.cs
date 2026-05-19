using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackMelee : NetworkBehaviour
{
    private CarController carController;

    public int damageAmount = 1;

    [SerializeField] PlayerLevelUI pj;

    ulong shooterClientId;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    public override void OnNetworkSpawn()
    {
        shooterClientId = OwnerClientId;
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
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (carController.isDashing)
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();

                if (enemy != null)
                {
                    enemy.TakeDamageServerRpc(damageAmount, shooterClientId);

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
        if (!IsOwner) return;

        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamageServerRpc(damageAmount, shooterClientId);
            }
        }
    }
}
