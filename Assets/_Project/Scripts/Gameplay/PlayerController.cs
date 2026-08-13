using UnityEngine;

/// <summary>
/// کنترلر placeholder آرش - فاز ۱ (نسخه Canonical PC-10.6)
/// </summary>
public class PlayerController : MonoBehaviour
{
    private const string BUILD_TAG = "PC-10.7";

    [Header("Move")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float rotationSmooth = 12f;
    [SerializeField] private float gravity = -20f;

    [HideInInspector] public bool inputLocked;

    private CharacterController _controller;
    private TehranCityInput _input;
    private float _vy;
    private float _speedMultiplier = 1f;

    private Vector3 _safePos; private float _safeY; private bool _hasSafe;
    private Vector3 _intendedSpawn; private float _lockUntil = -1f;
    private Vector3 _lastPos; private bool _hasLast;

    private readonly RaycastHit[] _rayBuffer = new RaycastHit[8];

    public Vector2 MoveInput { get; private set; }
    public bool IsMoving => MoveInput.sqrMagnitude > 0.01f;
    private bool _lockWarned;

    private void Awake()
    {
        Debug.Log($"[Player] BUILD={BUILD_TAG} id={GetInstanceID()} scene='{gameObject.scene.name}'");

        _lockWarned = false;
        // خودتمیزی داده صحنه: Rigidbody/CapsuleCollider سرگردان حذف شود
        var rb = GetComponent<Rigidbody>();
        if (rb != null) { Debug.LogWarning($"[Player] Rigidbody سرگردان روی Player حذف شد (scene='{gameObject.scene.name}')"); Destroy(rb); }
        var col = GetComponent<CapsuleCollider>();
        if (col != null) { Debug.LogWarning("[Player] CapsuleCollider سرگردان حذف شد."); Destroy(col); }

        _controller = GetComponent<CharacterController>();
        _input = new TehranCityInput();

        if (!string.IsNullOrEmpty(SceneTransition.PendingSpawn))
        {
            var spawnName = SceneTransition.PendingSpawn;
            var sp = GameObject.Find(spawnName);
            if (sp == null)
                sp = FindSpawnInActiveScene(spawnName)?.gameObject;

            if (sp != null)
                transform.position = sp.transform.position + Vector3.up * 1.05f;
            else
                Debug.LogWarning($"[Player] PendingSpawn '{spawnName}' پیدا نشد!");

            Debug.Log($"[Player] Spawn => '{spawnName}' found={sp != null} pos={transform.position} id={GetInstanceID()}");
            SceneTransition.PendingSpawn = "";

            _intendedSpawn = transform.position;
            _lockUntil = Time.time + 0.75f;
        }

        GroundSnap();
        MarkSafe();
        _intendedSpawn = transform.position;
        _lastPos = transform.position; _hasLast = true;

        // فیکس ریشه‌ای «برگشت یک‌فریمی به موقعیت editor»:
        // بدنه فیزیک CC با موقعیت asset صحنه ساخته شده؛ با بازسازی بدنه + SyncTransforms
        // موقعیت جدید همین حالا به دنیای فیزیک push می‌شود تا در اولین گام فیزیک
        // transform به مقدار قدیمی برنگردد.
        _controller.enabled = false;
        _controller.enabled = true;
        Physics.SyncTransforms();
        _controller.Move(Vector3.zero);
    }

    private void OnEnable() => _input.Gameplay.Enable();
    private void OnDisable() => _input.Gameplay.Disable();

    public void SetSpeedMultiplier(float m) => _speedMultiplier = m;

    private void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);

        WatchJump();

        MoveInput = inputLocked ? Vector2.zero : _input.Gameplay.Move.ReadValue<Vector2>();
        if (MoveInput.sqrMagnitude > 1f) MoveInput = MoveInput.normalized;

        Vector3 dir = CameraRelativeDir(MoveInput);

        bool nearGround = false;
        if (GroundRay(transform.position + Vector3.up * 2f, 12f, out RaycastHit hit))
        {
            float feetY = transform.position.y - 1f;

            if (feetY < hit.point.y - 0.02f)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + 1f, transform.position.z);
                _vy = 0f; MarkSafe();
            }
            else if (feetY <= hit.point.y + 0.15f && _vy <= 0f)
            {
                nearGround = true; _vy = 0f; MarkSafe();
            }
        }

        if (!nearGround) _vy += gravity * dt;

        Vector3 velocity = dir * (walkSpeed * _speedMultiplier);
        velocity.y = _vy;
        _controller.Move(velocity * dt);

        // قفل اسپان: تا 0.75s بعد از انتقال، هر جابه‌جایی >2.5m یعنی دخالت خارجی؛ برگردان
        if (Time.time < _lockUntil && (transform.position - _intendedSpawn).sqrMagnitude > 6.25f)
        {
            transform.position = _intendedSpawn;
            _vy = 0f;
            if (!_lockWarned)
            {
                _lockWarned = true;
                Debug.LogWarning("[Player] SpawnLock: یک پرش خارجی مسدود شد.");
            }
        }

        if (_hasSafe && transform.position.y < _safeY - 2.5f)
        {
            transform.position = _safePos; _vy = 0f;
            Debug.LogWarning("[Player] Fall rescue!");
        }

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSmooth * dt);
        }

        _lastPos = transform.position; _hasLast = true;
    }

    private string intended() => _intendedSpawn.ToString("F2");

    /// <summary>ضبط‌کننده پرش: هر جابه‌جایی ناگهانی >1.5m بین دو فریم را با شماره فریم لاگ می‌کند.</summary>
    private void WatchJump()
    {
        if (_hasLast && (transform.position - _lastPos).sqrMagnitude > 2.25f)
            Debug.LogWarning($"[Player][Jump] frame={Time.frameCount} t={Time.time:F2} from={_lastPos} to={transform.position}");
    }

    private void GroundSnap()
    {
        if (GroundRay(transform.position + Vector3.up * 2f, 10f, out RaycastHit hit))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 1.02f, transform.position.z);
            _vy = 0f;
        }
        else Debug.LogWarning("[Player] GroundSnap: هیچ کولایدری زیر بازیکن پیدا نشد!");
    }

    private void MarkSafe() { _safePos = transform.position; _safeY = transform.position.y; _hasSafe = true; }

    private bool GroundRay(Vector3 origin, float maxDistance, out RaycastHit bestHit)
    {
        bestHit = default;
        int count = Physics.RaycastNonAlloc(origin, Vector3.down, _rayBuffer, maxDistance, ~0, QueryTriggerInteraction.Ignore);
        float bestDist = float.MaxValue; bool found = false;
        for (int i = 0; i < count; i++)
        {
            var h = _rayBuffer[i];
            if (h.collider.transform == transform || h.collider.transform.IsChildOf(transform)) continue;
            if (h.distance < bestDist) { bestDist = h.distance; bestHit = h; found = true; }
        }
        return found;
    }

    private static Transform FindSpawnInActiveScene(string spawnName)
    {
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var t = FindInChildren(root.transform, spawnName);
            if (t != null) return t;
        }
        return null;
    }

    private static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform child in t) { var r = FindInChildren(child, name); if (r != null) return r; }
        return null;
    }

    private Vector3 CameraRelativeDir(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;
        Camera cam = Camera.main;
        Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.transform.right : Vector3.right;
        forward.y = 0f; right.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
        forward.Normalize(); right.Normalize();
        return (forward * input.y + right * input.x).normalized;
    }
}