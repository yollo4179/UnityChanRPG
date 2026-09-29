using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Player;
using TMPro;

namespace Player
{
    public enum ePlayerSlider
    {
        HEALTH,
        MANA,
        EXP,
        END
    }
    public class PlayerStatusUISet
    {
        public Action _handler;
        public Slider _slider;
        public Coroutine _co;
        public float _nowRatio = 1;
        public float _targetRatio = 1;
    }

}
public class UI_PlayerStatusBar : UI_Base
{
    public Player.PlayerStatusUISet[] _statusSet =new PlayerStatusUISet[(int)ePlayerSlider.END];
    TextMeshProUGUI _textLevel; 
    StatusScript _status; //v플레이어의 스크립트
    public enum Sliders
    {
        HealthBar_Slider,
        ManaBar_Slider,
        ExpBar_Slider
    }
    public enum TMPs
    {
        Level_TMP
    }

    public override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TextMeshProUGUI>(typeof(TMPs));
        _statusSet[(int)ePlayerSlider.HEALTH] = new Player.PlayerStatusUISet();
        _statusSet[(int)ePlayerSlider.MANA]= new Player.PlayerStatusUISet();
        _statusSet[(int)ePlayerSlider.EXP] = new Player.PlayerStatusUISet();

        _statusSet[(int)ePlayerSlider.HEALTH]._slider = Get<Slider>((int)Sliders.HealthBar_Slider);
        _statusSet[(int)ePlayerSlider.MANA]._slider = Get<Slider>((int)Sliders.ManaBar_Slider);
        _statusSet[(int)ePlayerSlider.EXP]._slider = Get<Slider>((int)Sliders.ExpBar_Slider);
        _textLevel = Get<TextMeshProUGUI>((int)TMPs.Level_TMP);
        LoadPlayerEXPAndLevel();
    }
    void Awake()
    {
        _status = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<StatusScript>();//otherPlayer //or fromManager
        Init();
        

        _status.OnPlayerStatusChangedEvent = UpdateSlider;
    }

    // Update is called once per frame
    

    IEnumerator DecreaseSlowly(ePlayerSlider statusType, float speed,int circle = 0)
    {
        while(circle>0)
        {
            _statusSet[(int)statusType]._nowRatio =Mathf.Clamp01(
                Mathf.MoveTowards(_statusSet[(int)statusType]._nowRatio, 1 , speed * Time.deltaTime)
            );
            if (Mathf.Abs(_statusSet[(int)statusType]._nowRatio - 1f) <= 0.01f)
            {
                 _statusSet[(int)statusType]._nowRatio = 0;
                --circle;
            }

            _statusSet[(int)statusType]._slider.value=_statusSet[(int)statusType]._nowRatio;
        }

        while (true)
        {
            _statusSet[(int)statusType]._nowRatio = Mathf.Clamp01(
                Mathf.MoveTowards(_statusSet[(int)statusType]._nowRatio, _statusSet[(int)statusType]._targetRatio, speed * Time.deltaTime)
            );
            _statusSet[(int)statusType]._slider.value = _statusSet[(int)statusType]._nowRatio;
           
            if (Mathf.Abs(_statusSet[(int)statusType]._nowRatio- _statusSet[(int)statusType]._targetRatio)<=0.01f)
            {
                _statusSet[(int)statusType]._co = null;
                yield break;
            }
            yield return null;
        }
    }
    public void LoadSlider(StatusScript targetStatus, ePlayerSlider statusType)
    {
        if (null == _statusSet[(int)statusType]._slider) return;
        _statusSet[(int)statusType]._nowRatio = targetStatus.CurHealth / targetStatus.MaxHealth;
        _statusSet[(int)statusType]._slider.value = _statusSet[(int)statusType]._nowRatio;
    }
    public void UpdateSlider(StatusScript targetStatus, ePlayerSlider statusType, float speed = 3f)
    {
        int circle = 0;
        switch (statusType)
        {
            case ePlayerSlider.HEALTH:
                {
                    Debug.Assert(0 != targetStatus.MaxHealth);
                    _statusSet[(int)statusType]._targetRatio = targetStatus.CurHealth / targetStatus.MaxHealth;
                    break;
                }
            case ePlayerSlider.MANA:
                {
                    Debug.Assert(0 != targetStatus.MaxMana);
                    _statusSet[(int)statusType]._targetRatio = targetStatus.CurMana / targetStatus.MaxMana;
                    break;
                }

            case ePlayerSlider.EXP:
                {
                    
                    PlayerInfo playerInfo = Managers.Player.PlayerInfo;
                    circle = playerInfo.Level -  Managers.Player.LevelPrev;
                    _statusSet[(int)statusType]._targetRatio = playerInfo.CurExp / (float)playerInfo.TotalExp;

                    _textLevel.text = "Level: "+playerInfo.Level.ToString();
                    break;
                }
        }


        if (Mathf.Abs(_statusSet[(int)statusType]._targetRatio - _statusSet[(int)statusType]._nowRatio) < 0f) return;
        if (null!=_statusSet[(int)statusType]._co)
        {
            CoroutineRunner.Instance.StopCoroutine(_statusSet[(int)statusType]._co); //Coroutine continue failed
            _statusSet[(int)statusType]._co=null;
        }

        _statusSet[(int)statusType]._co =CoroutineRunner.Instance.StartCoroutine(DecreaseSlowly(statusType,speed, circle));

        
    }

    public void LoadPlayerEXPAndLevel()
    {
        PlayerInfo playerInfo = Managers.Player.PlayerInfo;
        _textLevel.text = "Level: "+playerInfo.Level.ToString();
        _statusSet[(int)Player.ePlayerSlider.EXP ]._targetRatio = playerInfo.CurExp / playerInfo.TotalExp;
        _statusSet[(int)Player.ePlayerSlider.EXP]._nowRatio =_statusSet[(int)Player.ePlayerSlider.EXP]._targetRatio;
        _statusSet[(int)Player.ePlayerSlider.EXP]._slider.value = _statusSet[(int)Player.ePlayerSlider.EXP]._nowRatio;


    }
}
