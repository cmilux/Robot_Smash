using UnityEngine;

public class BarrelSpin : MonoBehaviour
{
    public Transform barrel;
    public float degreesPerShot = 70f;

    private PlayerAttackDistance weapon;

    private void OnEnable()
    {
        weapon = GetComponentInParent<PlayerAttackDistance>();

        if(weapon != null)
        {
            weapon.OnShoot += rotateBarrel;
        }
    }
    private void OnDisable()
    {
        if(weapon != null)
        {
            weapon.OnShoot -= rotateBarrel;
        }
    }

    void rotateBarrel()
    {
        barrel.Rotate(0f, 0f, degreesPerShot, Space.Self);
    }
}
