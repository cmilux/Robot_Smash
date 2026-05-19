using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerLevelUI : MonoBehaviour
{
    [Header("Player Experience")]
    [SerializeField] AnimationCurve expCurve;
    int currentLevel;
    int totalExp;

    //one shared copy across everything
    public static PlayerLevelUI Instance { get; private set; }      //anyone can read it, but only the class itself can write to it

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateLevel();
    }

    public void AddExp(int amount)
    {
        totalExp += amount;
        CheckForLevelUp();
        UpdateLevel();
    }

    void CheckForLevelUp()
    {
        while (totalExp >= GetExpForLevel(currentLevel + 1))
        {
            currentLevel++;
        }
    }

    void UpdateLevel()
    {
        int prevLevExp = GetExpForLevel(currentLevel);
        int nextLevExp = GetExpForLevel(currentLevel + 1);

        int start = totalExp - prevLevExp;
        int end = nextLevExp - prevLevExp;

        UIManager.Instance.UpdateExp(start, end, currentLevel);
    }

    int GetExpForLevel(int level) => (int)expCurve.Evaluate(level);

}
