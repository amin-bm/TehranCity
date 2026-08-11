using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// دوربین ایزومتریک گیم‌پلی (ADR-001): زاویه ۴۵ درجه، زوم محدود، چرخش محدود.
/// </summary>
public class IsometricCameraController : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Transform pivot;

    [Header("Zoom (limited)")]
    public float minDistance = 6f;
    public float maxDistance = 14f;
    public float zoomSpeed = 1.5f;

    [Header("Rotate (limited)")]
    public float maxYaw = 60f;
    public float rotateSpeed = 0.12f;

    [Header("Pitch")]
    public float pitch = 45f;

    private CinemachineFollow _follow;
    private TehranCityInput _input;
    private float _distance = 10f;
    private float _yaw = 0f;

    private void Awake()
    {
        _follow = GetComponent<CinemachineFollow>();
        _input = new TehranCityInput();
        _input.Gameplay.Enable();
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Update()
    {
        // Pivot فقط موقعیت را دنبال می‌کند (بدون چرخش کاراکتر)
        if (player != null && pivot != null)
            pivot.position = player.position + Vector3.up * 1f;

        // Zoom با اسکرول ماوس
        float scroll = _input.Gameplay.Zoom.ReadValue<Vector2>().y;
        if (Mathf.Abs(scroll) > 0.01f)
            _distance = Mathf.Clamp(_distance - scroll * zoomSpeed, minDistance, maxDistance);

        // چرخش محدود با نگه‌داشتن دکمه راست ماوس + حرکت ماوس
        if (_input.Gameplay.RotateCamera.ReadValue<float>() > 0.5f)
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float dx = mouse.delta.ReadValue().x;
                _yaw = Mathf.Clamp(_yaw + dx * rotateSpeed, -maxYaw, maxYaw);
            }
        }

        // آفست ایزومتریک: pitch ثابت ۴۵ درجه + yaw محدود
        float h = _distance * Mathf.Sin(pitch * Mathf.Deg2Rad);
        float r = _distance * Mathf.Cos(pitch * Mathf.Deg2Rad);
        Vector3 offset = Quaternion.Euler(0f, _yaw, 0f) * new Vector3(0f, h, -r);
        _follow.FollowOffset = offset;
    }
}