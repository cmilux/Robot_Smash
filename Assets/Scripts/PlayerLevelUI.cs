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

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Image expFill;

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

        levelText.text = currentLevel.ToString();
        expText.text = $"{start} exp / {end} exp";
        expFill.fillAmount = end > 0 ? (float)start / end : 1f;

        Debug.Log($"EXP: {totalExp} | prev: {prevLevExp} | next: {nextLevExp}");
    }

    int GetExpForLevel(int level) => (int)expCurve.Evaluate(level);

    /*[Header("Player experience")]
    [SerializeField] AnimationCurve expCurve;

    [SerializeField] NetworkVariable<int> currentLevel = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> totalExp = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    int prevLevExp;
    int nextLevExp;

    [Header("User interface elements")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Image expFill;

    public static PlayerLevelUI Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        currentLevel.OnValueChanged += OnLevelChanged;
        totalExp.OnValueChanged += OnExpChanged;

        UpdateUI();

        if (!IsOwner)
        {
            levelText.gameObject.SetActive(false);
            expText.gameObject.SetActive(false);
            expFill.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        currentLevel.OnValueChanged -= OnLevelChanged;
        totalExp.OnValueChanged -= OnExpChanged;
    }

    public void AddExp(int amount)
    {
        if (!IsOwner) return;
        AddExpServerRpc(amount);
    }

    [ServerRpc]
    void AddExpServerRpc(int amount)
    {
        totalExp.Value += amount;         //Add experience to the total
        CheckForLevelUp();
    }

    void CheckForLevelUp()
    {
        //If experience is a bigger value than stablished on curve
        while (totalExp.Value >= GetExpForLevel(currentLevel.Value + 1))
        {
            currentLevel.Value++;         //Set player to next level
        }
    }

    void OnLevelChanged(int prev, int next) => UpdateUI();
    void OnExpChanged(int prev, int next) => UpdateUI();

    void UpdateUI()
    {
        if(!IsOwner) return;

        int prevLevExp = GetExpForLevel(currentLevel.Value);
        int nextLevExp = GetExpForLevel(currentLevel.Value + 1);

        int start = totalExp.Value - prevLevExp;
        int end = nextLevExp - prevLevExp;

        levelText.text = currentLevel.Value.ToString();
        expText.text = $"{start} exp / {end} exp";          //Set current exp and how much u need to go to next level
        expFill.fillAmount =end > 0? (float)start / (float)end: 1f;     //Fills the experience bar from the UI

        Debug.Log($"EXP: {totalExp} | prev: {prevLevExp} | next: {nextLevExp}");
        Debug.Log("Updating UI on: " + gameObject.name);
    }

    int GetExpForLevel(int level) => (int)expCurve.Evaluate(level);*/
}
