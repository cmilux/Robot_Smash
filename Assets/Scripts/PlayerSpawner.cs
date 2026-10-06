using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] NetworkObject _playerPrefab;
    [SerializeField] Transform[] _spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    void OnSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimeOut)
    {
        int i = 0;
        foreach (ulong clientId in clientsCompleted)
        {
            Transform point = _spawnPoints[i % _spawnPoints.Length];
            NetworkObject car = Instantiate(_playerPrefab, point.position, point.rotation);
            car.SpawnAsPlayerObject(clientId);
            i++;
        }
    }
}
