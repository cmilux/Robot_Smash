using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
public class CarController : NetworkBehaviour
{
    [Header("Movement")]
    private Rigidbody _rb;
    private Vector2 _moveInput;
    public float speed = 10f;
    private float _currentSpeed;
    public float turnSpeed = 100f;
    public float acceleration = 8f;     //how fast it reaches target speed
    public bool isFrozen = false;       //If true the car cannot move

    [Header("Dash")]
    private CarBumper _carBumper;
    public float dashSpeed = 50f;
    public float dashDuration = 0.5f;
    public float dashCooldownBackup = 3f;   //use only if the bumper has no ItemData assigned
    public bool isDashing = false;
    private float _nextDashTime;

    [Header("Animator")]
    [SerializeField] Animator _animator;
    private NetworkVariable<float> _netSpeed = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _carBumper = GetComponent<CarBumper>();

        if(_animator == null) _animator = GetComponentInChildren<Animator>();

        _currentSpeed = speed;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isFrozen) return;       // If inventory its open frozen would be true

        HandleMov();
        HandleRotation();
        //FlipCar();
        UpdateAnimationSpeed();
    }

    private void Update()
    {
        // Applies to owner AND remote clients — reads whatever networkSpeed currently holds
        // aplica al dueño Y a los clientes remotos — lee lo que tenga networkSpeed en ese momento
        if (_animator != null)
        {
            _animator.SetFloat("Speed", _netSpeed.Value);
        }
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    void HandleMov()
    {
        float moveAmount = _moveInput.y * speed;

        if (isDashing)
        {
            moveAmount = speed; // force forward movement during dash (si no apretas la w igual acelera)
        }

        // Set the target velocity in the car forward direction
        Vector3 targetVelocity = transform.forward * moveAmount;
        // Keep the current vertical velocity
        targetVelocity.y = _rb.linearVelocity.y;

        // move smoothly towards the target velocity
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }

    void HandleRotation()
    {
        float turn = _moveInput.x * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        _rb.MoveRotation(_rb.rotation * turnRotation);
    }

    void UpdateAnimationSpeed()
    {
        float fowardVelocity = Vector3.Dot(_rb.linearVelocity, transform.forward);
        float normalizeSpeed = Mathf.Clamp(fowardVelocity / speed, -1f, 1f);

        _netSpeed.Value = normalizeSpeed;
    }

    public void ActivateDash()
    {  
        //no se puede dashear sin paragolpe 
        if (_carBumper == null || !_carBumper.isEquipped) return;
        if (Time.time < _nextDashTime) return;

        //Prevent starting a new DashRoutine() if one is already in progress
        if (!isDashing)
        {
            _carBumper.UseDurability();

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

        float cooldown = _carBumper.GetDashCooldown(dashCooldownBackup);
        _nextDashTime = Time.time + cooldown;
        _carBumper.SetNextDashReadyTime(_nextDashTime);   
    }

    void FlipCar()
    {
        Vector3 euler = transform.eulerAngles;
        float x = euler.x > 180 ? euler.x - 360 : euler.x;
        float z = euler.z > 180 ? euler.z - 360 : euler.z;
        if (Mathf.Abs(x) > 60f || Mathf.Abs(z) > 60f)
        {
            _rb.MoveRotation(Quaternion.Euler(0f, euler.y, 0f));
        }
    }
}

