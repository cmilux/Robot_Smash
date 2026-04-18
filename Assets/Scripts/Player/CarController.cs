using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class CarController : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 100f;

    public float dashSpeed = 50f;
    public float dashDuration = 0.5f;
    public bool isDashing = false;

    public bool isFrozen = false;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float currentSpeed;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        currentSpeed = speed;

        Cursor.visible = false;

        Cursor.lockState = CursorLockMode.Locked;
    }
    void FixedUpdate()
    {   // Si el inventario está abierto, isFrozen será true y el coche no se moverá
        if (isFrozen) return;
        // Movimiento hacia adelante/atrás
        float move = moveInput.y * speed * Time.fixedDeltaTime;

        // Force forward movement during dash even if there is no player input
        if (isDashing)
        {
            move = 1 * speed * Time.fixedDeltaTime;
        }
        Vector3 movement = transform.forward * move;

        rb.MovePosition(rb.position + movement);

        // Rotación
        float turn = moveInput.x * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        rb.MoveRotation(rb.rotation * turnRotation);
    }
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void ActivateDash()
    {   //Prevent starting a new DashRoutine() if one is already in progress
        if (!isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        float originalSpeed = speed;
        speed = dashSpeed;
        
        yield return new WaitForSeconds(dashDuration);

        speed = originalSpeed;
        isDashing = false;
    }
}

