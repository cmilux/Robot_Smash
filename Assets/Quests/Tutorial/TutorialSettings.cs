using UnityEngine;

//per player, per session choice made in the lobby
//static survives scene loads
//lobby > gamescene but its not saved to disk, so nothing carries over between people at expo
public class TutorialSettings : MonoBehaviour
{
    public static bool Enabled = true;

    //editor safety: if domain reload is disabled, statics would keep their old value between play sessions
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLaunch() => Enabled = true;
}
