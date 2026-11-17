using UnityEngine;
using UnityEngine.InputSystem;
public class QuitOnEscape : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
            return;
        }
    }

    void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}
