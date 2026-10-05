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
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [SerializeField] Button _hostButton;
    [SerializeField] Button _joinButton;
    [SerializeField] TMP_InputField _joinInputField;
    [SerializeField] TextMeshProUGUI _codeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _hostButton.onClick.AddListener(CreateRelay);
        _joinButton.onClick.AddListener(() => JoinRelay(_joinInputField.text));
    }

    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);       //parameter indicates the maximum number of connections that the client will allow
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        _codeText.text = "Code: " + joinCode;

        var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");      //datagram transport layer security

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started");
    }

    async void JoinRelay(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
        Debug.Log("A client just joined");
    }
}