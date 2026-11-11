using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Music and Volume")]
    [SerializeField] AudioClip _music;
    [SerializeField][Range(0f, 1f)] float _volume = 1f;

    void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(_music, _volume);
    }
}