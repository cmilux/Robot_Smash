using UnityEngine;

public class SawSpin : MonoBehaviour
{
    public CarSaws carSaws;
    public Transform sawLeft;
    public Transform sawRight;
    public float spinSpeed = 400f; // degrees per second(una vuelta completa son 360 gira mas de una vez por segundo)

    void Update()
    {
        if (carSaws == null) return;
        if (!carSaws.sawsOn) return;

        // Rotate each saw around own axis
        sawLeft.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self); // eje Z(gira en este eje por el mesh placeholder)
        sawRight.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self); // eje Z(gira en este eje por el mesh placeholder)
    }
}