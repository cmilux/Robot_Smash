using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script created to hide/show all ui to record trailer
/// Press F9 to hide/show
/// </summary>
public class UIHider : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (FindFirstObjectByType<UIHider>() != null) return;   //never create two

        GameObject go = new GameObject("UIHider");
        go.AddComponent<UIHider>();
        DontDestroyOnLoad(go);
    }

    bool _hidden;
    float _nextApply;

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f9Key.wasPressedThisFrame)
        {
            _hidden = !_hidden;
            Apply();
        }

        //while hidde, re hidde every second: new scenes and pooled enemy bars create canvases
        if (_hidden && Time.unscaledDeltaTime >= _nextApply)
        {
            _nextApply = Time.unscaledDeltaTime + 3f;
            Apply();
        }
    }

    void Apply()
    {
        //include inactive ones so they are hidden too
        foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            c.enabled = !_hidden;
    }
}
