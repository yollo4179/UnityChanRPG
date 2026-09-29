using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;




public class UI_DamageFont : UI_WorldPart
{ 
    TextMeshProUGUI _text;
    RectTransform _rect; 
    bool isCritical;
   

    float _moveDistance = 1f;
    float moveTime = 1f;


    public void Awake()
    {
        _rect =GetComponent<RectTransform>();
        _text = GetComponent<TextMeshProUGUI>();
        _poolable =GetComponent<Poolable>();
    }
    public void Init(int fontDamage, bool isCritical,Transform monTrans,Vector3 offset)
    {
        _text.text = fontDamage.ToString();
        if(isCritical )
            _text.color = Color.red;
        else
        {
            _text.color = Color.yellow;
        }
        _targetTransform = monTrans;
        _offset=offset; 
        StartCoroutine(PlayFont());

    }
   
    private IEnumerator PlayFont()
    {
        Vector3 start = _offset+ Vector3.zero; //월드에서의 (위치)
        Vector3 end = start + Vector3.up*_moveDistance;

        float nowAcc = 0f;
        float percent = 0f;

        //float preY = float.PositiveInfinity ;

       // GameObject go = Managers.Resource.Instantiate("UI/WorldUI/Part/TargetHPBar", Managers.UI.GetUIByName("UI_SSTargetHPBars(Canvas)").transform).gameObject;
        //gameObject.transform.SetParent(Managers.UI.GetUIByName("UI_SSTargetHPBars(Canvas)").transform);
        while (1>=percent)
        {
            nowAcc += Time.deltaTime; //시간 누적
            percent = nowAcc/moveTime;

            

            //Vector3 world;
            //Vector3 sp =Camera.main.WorldToScreenPoint(world =Vector3.Lerp(_targetTransform.position+start, _targetTransform.position+ end, percent));
            //transform.position =sp;
          _offset= Vector3.Lerp( start, end, percent);

            
           // preY=sp.y;

             Color col = _text.color;
            col.a= Mathf.Lerp(1f, 0f, percent);
            _text.color = col;
            yield return null; 
        }

        Managers.Pool.GetBack(_poolable);
    
    }

}
