using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public ItemDataBase itemDataBase;

    private void Awake()
    {
        if (instance == null) {  instance = this; }
        else { Destroy(gameObject);}

        //Inicializamos la DB
        itemDataBase.InitializeDataBase();
    }
}
