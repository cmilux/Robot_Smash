using System;
using UnityEngine;

public class VFXItemPickUp : MonoBehaviour
{
    float _rotateSpeed = 50f;
    [SerializeField] float _amplitude = 0.5f;
    [SerializeField] float _frequency = 0.01f;

    Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        //rotate item
        transform.Rotate(Vector3.up * _rotateSpeed * Time.deltaTime);

        //si no queremos q flote borrar esto
        //si queremos q flote, borrar rigidbody
        transform.position = new Vector3(
            _startPos.x,
            _startPos.y + Mathf.Sin(Time.time * _frequency) * _amplitude,
            _startPos.z
            );
    }
}
