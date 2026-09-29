using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.EventSystems;
public class EventManager 
{
    
  
    Dictionary<Type, Delegate> m_EventHandlers=new(); //구독자가 지정한 이벤트 타입, 구독자의 함수

    public bool GetSubscribingData<T>(Action<T> FuncHandler, out Delegate _Del)
    {
        if (null ==FuncHandler)
        {
            Debug.Log("<color = #ff0000>발행 시 호출할 이벤트 객체를 넘겨받을 함수가 null</color>");
            _Del = null;
            return false;
        }
        Delegate Del = null;
        Type TypeOfEvent = typeof(T);
        
        m_EventHandlers.TryGetValue(TypeOfEvent, out Del);
        _Del =Del;
        if(_Del==null) m_EventHandlers.Remove(TypeOfEvent);

        return true;
    }
    private class EventHandler<T> : IDisposable /*Handle*/
    {
        private Action<T> m_FuncHandle;

        public EventHandler(Action<T> _FuncHandle)
        {
            m_FuncHandle = _FuncHandle;
        }
        public void Dispose() 
        {
            if (null ==m_FuncHandle) return; 
            Managers.Event.UnSubscribe<T>(m_FuncHandle);
            m_FuncHandle = null; 
        }
    }

    public IDisposable  Subscribe<T>(Action<T> FuncHandler)//T는 이벤트,이 이벤트를 파라미터로 받는 함수를 넘겨준다. 
  {
        Delegate Del = null;
        Type TypeOfEvent = typeof(T);
        GetSubscribingData<T>(FuncHandler, out Del);

        if (null == Del)
        {
            if (m_EventHandlers.ContainsKey(TypeOfEvent))
            {
                m_EventHandlers[TypeOfEvent]= Delegate.Combine(Del, FuncHandler);
            }
            else
            {
                m_EventHandlers.Add(TypeOfEvent, FuncHandler);
            }
        }
        else
        {
            m_EventHandlers[TypeOfEvent]= Delegate.Combine(Del, FuncHandler);//트리거 해시 체이닝
        }

        return new EventHandler<T>(FuncHandler); 
    }
    //구독 해제 , 객체 삭제 될때,비활성화 되기전에  정리.
    public void UnSubscribe<T>(Action<T> FuncHandler) 
    {
        Delegate Del = null;
        Type TypeOfEvent = typeof(T);
        GetSubscribingData<T>(FuncHandler, out Del);

        if (null == Del)
        {
            return; 
        }
        else
        {
            m_EventHandlers[TypeOfEvent] = Delegate.Remove(Del,FuncHandler);//트리거 해시 체이닝
        }
    }
    public void Publish<T>(T Event)
    {
        var TypeOfEvent = typeof(T);
        m_EventHandlers.TryGetValue(TypeOfEvent, out var Del);
        if (null ==Del) {
            Debug.Log($"<color=#ff0000>{TypeOfEvent.Name}타입으로 등록된 이벤트가 없습니다.</color>"); 
            return;
        }
        (Del as Action<T>)?.Invoke(Event); //이벤트를 발행한다.
        /*이벤트 내 구조체 정보는 함수에서 각자 판단해서 로직을 수행하자.*/
    }
    public  void Clear()
    {
       
        m_EventHandlers.Clear();

    }

}
