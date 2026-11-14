using UnityEngine;
using TMPro;

//fills it with the credits whenever its panel opens
[RequireComponent(typeof(TextMeshProUGUI))]
public class CreditsText : MonoBehaviour
{
    [SerializeField] CreditsData _credits;

    private void OnEnable()
    {
        GetComponent<TextMeshProUGUI>().text = _credits != null ? _credits.text : "";
    }
}
