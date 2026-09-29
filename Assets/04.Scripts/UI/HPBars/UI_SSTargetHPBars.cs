using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using System;
public class UI_SSTargetHPBars :MonoBehaviour
{
    Camera _cam;
    List <GameObject>  _targetsHPBarList  = new List<GameObject>();
    List<Transform>   _targetsObjectList = new List<Transform>();
    List<bool>        _hpBarVisible = new List<bool>();
    string poolingKeyHPBar = "UI/WorldUI/Part/TargetHPBar";
    


    public void Start()
    {
        _cam = Camera.main;
        GameObject[] objects  =GameObject.FindGameObjectsWithTag("Enemies");
       

        List<GameObject> _targetlist = new List<GameObject>();
        _hpBarVisible = new List<bool>();

        foreach (var obj in objects)
        {
            _targetsObjectList.Add(obj.transform); //풀링으로 처리한다. 
            _targetsHPBarList.Add(Managers.Resource.Instantiate(poolingKeyHPBar, this.transform).gameObject); //풀링으로 처리한다. 
            _hpBarVisible.Add(true);
        }
    }
    private void UpdateHPBars()
    {
        for (int i = 0; i< _targetsHPBarList.Count; i++)
        {

            //if (null != _targetsHPBarList[i])
            //{
            //    if (true == _targetsObjectList[i].GetComponent<StatusScript>().IsDead)
            //    {
            //        Poolable poolable = _targetsHPBarList[i].GetComponent<Poolable>();
            //        Managers.Pool.GetBack(poolable);
            //        _targetsHPBarList[i] = null;
            //    }
            //}
            //else
            //{
            //    if (false == _targetsObjectList[i].GetComponent<StatusScript>().IsDead)
            //    {
            //      _targetsHPBarList[i] =Managers.Pool.LendPoolableTo(poolingKeyHPBar, this.transform).gameObject;

            //    }
            //}
            //if (null ==_targetsHPBarList[i]) continue;  //block

            StatusScript status = _targetsObjectList[i].GetComponent<StatusScript>();
            if ( true==_hpBarVisible[i]  &&true ==status.IsDead)
            {

                _targetsHPBarList[i].SetActive(false);
                _hpBarVisible[i] = false;
            }
            if (false==_hpBarVisible[i]  &&false == status.IsDead)
            {
                _targetsHPBarList[i].SetActive(true);
                _hpBarVisible[i] = true;
                _targetsHPBarList[i].GetComponent<UI_TargetHPBar>().LoadSlider(status);
            }
            Vector3 wordlPos = _targetsObjectList[i].transform.position+Vector3.up;
            Vector3 viewPos = _cam.WorldToViewportPoint(wordlPos);
            if (viewPos.z>0)
            {
                _targetsHPBarList[i].transform.position = _cam.WorldToScreenPoint(_targetsObjectList[i].transform.position + Vector3.up);
                Action<StatusScript, float> handler = _targetsHPBarList[i].GetComponent<UI_TargetHPBar>().UpdateSlider;

                status.OnHealthChangedEvent -= handler;
                status.OnHealthChangedEvent += handler;
            }
        }
    }


    public void Update()
    {
        UpdateHPBars();
    }

}
