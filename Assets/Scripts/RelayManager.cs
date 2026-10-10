using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] Button _hostButton;
    [SerializeField] Button _joinButton;
    [SerializeField] TMP_InputField _joinInputField;
    [SerializeField] TextMeshProUGUI _codeText;

    [Header("Lobby (host only)")]
    [SerializeField] Button _startButton;
    [SerializeField] TextMeshProUGUI _playersText;

    [Header("Settings")]
    [SerializeField] int _maxClients = 3;       //clients only, the host is not counted. so this 3 = 4 players in total (counting host)
    [SerializeField] string _gameSceneName = "GameScene";

    async void Start()
    {
        //lobby widgets stay hidden until we host
        _startButton.gameObject.SetActive(false);
        _playersText.gameObject.SetActive(false);

        //locked until services are ready
        _hostButton.interactable = false;
        _joinButton.interactable = false;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _hostButton.onClick.AddListener(CreateRelay);
        _joinButton.onClick.AddListener(() => JoinRelay(_joinInputField.text));

        _startButton.onClick.AddListener(StartGame);

        _hostButton.interactable = true;
        _joinButton.interactable = true;
    }

    async void CreateRelay()
    {
        SetMenuInteractable(false);         //avoids double click while waiting

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxClients);       //parameter indicates the maximum number of connections that the client will allow
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            _codeText.text = "Code: " + joinCode;

            var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");      //datagram transport layer security

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayersChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayersChanged;

            NetworkManager.Singleton.StartHost();

            //host stays in the lobby, show the start button and the player count
            _hostButton.gameObject.SetActive(false);
            _joinButton.gameObject.SetActive(false);
            _joinInputField.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(true);
            _playersText.gameObject.SetActive(true);
            UpdatePlayersText();

            Debug.Log("Host started, waiting in lobby");
        }
        catch (RelayServiceException exception)
        {
            Debug.LogError(exception);
            _codeText.text = "Could not create the room";
            SetMenuInteractable(true);
        }
    }

    async void JoinRelay(string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode)) return;

        SetMenuInteractable(false);

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode.Trim());
            var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.OnClientConnectedCallback += OnJoinFailedKicked;
            NetworkManager.Singleton.StartClient();     //no loadScene here, the hosts scene load brings us in

            _hostButton.gameObject.SetActive(false);
            _joinButton.gameObject.SetActive(false);
            _joinInputField.gameObject.SetActive(false);
            _codeText.text = "Waiting for the host to start...";
        }
        catch (RelayServiceException exception)
        {
            Debug.LogError(exception);
            _codeText.text = "Invalid code";
            SetMenuInteractable(true);
        }
    }

    void StartGame()
    {
        //only the host can load scenes and the button only exists for the host
        if (!NetworkManager.Singleton.IsHost) return;

        _startButton.interactable = false;
        NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
    }

    void OnPlayersChanged(ulong cliendId) => UpdatePlayersText();

    void UpdatePlayersText()
    {
        int connected = NetworkManager.Singleton.ConnectedClientsIds.Count;     //includes the host
        _playersText.text = $"Players: {connected}/{_maxClients + 1}";
    }

    //client side, connection failed or the host closed the room
    void OnJoinFailedKicked(ulong cliendId)
    {
        if (cliendId != NetworkManager.Singleton.LocalClientId) return;

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnJoinFailedKicked;
        _codeText.text = "Could not connect";

        _hostButton.gameObject.SetActive(true);
        _joinButton.gameObject.SetActive(true);
        _joinInputField.gameObject.SetActive(true);
        SetMenuInteractable(true);
    }
    
    void SetMenuInteractable(bool value)
    {
        _hostButton.interactable = value;
        _joinButton.interactable = value;
        _joinInputField.interactable = value;
    }

    private void OnDestroy()
    {
        //relay manager lives in the lobby scene, the network manager survives it, so we must unsuscribe
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayersChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayersChanged;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnJoinFailedKicked;
    }
}