using UnityEngine;

/// <summary>
/// کنترلر placeholder آرش - فاز ۱
/// حرکت نسبی به دوربین + چرخش نرم + گرانش.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float rotationSmooth = 12f;
    [SerializeField] private float gravity = -20f;

    private CharacterController _controller;
    private TehranCityInput _input;
    private float _vy;
    private float _speedMultiplier = 1f; // hook برای NeedsService (Energy<25 => 0.8)

    public Vector2 MoveInput { get; private set; } // برای Animator در آینده
    public bool IsMoving => MoveInput.sqrMagnitude > 0.01f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = new TehranCityInput();
    }

    private void OnEnable() => _input.Gameplay.Enable();
    private void OnDisable() => _input.Gameplay.Disable();

    /// <summary>بعداً NeedsService با این ضریب سرعت را کم می‌کند.</summary>
    public void SetSpeedMultiplier(float m) => _speedMultiplier = m;

    private void Update()
    {
        MoveInput = _input.Gameplay.Move.ReadValue<Vector2>();
        if (MoveInput.sqrMagnitude > 1f) MoveInput = MoveInput.normalized;

        Vector3 dir = CameraRelativeDir(MoveInput);

        // گرانش ساده
        if (_controller.isGrounded && _vy < 0f) _vy = -1f;
        _vy += gravity * Time.deltaTime;

        Vector3 velocity = dir * (walkSpeed * _speedMultiplier);
        velocity.y = _vy;
        _controller.Move(velocity * Time.deltaTime);

        // چرخش نرم به سمت حرکت
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSmooth * Time.deltaTime);
        }
    }

    private Vector3 CameraRelativeDir(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

        Camera cam = Camera.main;
        Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.transform.right : Vector3.right;

        forward.y = 0f;
        right.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }
}