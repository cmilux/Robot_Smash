using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CarSelectPanel : MonoBehaviour
{
    [System.Serializable]
    public class CarOption
    {
        public Button button;
        public int variantId;//-1 = default car
    }

    [SerializeField] CarOption[] _carOptions;
    [SerializeField] Button _confirmButton;

    private int _selectedId = -1;//the car the player picked, default until they choose
    private InventoryManager _localInventory; //the local player's car, null until it spawns
    private PlayerInput _localInput;

    void Start()
    {
        _confirmButton.onClick.AddListener(Confirm);

        foreach (var carOption in _carOptions)
        {
            carOption.button.onClick.AddListener(() => SelectCar(carOption.variantId));
        }
    }
    private void Update()
    {
        //the car locks the cursor when it spawns, keep it free while choosing
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //already found the local car, nothing else to do
        if (_localInventory != null) return;

        //wait until the local player's car exists
        if(NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null) return;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (localPlayer == null) return;

        _localInventory = localPlayer.GetComponent<InventoryManager>();
        _localInput = localPlayer.GetComponent<PlayerInput>();

        //dont let the car move while the menu is open
        if(_localInput != null) _localInput.enabled = false;

        //apply the car the player picked before their car appeared
        if (_localInventory != null) _localInventory.SetCarVariant(_selectedId);
    }
    private void SelectCar(int variantId)
    {
        _selectedId = variantId;
        
        if( _localInventory != null)
        {
            _localInventory.SetCarVariant(variantId);
        }
    }
    private void Confirm()
    {
        //cant confirm until the car exists
        if(_localInventory ==  null) return;

        _localInventory.SetCarVariant(_selectedId);

        //give the controls back to the player
        if (_localInput != null) _localInput.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        gameObject.SetActive(false);
    }
}
