using UnityEngine;
using UnityEngine.UI;
public class LobbyMenu : MonoBehaviour
{
    [SerializeField] GameObject _carSelectPanel;
    [SerializeField] GameObject _connectionPanel;
    [SerializeField] Button _continueButton;
    [System.Serializable]
    public class CarOption
    {
        public Button button;
        public int variantId;   
    }

    [SerializeField] CarOption[] _carOptions;

    private void Start()
    {
        _continueButton.onClick.AddListener(Continue);

        foreach (var carOption in _carOptions)
        {
            carOption.button.onClick.AddListener(() => SelectCar(carOption.variantId));
        }
    }
    public void SelectCar(int variantId)
    {
        PlayerCarChoices.SelectedCarVariantId = variantId;
    }
    public void Continue()
    {
        _carSelectPanel.SetActive(false);
        _connectionPanel.SetActive(true);
    }
}
