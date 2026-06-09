using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class CarController : NetworkBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 100f;

    // Dash settings (a fast and short speed boost)
    public float dashSpeed = 50f;
    public float dashDuration = 0.5f;
    public bool isDashing = false;

    // If true the car cannot move
    public bool isFrozen = false;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float currentSpeed;

    private float flippedRotation = 90f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        currentSpeed = speed;

        Cursor.visible = false;

        Cursor.lockState = CursorLockMode.Locked;
    }
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // If inventory its open frozen would be true
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

        FlipCar();      //flip car
    }
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void ActivateDash()
    {
        //Prevent starting a new DashRoutine() if one is already in progress
        if (!isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    // A timer function that controls how long the dash lasts
    private IEnumerator DashRoutine()
    {
        isDashing = true;

        // Save the normal speed before changing it
        float originalSpeed = speed;
        speed = dashSpeed;

        // Wait for a short time (dashDuration)
        yield return new WaitForSeconds(dashDuration);

        // Return to normal speed after waiting
        speed = originalSpeed;
        isDashing = false;
    }

    void FlipCar()
    {
        //flip car to normal if player finds itself flipped upside down or sideways
        if (gameObject.transform.rotation.x > flippedRotation || gameObject.transform.rotation.x > -flippedRotation
            || gameObject.transform.rotation.y > flippedRotation || gameObject.transform.rotation.y > -flippedRotation
            || gameObject.transform.rotation.z > flippedRotation || gameObject.transform.rotation.z > flippedRotation)
        {
            gameObject.transform.Rotate(0f, 0f, 0f);
        }
    }
}

