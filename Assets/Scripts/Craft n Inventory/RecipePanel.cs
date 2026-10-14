using UnityEngine;

public class RecipePanel : MonoBehaviour
{
    [SerializeField] private GameObject _entryPrefab;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if(GameManager.instance == null) return;

        int level = PlayerLevelUI.Instance != null ? PlayerLevelUI.Instance.CurrentLevel : 0;

        foreach(RecipeData recipe in GameManager.instance.itemDataBase.recipes)
        {
            if(recipe ==null) continue;

            GameObject row = Instantiate(_entryPrefab, transform);
            RecipeEntryUi entry = row.GetComponent<RecipeEntryUi>();

            if(entry != null)
            {
                bool unlocked = (level >= recipe.requiredLevel);
                entry.Configure(recipe, unlocked);
            }
        }
    }
}
