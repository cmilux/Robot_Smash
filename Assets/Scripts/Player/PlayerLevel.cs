using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] AnimationCurve expCurve;

    int currentLevel;
    int totalExp;
    int prevLevExp;
    int nextLevExp;

    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Image expFill;
    private void Start()
    {
        UpdateLevel();
    }

    public void AddExp(int amount)
    {
        totalExp += amount;
        CheckForLevelUp();
        UpdateUI();
    }

    void CheckForLevelUp()
    {
        if (totalExp >= nextLevExp)
        {
            currentLevel++;
            UpdateLevel();
        }
    }

    void UpdateLevel()
    {
        prevLevExp = (int)expCurve.Evaluate(currentLevel);
        nextLevExp = (int)expCurve.Evaluate(currentLevel + 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        int start = totalExp - prevLevExp;
        int end = nextLevExp - prevLevExp;

        levelText.text = currentLevel.ToString();
        expText.text = $"{start} exp / {end} exp";
        expFill.fillAmount = (float)start / (float)end;
    }
}
