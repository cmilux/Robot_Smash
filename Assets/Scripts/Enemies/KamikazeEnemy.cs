using UnityEngine;

public class KamikazeEnemy : MonoBehaviour
{
    //EnemyHealth _enemyHealht;
    public ParticleSystem _explosion;
    public bool isDead = false;

    private void Start()
    {
        //_enemyHealht = GetComponent<EnemyHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _explosion.Play();
            //Destroy enemy if player collides w it
            isDead = true;
            Destroy(transform.root.gameObject, 5f);
        }
    }
}
