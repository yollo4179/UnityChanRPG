using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Diagnostics;

public static class UIUtil
{
    public static T FindChild<T>(GameObject go,string name , bool recur=false )where T:UnityEngine.Object
    {
        if(null==go)return null;
        
        if(false ==recur)
        {
            for (int i = 0; i<go.transform.childCount; i++)
            {
                Transform transform  =go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name)|| transform.name== name)
                {
                    T component = transform.GetComponent<T>();
                    if(null != component)
                    {
                        return component;
                    }
                }
            }

        }
        else
        {
            foreach(T child in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name)|| child.name== name)
                {
                    return child; 
                }
            }
        }
        return null; 
    }
   
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
}

//Name은 너가 Reflection 이용해서 접근해야되니까 enum으로 이름 써놓고 바인딩해 oz? UI_Shop, UI_Inven이런데서
//https://ansohxxn.github.io/unity%20lesson%202/ch7-4/
public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    
    public abstract void Init(); 

    protected void Bind<T> (Type type) where T : UnityEngine.Object
    {

        string[] names = Enum.GetNames (type); //이넘에서 지정한 컴포넌트 개수만큼 할당, 해놓고...Type 키로 추가하고 일단 캐싱  같은 타입 같은 이름 가진 Object (컴이든 겜오든)

        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add (typeof(T), objects);//일단 딕셔너리에 캐싱( 같은 타입만 모아놓는다 .)

        //T 속하는 오브젝트들을 딕셔너리의 오브젝트 밸의 원소들에 하나한 추가
        for(int i=0;i< names.Length;++i)
        {
            if(typeof(T) == typeof(GameObject))
            {
                //GameObject
                objects[i]= UIUtil.FindChild(gameObject, names[i], true);
            }
            else
            {
                //Button , Image,....
                objects[i]= UIUtil.FindChild<T>(gameObject, names[i], true);
            }
            if(null == objects[i])
            {
                Debug.Log($"<color =#ff0000>Failed To VBind {names[i]}</color>");
            }
        }
    }

     //해당 이벤트 (클릭)이 발생 했을때 매개변수로 전달된 콜백 호출 ( GO의자식에 핸들러가 붙어있다면)
    public static void BindEvent(GameObject go, Action<PointerEventData> action,Define.UIEvent type= Define.UIEvent.Click)
    {


        UI_EventHandler evt = UIUtil.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                {
                    evt.OnClickHandler -=action; //이전꺼 지우고 새로 
                    evt.OnClickHandler += action;
                    break;
                }
            case Define.UIEvent.Drag:
                {
                    evt.OnDragHandler -= action; 
                    evt.OnDragHandler += action;
                    break; 
                }
        }

    }
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); } // 오브젝트로서 가져오기
    protected Text GetText(int idx) { return Get<Text>(idx); } // Text로서 가져오기
    protected Button GetButton(int idx) { return Get<Button>(idx); } // Button로서 가져오기
    protected Image GetImage(int idx) { return Get<Image>(idx); } // Image로서 가져오기

    public virtual void FixItem(in RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition3D = Vector3.zero;
        rect.anchoredPosition = Vector3.zero;
    }

    public virtual void FixDropItem(in RectTransform rect)
    {
   
    }

    public virtual void ActionBeforeDragEnter()
    {

    }
    public virtual void SetSlotKey(ItemDataStorage itemData) {}
    public virtual bool HasSameItem(ItemDataStorage itemData) { return false; }

    public virtual void EmptySlot(ItemDataStorage itemData=null) { }
    public virtual void EmptySlotKey(ItemDataStorage itemData) { }
}
