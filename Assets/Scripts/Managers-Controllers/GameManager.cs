using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public ItemDataBase itemDataBase;

    private void Awake()
    {
        if (instance == null) {  instance = this; }
        else { Destroy(gameObject);}

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;       //fps limit || limite de fps

        //Inicializamos la DB
        itemDataBase.InitializeDataBase();
    }
}
