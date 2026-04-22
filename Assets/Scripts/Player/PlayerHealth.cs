using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player life")]
    public int health;
    public int maxHealth = 5;
    public bool isDead = false;

    [Header("User interface elements")]
    [SerializeField] TextMeshProUGUI totalLifeText;
    [SerializeField] Image lifeFill;

    private void Start()
    {
        health = maxHealth;
        totalLifeText.text = $"HP: {health.ToString()}";
    }
    public void LoseHealth(int damage)
    {
        if (isDead) return;

        health -= damage;

        //update ui health bar
        UpdateUI();

        if (health <= 0)
        {//pero para la ui queda en cero
            health = 0;

            isDead = true;

            //para el enemigo torreta if (player == null || !player.gameObject.activeInHierarchy) return;

            gameObject.SetActive(false);
        }
    }

    void UpdateUI()
    {
        totalLifeText.text = $"HP: {health.ToString()}";        //Set current life
        lifeFill.fillAmount = (float)health / maxHealth;     //Fills the life bar from the UI
    }
}
