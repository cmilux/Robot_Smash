using System.Collections;
using System.Security.Cryptography;
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

    [Header("Ground stick")]
    [SerializeField] LayerMask _groundMask;     //only terrain/ground layers, not the players
    [SerializeField] float _rayHeight = 0.5f;   //how far above the pivot the ray starts
    [SerializeField] float _snapDistance = 1.0f;    //max gap to the ground to still count as grounded
    [SerializeField] float _stickForce = 20f;   //downward push while grounded
    private bool _isGrounded;
    private Vector3 _groundNormal = Vector3.up;

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
        if (_rb.isKinematic) return;

        CheckGround();
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

        if (IsOwner && _moveInput.sqrMagnitude > 0.1f)
        {
            // tutorial step to teach player how to move
            TutorialEvents.Raise(TutorialStep.Move);
        }
    }

    void HandleMov()
    {
        float moveAmount = _moveInput.y * speed;

        if (isDashing)
        {
            moveAmount = speed; // force forward movement during dash (si no apretas la w igual acelera)
        }

        Vector3 targetVelocity;

        if (_isGrounded)
        {
            //follow the slope: move along the ground plane, not straight ahead
            Vector3 slopeDir = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;
            targetVelocity = slopeDir * moveAmount;
        }
        else
        {
            //in the aire gravity decides the vertical speed
            targetVelocity = transform.forward * moveAmount;
            targetVelocity.y = _rb.linearVelocity.y;
        }

        //move smoothly towards the target velocity
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        //small push to the ground so it doesnt take off on crests or downhill
        if (_isGrounded) _rb.AddForce(-_groundNormal * _stickForce, ForceMode.Acceleration);

       /* // Set the target velocity in the car forward direction
        Vector3 targetVelocity = transform.forward * moveAmount;
        // Keep the current vertical velocity
        targetVelocity.y = _rb.linearVelocity.y;

        // move smoothly towards the target velocity
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);*/
    }

    void HandleRotation()
    {
        float turn = _moveInput.x * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        _rb.MoveRotation(_rb.rotation * turnRotation);
    }

    //clear stuck input after the player input was disabled || limpia input trabado tras desabilitar el player input
    public void ResetInput() => _moveInput = Vector2.zero;

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

    //cast a ray down to know if theres ground and how tilted it is
    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * _rayHeight;

        _isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            _rayHeight + _snapDistance,
            _groundMask,
            QueryTriggerInteraction.Ignore);

        _groundNormal = _isGrounded ? hit.normal : Vector3.up;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * _rayHeight;
        Gizmos.DrawLine(origin, origin + Vector3.down * (_rayHeight + _snapDistance));
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

