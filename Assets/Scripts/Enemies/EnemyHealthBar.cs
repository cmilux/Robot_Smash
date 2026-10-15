using Unity.Profiling;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    //shows up in profiler>cpu usage as "EnemyHealthBar.Billboard"
    //the time is the total for all health bars, and the calls columns tells u how many ran
    static readonly ProfilerMarker s_BillboardMarker = new ProfilerMarker("EnemyHealthBar.Billboard");

    Transform _cam;

    private void LateUpdate()
    {
        using (s_BillboardMarker.Auto())
        {
            if (_cam == null)
            {
                Camera main = Camera.main;
                if (main == null) return;       //no active camera right now, try again next frame
                _cam = main.transform;
            }
        }

        transform.LookAt(transform.position + _cam.forward);
    }
}
