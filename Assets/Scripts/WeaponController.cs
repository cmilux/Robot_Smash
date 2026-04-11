using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform cameraTransform;

    void Update()
    {
        Vector3 rotation = cameraTransform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }
}