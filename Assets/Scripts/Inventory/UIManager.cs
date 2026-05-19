using NUnit;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Inventory")]
    public Image ghostIcon;

    [Header("Health")]
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Image healthFill;

    [Header("Level and experience")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Image expFill;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null) healthText.text = $"HP: {current}";
        if (healthFill != null) healthFill.fillAmount = (float)current / max;
    }

    public void UpdateExp(int current, int max, int level)
    {
        if (levelText != null) levelText.text = level.ToString();
        if (expText != null) expText.text = $"{current} exp / {max} exp";
        if (expFill != null) expFill.fillAmount = max > 0 ? (float)current / max : 1f;
    }
}
