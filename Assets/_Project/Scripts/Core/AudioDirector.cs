using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// فاز A / قدم A6 — کارگردان صدا. v3: فال‌بک هوشمند AudioListener
/// (اگر دوربینِ شنونده رسید، فال‌بک حذف می‌شود؛ اگر هیچ نبود، ساخته می‌شود).
/// </summary>
public class AudioDirector : MonoBehaviour
{
    private static AudioDirector _inst;
    private AudioSource _amb;
    private AudioSource _sfx;
    private AudioBank _bank;
    private AudioListener _fallbackListener;
    private readonly Dictionary<string, AudioClip> _cache = new();
    private string _ambKey = "";
    private long _lastBalance;
    private bool _hasBalance;
    private PlayerController _player;
    private float _stepAcc;
    private const float StepInterval = 0.34f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<AudioDirector>() != null) return;
        var go = new GameObject("AudioDirector");
        DontDestroyOnLoad(go);
        go.AddComponent<AudioDirector>();
    }

    public static void Play(string key)
    {
        if (_inst != null) _inst.PlayInternal(key);
    }

    private void Awake()
    {
        if (_inst != null && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        _amb = gameObject.AddComponent<AudioSource>();
        _amb.loop = true; _amb.playOnAwake = false; _amb.volume = 0.6f;
        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false; _sfx.volume = 0.9f;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ServiceLocator.Ensure();
        var bus = ServiceLocator.EventBus;
        if (bus != null) bus.Subscribe<BalanceChangedEvent>(OnBalance);
        SyncListener();
        SetAmbienceFor(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (ServiceLocator.IsReady)
            ServiceLocator.EventBus.Unsubscribe<BalanceChangedEvent>(OnBalance);
        if (_inst == this) _inst = null;
    }

    /// <summary>تضمین دقیقاً یک AudioListener: فال‌بک فقط وقتی هیچ‌کس ندارد؛ با رسیدن شنونده‌ی دوربین، فال‌بک حذف.</summary>
    private void SyncListener()
    {
        var all = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool otherExists = false;
        foreach (var l in all)
            if (l.transform != transform) otherExists = true;
        if (otherExists)
        {
            if (_fallbackListener != null) { Destroy(_fallbackListener); _fallbackListener = null; }
        }
        else if (all.Length == 0 && _fallbackListener == null)
        {
            _fallbackListener = gameObject.AddComponent<AudioListener>();
        }
    }

    private void OnBalance(BalanceChangedEvent e)
    {
        long b = ServiceLocator.Economy.Balance;
        if (_hasBalance)
        {
            if (b > _lastBalance) PlayInternal("coin");
            else if (b < _lastBalance) PlayInternal("spend");
        }
        _lastBalance = b; _hasBalance = true;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        PlayInternal("whoosh");
        SyncListener();
        SetAmbienceFor(s.name);
    }

    private void SetAmbienceFor(string sceneName)
    {
        string clip =
            sceneName.Contains("Room01") ? "AMB_Room" :
            sceneName.Contains("Block01") ? "AMB_Alley" :
            sceneName.Contains("MobileShop01") ? "AMB_Shop" : "";
        if (clip == "" || clip == _ambKey) return;
        _ambKey = clip;
        var c = Load(clip);
        if (c != null) { _amb.clip = c; _amb.Play(); }
    }

    private void Update()
    {
        if (_player == null) _player = FindFirstObjectByType<PlayerController>();
        if (_player != null && _player.IsMoving)
        {
            _stepAcc += Time.deltaTime;
            if (_stepAcc >= StepInterval)
            {
                _stepAcc = 0f;
                _sfx.pitch = Random.Range(0.9f, 1.1f);
                PlayInternal("footstep");
            }
        }
        else _stepAcc = 0f;
    }

    private void PlayInternal(string key)
    {
        var c = Load("SFX_" + char.ToUpper(key[0]) + key.Substring(1));
        if (c != null && _sfx != null) _sfx.PlayOneShot(c);
    }

    private AudioClip Load(string name)
    {
        if (_cache.TryGetValue(name, out var c)) return c;
        if (_bank == null) _bank = Resources.Load<AudioBank>("AudioBank");
        c = _bank != null ? _bank.Get(name) : null;
        if (c == null) Debug.LogWarning($"[Audio] کلیپ در AudioBank نیست: {name} — Setup 23 را اجرا کن.");
        _cache[name] = c;
        return c;
    }
}