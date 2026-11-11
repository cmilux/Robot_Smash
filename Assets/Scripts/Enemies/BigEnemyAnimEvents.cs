using UnityEngine;

// Lives on the model with the Animator and forwards animation events to BigEnemy on the parent
public class BigEnemyAnimEvents : MonoBehaviour
{
    private BigEnemy _bigEnemy;

    private void Awake()
    {
        _bigEnemy = GetComponentInParent<BigEnemy>();
    }

    // Called by the animation event on ANIM_Enemy_Big_Attack_Spawn
    public void OnSpawnThrow()
    {
        if (_bigEnemy != null) _bigEnemy.OnSpawnThrow();
    }
}
