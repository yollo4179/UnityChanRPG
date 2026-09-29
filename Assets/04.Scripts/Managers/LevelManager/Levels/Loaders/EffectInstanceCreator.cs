using System.Threading;
using UnityEngine;
using static PlayerControllerCom;

public class EffectInstanceCreator : InstanceCreator
{
    public override void AddInstances()
    {
        AddPoolingInstances();
    }

    public override void AddPoolingInstances()
    {
        AddPlayerEffect();
        AddBaseEffect();
        AddMonsterEffect();

    }
    public void AddBaseEffect()
    {
        //HitEffect
    }
    public void AddMonsterEffect()
    {
        
    }
    public void AddPlayerEffect()
    {
     
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Col_Base"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Col_Blue"));
        //Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Dance_Blue"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Dance_Green"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Ink"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Cosmos"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Ink_verticalVariant"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Multi_Blue"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Row_Base"));
        //Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Row_Red"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_RowBlue"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Slashes/Slash_Water"));

        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Sparks/Sparks_red"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Sparks/Sparks_blue"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Sparks/Sparks_explode_white"));

        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Sparks/Holy_hit"));
        /**Chain Strike*/
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Skills/ChainLightning"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Hit/Electro_hit"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Projectiles/ChainLightningProj_Prefab"));
        /*Reinforce Effects*/
     


        



    }

}
