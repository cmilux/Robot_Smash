using NUnit;
using System;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //one shared copy across everything
    public static UIManager Instance { get; private set; }      //anyone can read it, but only the class itself can write to it

    [Header("Inventory")]
    public Image ghostIcon;
    [SerializeField] GameObject inventoryPanel;

    [Header("Craft")]
    [SerializeField] GameObject craftPanel;

    [Header("Player Health")]
    
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Image healthFill;

    [Header("Level and experience")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] Image expFill;

    [Header("Quests")]
    [SerializeField] GameObject questPanel;           // el panel entero del HUD de mision, para poder ocultarlo
    [SerializeField] TextMeshProUGUI questTitleText;
    [SerializeField] TextMeshProUGUI questObjectivesText;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button gameOverMenuButton;
    [SerializeField] string menuSceneName = "Lobby";

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(ReturnToMenu);
    }

    private void Update()
    {
        if (inventoryPanel.activeSelf && craftPanel.activeSelf)
        {
            questPanel.SetActive(false);
        }
        else
        {
            questPanel.SetActive(true);
        }
    }

    public void UpdatePlayerHealth(int current, int max)
    {
        if (healthText != null) healthText.text = $"HP: {current}";
        if (healthFill != null) healthFill.fillAmount = (float)current / max;
    }

    public void UpdatePlayerExp(int current, int max, int level)
    {
        if (levelText != null) levelText.text = level.ToString();
        if (expText != null) expText.text = $"{current} exp / {max} exp";
        if (expFill != null) expFill.fillAmount = max > 0 ? (float)current / max : 1f;
    }

    // called directly by QuestManager whenever the active quest or progress changes
    public void UpdateQuest(QuestData quest, NetworkList<int> progress)
    {
        Debug.Log($"[UI] UpdateQuest: quest={quest?.questTitle ?? "NULL"}, progress.Count={progress.Count}, len={quest?.objectives.Count}");

        if (quest == null)
        {
            questPanel.SetActive(false);
            return;
        }

        // the list is still being rebuilt (mid Clear/Add sequence) — skip this transient update,
        // the next event fired once everything settles will have matching lengths
        if (quest.objectives.Count != progress.Count) return;

        questPanel.SetActive(true);
        questTitleText.text = quest.questTitle;

        string objectivesList = "";
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            QuestObjective obj = quest.objectives[i];
            string bonusTag = obj.isOptional ? " (bonus)" : "";

            // build the plain line first
            string line = $"{obj.description}: {progress[i]}/{obj.requiredAmount}{bonusTag}";

            // if this specific objective is done, wrap it in strikethrough tags
            bool isDone = progress[i] >= obj.requiredAmount;
            if (isDone)
            {
                line = $"<s>{line}</s>"; // <s> and </s> are TextMeshPro's rich text tags for strikethrough
            }

            objectivesList += line + "\n";
        }

        questObjectivesText.text = objectivesList;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void ReturnToMenu()
    {
        //network manager survives scene loads, so needs to be shut it down and destroy it
        //otherwise the lobby own copy will duplicate it
        //el network manager sobrevive a las escenas, apagarlo y destruirlo evita duplicados en lobby
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.Shutdown();
            Destroy(nm.gameObject);
        }

        SceneManager.LoadScene(menuSceneName);
    }
}
