using System;
using UnityEngine;

public class VFXItemPickUp : MonoBehaviour
{
    float rotateSpeed = 50f;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
