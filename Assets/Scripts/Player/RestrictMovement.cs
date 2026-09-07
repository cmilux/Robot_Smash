using UnityEngine;

public class RestrictMovement : MonoBehaviour
{
    float xfloatMin = 35f;
    float xfloatMax = 657f;
    float yfloatMin;                //In case they need to be used in the future for heights or something like that
    float yfloatMax;
    float zfloatMin = 32f;
    float zfloatMax = 540f;

    // Update is called once per frame
    void FixedUpdate()
    {
        //Restricts the player from leaving the terrain || Mantiene al player dentro del terreno de juego
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, xfloatMin, xfloatMax),            //Mathf.Clamp returns a value between a min and max value || Devuelve un valor entre el minimo el maximo
            transform.position.y, 
            Mathf.Clamp(transform.position.z, zfloatMin, zfloatMax)
            );
    }
}
