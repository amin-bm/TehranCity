using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>در Bootstrap ساخته می‌شود؛ سرویس‌ها را Ensure و شبیه‌سازی را Tick می‌کند.</summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private string nextScene = "";

    private PlayerController _player;
    private float _bridgeAcc;

    private void Awake()
    {
        if (FindFirstObjectByType<GameManager>() != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        ServiceLocator.Ensure();
        if (FindFirstObjectByType<SaveDirector>() == null)
        {
            var sd = new GameObject("SaveDirector");
            sd.AddComponent<SaveDirector>();
            DontDestroyOnLoad(sd);
        }
        ServiceLocator.EventBus.Publish(new GameStartedEvent());
        Debug.Log($"[GameManager] Awake in '{SceneManager.GetActiveScene().name}' | nextScene='{nextScene}'");
    }

    private void Start()
    {
        // اگر Save وجود دارد، قبل از لود صحنه بعدی اعمال شود
        ServiceLocator.Save.TryLoad();

        if (!string.IsNullOrEmpty(nextScene) &&
            SceneManager.GetActiveScene().name != nextScene)
        {
            Debug.Log($"[GameManager] Loading scene '{nextScene}'");
            SceneManager.LoadScene(nextScene);
        }
    }

    private void Update()
    {
        if (!ServiceLocator.IsReady) return;
        DriveSimulation(Time.deltaTime);
    }

    internal void DriveSimulation(float dt)
    {
        float gameMinutes = ServiceLocator.TimeService.Tick(dt);
        ServiceLocator.Needs.Tick(gameMinutes / 60f);

        // پل Needs -> حرکت (Energy<25 => سرعت -20%)
        _bridgeAcc += dt;
        if (_bridgeAcc >= 0.5f)
        {
            _bridgeAcc = 0f;
            if (_player == null) _player = FindFirstObjectByType<PlayerController>();
            if (_player != null)
                _player.SetSpeedMultiplier(ServiceLocator.Needs.MoveSpeedMultiplier);
        }
    }
}