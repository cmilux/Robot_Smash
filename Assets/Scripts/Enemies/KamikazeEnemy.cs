using UnityEngine;

public class KamikazeEnemy : MonoBehaviour
{
    //EnemyHealth _enemyHealht;
    public ParticleSystem _explosion;

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
            Destroy(transform.root.gameObject, 5f);
        }
    }
}
