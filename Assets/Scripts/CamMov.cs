using UnityEngine;

public class CamMov : MonoBehaviour
{
    public Transform car;
    public float sensitivity = 5;        // How fast camera is going to turn

    public float maxAngle = 50;

    private float yaw;      // Horizontal rotation

    private InputSystem_Actions controls;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void LateUpdate()
    {
        // Follow car position
        transform.position = car.position;
        //transform.position = Vector3.Lerp(
        //transform.position,
        //car.position,
        //8f * Time.deltaTime
        //);

        // Mouse input
        Vector2 mouse = controls.Player.Look.ReadValue<Vector2>();

        // Update yaw (horizontal rotation) from mouse
        yaw += mouse.x * sensitivity * Time.deltaTime;

        // Get car's current Y rotation (direction)
        float carYaw = car.eulerAngles.y;

        // Convert difference to -180 to 180 range
        // How far the camera rotated compared to the car
        float relativeYaw = Mathf.DeltaAngle(carYaw, yaw);

        // Clamp relative angle
        // Limit the camera rotation among the X axis
        relativeYaw = Mathf.Clamp(relativeYaw, -maxAngle, maxAngle);

        // Recalculate final horizontal rotation
        yaw = carYaw + relativeYaw;

        // Apply rotation (only Y axis)
        // Y axis moves left/right (creo q es algo del plano 2d porq el mouse se mueve en plano 2d)
        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }
}
