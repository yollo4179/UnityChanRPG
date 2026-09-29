using System;
using UnityEngine;

public class EventAceptableBase
{
    protected IDisposable m_EventHandle;
    public virtual bool isMet() { return false; }

    public virtual void Subscribe() { }/*이벤트 구독 (퀘스트 시작 시)*/
    public void UnSubscribe() { if (null ==m_EventHandle) return; m_EventHandle.Dispose(); }
}
