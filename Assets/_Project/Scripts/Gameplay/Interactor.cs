using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// تشخیص نزدیک‌ترین IInteractable با اسکن شعاعی (مستقل از Rigidbody/Trigger).
/// </summary>
public class Interactor : MonoBehaviour
{
    [SerializeField] private float radius = 1.6f;
    [SerializeField] private float scanInterval = 0.1f;

    private readonly Collider[] _buffer = new Collider[16];
    private readonly List<IInteractable> _candidates = new();
    private IInteractable _current = null;
    private TehranCityInput _input;
    private InteractionUI _ui;
    private float _nextScan;
    private void OnDestroy() => _input.Gameplay.Disable();

    private void Awake()
    {
        _input = new TehranCityInput();
        _input.Gameplay.Enable();
        // Include: حتی اگر UI_Canvas با Clip Mode خاموش باشد، کامپوننت پیدا شود
        _ui = Object.FindFirstObjectByType<InteractionUI>(FindObjectsInactive.Include);
        if (_ui == null)
        {
            Debug.LogWarning("[Interactor] InteractionUI پیدا نشد! (Setup 8 را اجرا کرده‌ای؟)");
        }
        else
        {
            // UI ماندگار است؛ پرامپت صحنه قبلی را همان لحظه پاک کن
            _ui.HidePrompt();
        }
    }

    private void Update()
    {
        if (Time.time >= _nextScan)
        {
            _nextScan = Time.time + scanInterval;
            Scan();
        }

        if (_current != null && _input.Gameplay.Interact.triggered)
        {
            _current.OnInteract(gameObject);
        }
    }

    private void Scan()
    {
        // اگر UI هنگام Awake غیرفعال بوده (مثلاً Clip Mode)، بعداً دوباره جستجو کن
        if (_ui == null)
            _ui = Object.FindFirstObjectByType<InteractionUI>(FindObjectsInactive.Include);

        int count = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.2f, radius, _buffer);

        _candidates.Clear();
        for (int i = 0; i < count; i++)
        {
            if (_buffer[i].TryGetComponent<IInteractable>(out var c) && c.CanInteract)
                _candidates.Add(c);
        }

        IInteractable best = null;
        float bestDist = float.MaxValue;
        foreach (var c in _candidates)
        {
            float d = ((c as MonoBehaviour).transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c; }
        }

        if (!ReferenceEquals(best, _current))
        {
            _current = best;
            if (_ui != null)
            {
                if (_current != null) _ui.ShowPrompt(_current.Prompt);
                else _ui.HidePrompt();
            }
        }
    }
}