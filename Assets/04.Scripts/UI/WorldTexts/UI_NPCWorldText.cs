using TMPro;
using UnityEngine;

public class UI_NPCWorldText : UI_WorldPart
{
    TextMeshProUGUI _textMeshPro;

    private void OnEnable()
    {
        

    }

    public void SetText(string Content, Color color , Vector2 size)
    {   if(null==_textMeshPro)
            _textMeshPro = GetComponent<TextMeshProUGUI>();

        _textMeshPro.text = Content;
        _textMeshPro.color = color;
        //_textMeshPro.rectTransform.sizeDelta = size;
    }
    public void SetPosition(Transform NPCTransform ,Vector3 offset)
    {
      
        // 나 렉트 트랜스폼 
        Camera cam = Camera.main;
        _offset = offset;
        Canvas canvas = Managers.UI.GetCachedUIByName("UI_SSDamageFonts_Canvas").GetComponent<Canvas>();
        gameObject.transform.SetParent(canvas.transform);
        _targetTransform = NPCTransform;

    }
}
