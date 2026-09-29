using NUnit.Framework;

using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UIElements;
using Unity.XR.CoreUtils;
public class DamageFontsDesc
{
    public  void AddFontInfo(int fontDamage, bool isCritical)
    {
        _fontDamages.Add(fontDamage);
        _isCriticalList.Add(isCritical);
    }
    public void SetPosition(Transform transform)
    {
        _transform =transform;
    }

    public List<int>  _fontDamages =new List<int>();
    public List<bool> _isCriticalList =new List<bool>();
    public void ClearFontsDesc()
    {
        _fontDamages.Clear();
        _isCriticalList.Clear();
        _transform=null;
    }
    public Transform _transform;
}

public class UI_SSDamageFonts : MonoBehaviour
{
    Camera _cam;

    public void Start()
    {
        _cam = Camera.main;
    }
    public void Update()
    {
        //카메라 컬링 영역외부에 있으면 투명하게 처리
        foreach(Transform child in transform)
        {
            UI_WorldPart uI_WorldPart = child.GetComponent<UI_WorldPart>();
            if (null==uI_WorldPart.TargetTransform) continue; 

            if(_cam.WorldToViewportPoint(child.GetComponent<UI_WorldPart>().TargetTransform.position+uI_WorldPart.Offset).z <0)
            {
                child.GetComponent<CanvasGroup>().alpha = 0f;
                
            }
            else
            {
                child.GetComponent<CanvasGroup>().alpha = 1f;
                child.transform.position
                        = Camera.main.WorldToScreenPoint(uI_WorldPart.TargetTransform.position+uI_WorldPart.Offset);
            }
        }
    }
    public void FontMaker(DamageFontsDesc dmgFontDesc)
    {
       // float extraSpacing = 10f;

        // 데미지 텍스트가 들어갈 캔버스 / 카메라 (반드시 세팅되어 있어야 함)
        Canvas canvas = GetComponent<Canvas>();
        Camera uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;


        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(0f,0.5f), 0f, UnityEngine.Random.Range(0f, 0.5f));

        for (int i = 0; i < dmgFontDesc._fontDamages.Count; ++i)
        {
            Poolable poolable = Managers.Pool.LendPoolableTo("DamageFont_TMP",transform);

            GameObject go = poolable.gameObject;
            // 텍스트 세팅 (여기서 text/스타일이 바뀐다고 가정)
            var ui = go.GetComponent<UI_DamageFont>();
           

            var tmp = go.GetComponent<TextMeshProUGUI>();
            var rect = tmp.rectTransform;

            // 1) 월드 위치가 카메라 앞에 있는지 확인
            Transform worldPosOrigin = dmgFontDesc._transform;            // 기준 월드 위치

            Vector3  offsetPos= Vector3.up*i*0.2f+randomOffset;
            




            ui.Init(dmgFontDesc._fontDamages[i], dmgFontDesc._isCriticalList[i], worldPosOrigin, offsetPos);
        }
    }
    public void RegisterToCavas(GameObject damageFont)
    {
        damageFont.transform.SetParent(this.transform); //캔버스의 부모로 , 폰트 위치 설정은 거기서 
    }    

    //여기서는 등록만 하고  데미지 폰트는 애니메이션 재생이 끝나면 GetBack(한다)Poolable.  
}
