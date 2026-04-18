using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy class")]
    public int health = 3;
    public bool isDead;
    public float timeBeforeDestroy;

    public virtual void TakeDamage(int damageAmount)
    {
        //Takes damage from enemies
        health -= damageAmount;

        if (health <= 0)
        {
            isDead = true;                  //Enemy is dead
            Die(timeBeforeDestroy);         //Call Die method wirh parameter
        }
    }

    protected virtual void Die(float timeBeforeDestroys)
    {
        //Enemy will destroy after some time set in parameter
        Destroy(gameObject, timeBeforeDestroy);
    }
}
