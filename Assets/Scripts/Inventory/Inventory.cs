using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : NetworkBehaviour
{
    [Header("Configuración para Recoger")]
    public float pickupRadius = 7f;

    public LayerMask itemLayer;

    private void Start()
    {

    }

    private void OnInteract(InputValue value)
    {
        // Solo el owner puede usar su inventario
        if (!IsOwner) return;

        // Se presionó la tecla interact
        if (value.isPressed)
        {

            // Buscar items cerca
            Collider[] colliders = Physics.OverlapSphere(
                transform.position,
                pickupRadius,
                itemLayer
            );
            foreach (Collider col in colliders)
            {
                // Intentar obtener ItemPickup
                ItemPickup itemOnGround =
                    col.GetComponent<ItemPickup>();

                if (itemOnGround != null)
                {
                    itemOnGround.Pickup();

                    break;
                }
            }
        }
    }

    // TEMPORAL
    // falta agregar el inventario real
    public int AddItem(ItemData itemData, int quantity)
    {
        return 0;
    }
}