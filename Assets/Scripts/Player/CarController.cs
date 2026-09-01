using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class CarController : NetworkBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 100f;

    // Dash settings
    public float dashSpeed = 50f;
    public float dashDuration = 0.5f;
    public bool isDashing = false;
    public float acceleration = 8f; // how fast it reaches target speed

    // If true the car cannot move
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
    //TO DO: fix car climbing hills in little jumps
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // If inventory its open frozen would be true
        if (isFrozen) return;

        float moveAmount = moveInput.y * speed;

        if (isDashing)
        {
            moveAmount = speed; // force forward movement during dash (si no apretas la w igual acelera)
        }

        // Set the target velocity in the car forward direction
        Vector3 targetVelocity = transform.forward * moveAmount;
        // Keep the current vertical velocity
        targetVelocity.y = rb.linearVelocity.y;

        // move smoothly towards the target velocity
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // Rotation
        float turn = moveInput.x * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        //FlipCar();      //flip car
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
        Vector3 euler = transform.eulerAngles;
        float x = euler.x > 180 ? euler.x - 360 : euler.x;
        float z = euler.z > 180 ? euler.z - 360 : euler.z;
        if (Mathf.Abs(x) > 60f || Mathf.Abs(z) > 60f)
        {
            rb.MoveRotation(Quaternion.Euler(0f, euler.y, 0f));
        }
    }
}

