using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackMelee : NetworkBehaviour
{
    private CarController carController;

    public int damageAmount = 20;

    [SerializeField] PlayerLevelUI pj;

    // The network ID of the player who is attacking
    ulong shooterClientId;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    public override void OnNetworkSpawn()
    {
        // Save the ID of the local player who owns this car
        shooterClientId = OwnerClientId;
    }

    // Automatically called by the Input System when pressing Shift Key
    public void OnDash(InputValue value)
    {   
        if (value.isPressed)
        {
            // Tell the car to start the speed boost
            carController.ActivateDash();
        }
    }

    // When the physical body of the car crashes into another solid object
    private void OnCollisionEnter(Collision collision)
    {   
        if (!IsOwner) return;

        // Check if the object we hit is an Enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Only deal damage if the car is currently dashing 
            if (carController.isDashing)
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();

                if (enemy != null)
                {
                    // Ask the server to reduce health from the enemy
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
