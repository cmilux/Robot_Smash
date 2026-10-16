using System.Collections;
using TMPro;
using UnityEngine;

public class RecipeUnlockPopup : MonoBehaviour
{
    [SerializeField] private GameObject _unlockPopUpImage;
    [SerializeField] private TextMeshProUGUI _unlockPopUpText;
    [SerializeField] private float _showDuration = 2f;
    private Coroutine _hideRoutine;
    void Start()
    {
        if (PlayerLevelUI.Instance != null)
        {
            PlayerLevelUI.Instance.OnLevelChanged += HandleLevelChanged;
        }

    }
    private void OnDestroy()
    {
        if (PlayerLevelUI.Instance != null)
        {
            PlayerLevelUI.Instance.OnLevelChanged -= HandleLevelChanged;
        }
    }
    private void HandleLevelChanged(int newLevel)
    {
        foreach (RecipeData recipe in GameManager.instance.itemDataBase.recipes)
        {
            if (recipe == null) continue;

            if (recipe.requiredLevel == newLevel)
            {
                _unlockPopUpImage.SetActive(true);
                _unlockPopUpText.text = $"Nueva receta desbloqueada: {recipe.result.nombre}";
                
                if(_hideRoutine != null) StopCoroutine(_hideRoutine);
                _hideRoutine = StartCoroutine(HideAfterDelay());
            }
        }
    }
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(_showDuration);
        _unlockPopUpImage.SetActive(false);
    }
}
