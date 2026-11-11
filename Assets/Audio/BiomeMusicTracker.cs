using Unity.Netcode;
using UnityEngine;

//lives on the player prefab, tracks the biome music zone the player is in
public class BiomeMusicTracker : NetworkBehaviour
{
    [SerializeField] float _checkInterval = 1f;
    float _nextCheck;
    BiomeMusicZone _currentZone;

    void Update()
    {
        if (!IsOwner) return;
        if (Time.time < _nextCheck)
            _nextCheck = Time.time + _checkInterval;

        if (AudioManager.Instance == null) return;

        BiomeMusicZone zone = BiomeMusicZone.GetZone(transform.position);
        _currentZone = zone;

        //if the track is already playing, the AudioManager will just adjust the volume
        if (zone != null) AudioManager.Instance.PlayMusic(zone.Music, zone.Volume);
    }

}