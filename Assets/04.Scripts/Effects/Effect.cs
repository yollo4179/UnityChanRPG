using System.Collections;
using UnityEngine;
public class ParticleDesc
{
    bool isLoop = false; 
}

public class Effect : MonoBehaviour //poolable 은 무조건 가져야함
{
    ParticleSystem _particleSystem;
    ParticleDesc _particleDesc = new ParticleDesc();
    Poolable _pool;

    public void Start()
    {
        Init(); 
    }
    public void Init()
    {
        _pool  = GetComponent<Poolable>();
        Debug.Assert(_pool != null);
        _particleSystem = GetComponent<ParticleSystem>();   
    }
    public void SetParticleDesc(ParticleDesc particleDesc)
    {
        _particleDesc = particleDesc;
    }
    public void ParticleOn()
    {
        Init();
        _particleSystem.Play();
        StartCoroutine(ParticleEndAfterPlay());
        
    }
    IEnumerator ParticleEndAfterPlay()
    {
        yield return new WaitUntil(()=> { return _particleSystem.isStopped; });

        Managers.Pool.GetBack(_pool);
        yield break; 
    }

}
