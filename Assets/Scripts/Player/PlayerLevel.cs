using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    [Header("Player experience")]
    [SerializeField] AnimationCurve expCurve;
    int currentLevel;
    int totalExp;
    int prevLevExp;
    int nextLevExp;

    [Header("User interface elements")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Image expFill;

    private void Start()
    {
        UpdateLevel();
    }

    public void AddExp(int amount)
    {
        totalExp += amount;         //Add experience to the total
        CheckForLevelUp();
        UpdateUI();
    }

    void CheckForLevelUp()
    {
        //If experience is a bigger value than stablished on curve
        if (totalExp >= nextLevExp)
        {
            currentLevel++;         //Set player to next level
            UpdateLevel();
        }
    }

    void UpdateLevel()
    {
        //Checks the value of the curve with previous and next level of experience
        prevLevExp = (int)expCurve.Evaluate(currentLevel);
        nextLevExp = (int)expCurve.Evaluate(currentLevel + 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        int start = totalExp - prevLevExp;
        int end = nextLevExp - prevLevExp;

        levelText.text = currentLevel.ToString();
        expText.text = $"{start} exp / {end} exp";          //Set current exp and how much u need to go to next level
        expFill.fillAmount = (float)start / (float)end;     //Fills the experience bar from the UI
    }
}
