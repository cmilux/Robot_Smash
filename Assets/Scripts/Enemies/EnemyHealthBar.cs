using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            Debug.Log("cam is null");
        }

        transform.LookAt(transform.position + cam.forward);
    }
}
