using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public abstract class SceneBase : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Static;


    private void Awake()
    {
        Init();

    }

    protected virtual void Init(){
        Object go = GameObject.FindFirstObjectByType<EventSystem>();
        if (null==go)
            Managers.Resource.Instantiate("UI/EventSystem").name ="@EventSystem";

    }
    protected virtual void InitUIs() { 
        
    }
    protected virtual void InitSceneEffects() { }
    public  virtual void Clear() { }

}
