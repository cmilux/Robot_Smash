using System.Collections.Generic;
using UnityEngine;

//marks an area of the map w a music track. uses a box collider only to define the size
[RequireComponent(typeof(BoxCollider))]
public class BiomeMusicZone : MonoBehaviour
{
    static readonly List<BiomeMusicZone> _zones = new List<BiomeMusicZone>();

    [Header("Music and Volume")]
    [SerializeField] AudioClip _music;
    public AudioClip Music => _music;
    [SerializeField][Range(0f, 1f)] float _volume = 1f;
    public float Volume => _volume;

    Bounds _bounds;

    private void OnEnable() => _zones.Add(this);
    private void OnDisable() => _zones.Remove(this);

    void Start()
    {
        //cache the size and turn the collider off
        BoxCollider box = GetComponent<BoxCollider>();
        _bounds = box.bounds;
        box.enabled = false;
    }

    //returns the zone that contains this position
    public static BiomeMusicZone GetZone(Vector3 position)
    {
        foreach (BiomeMusicZone zone in _zones)
        {
            if (position.x >= zone._bounds.min.x && position.x <= zone._bounds.max.x &&
                position.z >= zone._bounds.min.z && position.z <= zone._bounds.max.z)
                return zone;
        }
        return null;
    }
}