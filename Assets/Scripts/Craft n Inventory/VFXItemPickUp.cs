using System;
using UnityEngine;

public class VFXItemPickUp : MonoBehaviour
{
    float rotateSpeed = 50f;
    [SerializeField] float amplitude = 0.5f;
    [SerializeField] float frequency = 0.01f;

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        //si no queremos q flote borrar esto
        //si queremos q flote, borrar rigidbody
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + Mathf.Sin(Time.time * amplitude) * frequency,
            transform.position.z
            );
    }
}
