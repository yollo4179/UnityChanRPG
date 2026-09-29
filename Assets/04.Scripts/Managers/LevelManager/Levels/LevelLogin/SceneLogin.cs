using Newtonsoft.Json.Bson;
using NUnit.Framework;
using Unity.Behavior;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

public class SceneLogin : SceneBase
{

    EffectInstanceCreator effectInstanceCreator;
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.LoginScene;
    }

    private void Start()
    {
        effectInstanceCreator = GetComponent<EffectInstanceCreator>();
        effectInstanceCreator.AddInstances();
        RegisterpoolingUIs();
        RegisterHitBox();
        RegisterItems();
    }

    public void RegisterpoolingUIs()
    {
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/UI/WorldUI/Part/DamageFont_TMP"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/UI/WorldUI/Part/NPC_Text"));
        

    }
    public void RegisterHitBox()
    {
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/HitBox/HitBoxPlayer_Prefab"),false);
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/HitBox/HitBoxMonster_Prefab"),false);
    }

    public void RegisterItems()
    {
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Items/Prefabs/Coin"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Items/Prefabs/BoxOfPandora"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Items/Prefabs/Bottle_Mana"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Items/Prefabs/Bottle_Health"));


    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            StartGame(); 
        }
    }

    public void StartGame()
    {        
            Managers.Scene.LoadScene(Define.Scene.LoadingScene);

    }
    public void ExitGame()
    {

    }
}
