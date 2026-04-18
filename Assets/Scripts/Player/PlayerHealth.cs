using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health;
    public int maxHealth = 5;
    public bool isDead = false;

    private void Start()
    {
        health = maxHealth;
    }
    public void LoseHealth(int damage)
    {
        if (isDead) return;

        health -= damage;
        //por consola pueden salir valores negativos
        Debug.Log($"player health {health}");

        if (health <= 0)
        {//pero para la ui queda en cero
            health = 0;

            isDead = true;

            Debug.Log("player died");

            //para el enemigo torreta if (player == null || !player.gameObject.activeInHierarchy) return;

            gameObject.SetActive(false);
        }
    }

}
