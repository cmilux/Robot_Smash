using UnityEngine;
using UnityEngine.UI;

//put this on the toggle in the lobby ui
//writes the player choice to tutorialsettings
[RequireComponent(typeof(Toggle))]
public class TutorialToggle : MonoBehaviour
{
    //tick this only on the first in the first screen the player sees
    //it resets the choice to "tutorial on"
    [SerializeField] bool _resetToDefaultOnAwake = false;
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();

        if (_resetToDefaultOnAwake) TutorialSettings.Enabled = true;

        _toggle.SetIsOnWithoutNotify(TutorialSettings.Enabled);
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDestroy()
    {
        if (_toggle != null) _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn) => TutorialSettings.Enabled = isOn;
}
