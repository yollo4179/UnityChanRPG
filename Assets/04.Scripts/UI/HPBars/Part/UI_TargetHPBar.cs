using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_TargetHPBar : MonoBehaviour
{
    Slider _slider;
    float _targetRatio=1;
    float _nowRatio = 1;
    Coroutine _co = null; 
    public  bool IsCoroutineOn { get { return _co != null; } }
    public void Start()
    {
        _slider = GetComponent<Slider>();
    }
    IEnumerator DecreaseSlowly(float speed)
    {
        while (true)
        {
            _nowRatio = Mathf.Clamp01(Mathf.MoveTowards(_nowRatio, _targetRatio, speed * Time.deltaTime));
            _slider.value = _nowRatio;
            if (Mathf.Abs(_nowRatio- _targetRatio)<0.01f)
            {
                _co = null;
                yield break;
            }
            yield return null; 
        }
    }
    public void LoadSlider(StatusScript targetStatus)
    {
        float fullHp = targetStatus.MaxHealth;
        float nowHp = targetStatus.CurHealth;
        _slider.value = nowHp / fullHp;
    }
    public void UpdateSlider(StatusScript targetStatus,float speed =3f)
  {

        if (null ==_slider) return;
        float fullHp = targetStatus.MaxHealth;
        float nowHp = targetStatus.CurHealth;

        if (0 == fullHp) return; 

         _targetRatio = nowHp / fullHp;

        if (Mathf.Abs(_targetRatio - _nowRatio)<0.001f) return;

            if (null != _co)
            StopCoroutine(_co);
       CoroutineRunner.Instance.StartCoroutine(DecreaseSlowly(speed));

  }
}
