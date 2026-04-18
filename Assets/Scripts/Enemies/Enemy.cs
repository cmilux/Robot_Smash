using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 3;
    public bool isDead;
    public float timeBeforeDestroy;

    public virtual void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            isDead = true;
            Die(timeBeforeDestroy);
        }
    }

    protected virtual void Die(float timeBeforeDestroys)
    {
        Destroy(gameObject, timeBeforeDestroy);
    }
}
