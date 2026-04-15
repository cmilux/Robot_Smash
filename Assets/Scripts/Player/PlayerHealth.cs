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

        Debug.Log($"player health {health}");
        
        if (health <= 0)
        {  
          isDead = true;

          health = 0;

          Debug.Log("player died");

          gameObject.SetActive(false);

        }   
    }
    
}
