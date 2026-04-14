using UnityEngine;

public class KamikazeEnemy : MonoBehaviour
{
    EnemyHealth _enemyHealht;
    public ParticleSystem _explosion;

    private void Start()
    {
        _enemyHealht = GetComponent<EnemyHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _explosion.Play();
        //Destroy enemy if player collides w it
        Destroy(gameObject, 5f);
    }
}
