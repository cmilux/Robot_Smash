using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ServerController : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            NetworkManager.Singleton.StartHost(); //Starts hosts || Iniciamos el Host
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            NetworkManager.Singleton.StartClient(); //Starts client || Iniciamos como client
        }
    }
}