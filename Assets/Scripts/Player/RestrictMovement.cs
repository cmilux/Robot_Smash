using UnityEngine;

public class RestrictMovement : MonoBehaviour
{
    float xfloatMin = 2f;
    float xfloatMax = 998f;
    float yfloatMin;                //In case they need to be used in the future for heights or something like that
    float yfloatMax;
    float zfloatMin = 2f;
    float zfloatMax = 998f;

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
