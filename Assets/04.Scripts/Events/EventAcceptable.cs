using System;
using UnityEngine;

public class EventAcceptable 
{
    protected IDisposable m_EventHandle;
    public virtual bool isMet() { return false; }

    protected virtual void Subscribe() {  }/*이벤트 구독 (퀘스트 시작 시)*/
    protected void UnSubscribe() {
        if (null ==m_EventHandle) 
            return; 
        m_EventHandle.Dispose(); 
    }
}
