using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UI_TargetHPBar : MonoBehaviour
{
    private Slider _slider;
    private float _targetRatio = 1f;
    private float _nowRatio = 1f;
    private Coroutine _co;
    public bool IsCoroutineOn => _co != null;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private IEnumerator DecreaseSlowly(float speed)
    {
        while (Mathf.Abs(_nowRatio - _targetRatio) > 0.001f)
        {
            _nowRatio = Mathf.MoveTowards(_nowRatio, _targetRatio, speed * Time.deltaTime);
            _slider.value = _nowRatio;
            yield return null;
        }
        _nowRatio = _targetRatio;
        _slider.value = _nowRatio;
        _co = null;
    }

    public void LoadSlider(StatusScript targetStatus)
    {
        if (_slider == null) _slider = GetComponent<Slider>();
        StopAnimation();
        _targetRatio = HealthRatio(targetStatus);
        _nowRatio = _targetRatio;
        _slider.value = _nowRatio;
    }

    public void UpdateSlider(StatusScript targetStatus, float speed = 3f)
    {
        _targetRatio = HealthRatio(targetStatus);
        StopAnimation();
        if (!isActiveAndEnabled || speed <= 0f)
        {
            _nowRatio = _targetRatio;
            if (_slider != null) _slider.value = _nowRatio;
            return;
        }
        if (Mathf.Abs(_targetRatio - _nowRatio) <= 0.001f)
        {
            _nowRatio = _targetRatio;
            _slider.value = _nowRatio;
            return;
        }
        _co = StartCoroutine(DecreaseSlowly(speed));
    }

    private static float HealthRatio(StatusScript status)
    {
        return status != null && status.MaxHealth > 0f
            ? Mathf.Clamp01((float)status.CurHealth / status.MaxHealth) : 0f;
    }

    private void StopAnimation()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }

    private void OnDisable()
    {
        StopAnimation();
    }
}
