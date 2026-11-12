using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

//one id per sound effect
public enum SFXId
{
    UIClick,
    PlayerShoot,
    EnemyShoot,
    Explosion,
    PlayerHit,
    PlayerDamage,
    EnemySteps,
    ItemPickup,
    BoxBreak,
    Craft,
    Heal,
    Death,
    Dash,
    LevelUp,
    QuestComplete,
    GameOver,
    UIStart,
    InventoryOpen,
    InventoryClose,
    LowHealth,
    EnemyDeath
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SFX
    {
        public SFXId id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitchVariation = 0.05f;
        public float minInterval = 0.05f;       //ignores repeats of this sound inside this time
    }

    [Header("Mixer")]
    [SerializeField] AudioMixer _mixer;
    [SerializeField] AudioMixerGroup _musicGroup;
    [SerializeField] AudioMixerGroup _sfxGroup;

    [Header("Music")]
    [SerializeField] float _fadeTime = 1.5f;

    [Header("SFX")]
    [SerializeField] List<SFX> _sfxList = new List<SFX>();
    [SerializeField] int _poolSize = 16;
    [SerializeField] float _minDistance = 5f;
    [SerializeField] float _maxDistance = 60f;

    //static, survives scene loads, session only
    static float _musicVolume = 0.8f;
    static float _sfxVolume = 0.8f;
    public float MusicVolume => _musicVolume;
    public float SFXVolume => _sfxVolume;

    AudioSource _musicA, _musicB, _currentMusic;
    Coroutine _fadeRoutine;

    AudioSource[] _pool;
    int _nextSource;
    Dictionary<SFXId, SFX> _sfx = new Dictionary<SFXId, SFX>();
    Dictionary<SFXId, float> _lastPlay = new Dictionary<SFXId, float>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (SFX sfx in _sfxList) _sfx[sfx.id] = sfx;
        {
            _musicA = NewSource("Music A", _musicGroup, true);
            _musicB = NewSource("Music B", _musicGroup, true);
            _currentMusic = _musicA;

            _pool = new AudioSource[_poolSize];
            for (int i = 0; i < _poolSize; i++) _pool[i] = NewSource($"SFX {i}", _sfxGroup, false);

            SetMusicVolume(_musicVolume);
            SetSFXVolume(_sfxVolume);
        }
    }
    AudioSource NewSource(string sourceName, AudioMixerGroup group, bool loop)
    {
        GameObject go = new GameObject(sourceName);
        go.transform.SetParent(transform);

        AudioSource source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = group;
        source.playOnAwake = false;
        source.loop = loop;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = _minDistance;
        source.maxDistance = _maxDistance;
        return source;
    }

    #region Volume
    //slider value foes 0-1, mixer works in decibels
    static float ToDecibels(float v) => Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f;

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        _mixer.SetFloat("MusicVolume", ToDecibels(_musicVolume));
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        _mixer.SetFloat("SFXVolume", ToDecibels(_sfxVolume));
    }
    #endregion

    #region Music
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        volume = Mathf.Clamp01(volume);

        if (_currentMusic.clip == clip && _currentMusic.isPlaying)
        {
            if (_fadeRoutine == null) _currentMusic.volume = volume; //already playing this track, just adjust volume
            return;
        }

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossFadeRoutine(clip, volume));
    }

    //fades the old track out while the new one fades in
    IEnumerator CrossFadeRoutine(AudioClip clip, float targetVolume)
    {
        AudioSource from = _currentMusic;
        AudioSource to = (from == _musicA) ? _musicB : _musicA;
        _currentMusic = to;

        float fromStart = from.volume;

        if (!(to.isPlaying && to.clip == clip))
        {
            to.clip = clip;
            to.volume = 0f;
            to.Play();
        }
        float toStart = to.volume;

        float t = 0f;
        while (t < _fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / _fadeTime);

            to.volume = Mathf.Lerp(toStart, targetVolume, Mathf.Sin(k * Mathf.PI * 0.5f)); //ease in
            from.volume = fromStart * Mathf.Cos(k * Mathf.PI * 0.5f); //ease out

            yield return null;
        }

        to.volume = targetVolume;
        from.Stop();
        from.volume = 1f;
        _fadeRoutine = null;
    }
    #endregion

    #region SFX
    //no position, 2d sounds (UI, car)
    //with position, 3d sounds in the world
    public void PlaySfx(SFXId id) => Play(id, null);
    public void PlaySfxAt(SFXId id, Vector3 position) => Play(id, position);

    void Play(SFXId id, Vector3? position)
    {
        if (!_sfx.TryGetValue(id, out SFX sfx) || sfx.clips == null || sfx.clips.Length == 0) return;
        //if (_lastPlay.TryGetValue(id, out float last) && Time.unscaledDeltaTime - last < s.minInterval) return;
        //_lastPlay[id] = Time.unscaledDeltaTime;

        AudioSource source = _pool[_nextSource];
        _nextSource = (_nextSource + 1) % _pool.Length;     //round robin: reuses the oldest source

        source.transform.position = position ?? Vector3.zero;
        source.spatialBlend = position.HasValue ? 1f : 0f;   //3d or 2d sound
        source.pitch = 1f + Random.Range(-sfx.pitchVariation, sfx.pitchVariation);
        source.clip = sfx.clips[Random.Range(0, sfx.clips.Length)];
        source.volume = sfx.volume;
        source.Play();
    }
    #endregion
}