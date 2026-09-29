using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
public class WeaponEffectManager
{
    public Dictionary<ELEMENT, ReinforceEffect> _dicEffect;
    public Dictionary<string, Poolable> _reinforceEffects;

    public ReinforceInfo _nowEffect;
    public void Init()
    {
        var catalogs = Resources.LoadAll<ReinforceEffect>("Data/ScriptableObjects/ReinforceEffect");
        _dicEffect = catalogs.ToDictionary(X=>X.Element);// SelectMany로 평탄화 ->하나의 반복자로 ()
        /*Register Pooling */
        foreach (var effectInfo in  _dicEffect)
        {
            foreach(var effect in effectInfo.Value.ReinforceEffectPrefabs)
            {
                Managers.Pool.CreatePool(effect.EffectPrefab,true,5);
            }

            
        }
        
        _reinforceEffects = new Dictionary<string, Poolable>();
    }

    public ReinforceEffect GetEffect(ELEMENT element )
    {
        return _dicEffect[element]; 
    }
    public void ActivateEffect(ELEMENT element,int level,string instanceID ,MeshRenderer mr ,Transform parent)
    {
        var effects = _dicEffect[element].ReinforceEffectPrefabs;
        _nowEffect =null;
        foreach (var effect in effects) {
            if (effect == null) continue;
            if (effect.Level > level) break;
            _nowEffect=effect;
        }
        if (null ==_nowEffect) return;
        Poolable handle=  Managers.Pool.LendPoolableTo(_nowEffect.EffectPrefab.name,null);
        SetWeaponMesh(handle,mr);
        handle.transform.SetParent(parent);
        _reinforceEffects.Add(instanceID, handle);
    }
    public void InActivateEffect(string instanceID)
    {
        if ( !_reinforceEffects.ContainsKey(instanceID)) return ;
        Poolable handle = _reinforceEffects[instanceID];
        ClearWeaponMesh(handle);
        Managers.Pool.GetBack(handle);

        _reinforceEffects.Remove(instanceID);
    }
    public void ClearEffect()
    {
        if (_nowEffect!=null)
        {

            _nowEffect.EffectPrefab.SetActive(false);
        }
    }

    public void SetWeaponMesh(Poolable handle ,MeshRenderer meshRenderer)
    {
        var particleSystems = handle.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var particleSystem in particleSystems)
        {
            var shape = particleSystem.shape; // ★ 모듈을 로컬 변수로 받기 (중요)
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer; // 모드 지정
            shape.meshRenderer = meshRenderer;                      // 씬의 MeshRenderer 할당
            shape.skinnedMeshRenderer = null;
        }
    }
    public void ClearWeaponMesh(Poolable handle)
    {
        var particleSystems = handle.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var particleSystem in particleSystems)
        {
            var shape = particleSystem.shape; // ★ 모듈을 로컬 변수로 받기 (중요)
            shape.enabled = false;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer; // 모드 지정
            shape.meshRenderer = null;                      // 씬의 MeshRenderer 할당
            shape.skinnedMeshRenderer = null;
        }
    }
}
