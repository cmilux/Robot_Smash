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
        //Adds experience based on enemy killed (set indivually in each enemy) || agrega experiencia basado en el enemigo derrotado (se configura en el inspector de cada enemigo)
        totalExp += amount;
        //Updates level, exp and UI
        CheckForLevelUp();
        UpdateLevel();
    }

    void CheckForLevelUp()
    {
        while (totalExp >= GetExpForLevel(currentLevel + 1))
        {
            currentLevel++;     //Level up player if exp needed for that level was reached || sube de nivel al player si tiene la experiencia necesaria
        }
    }

    void UpdateLevel()
    {
        int prevLevExp = GetExpForLevel(currentLevel);  //exp needed for current level || exp necesaria para el nivel actual
        int nextLevExp = GetExpForLevel(currentLevel + 1);  //exp needed for next level || exp necesaria para el prox nivel

        int start = totalExp - prevLevExp;      //how much exp was earned in current level || cuanta exp gano en el nivel actual
        int end = nextLevExp - prevLevExp;      //total exp needed to complete this level || exp total necesaria para completar el nivel actual

        UIManager.Instance.UpdateExp(start, end, currentLevel);
    }

    //samples the animation curve to get the exp threshold for a given level || evalua la curva de animacion para obtener el umbral de exp de un nivel
    int GetExpForLevel(int level) => (int)expCurve.Evaluate(level);

}
