using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.HighDefinition.WaterSurface;

public enum eShaderEffect
{
    DISOLVE,
    PHASE,
    ORIGIN,
    END,
}
public enum eFadeMode
{
    FADE_OUT,
    FADE_IN,
    FADE_STAY, 
    END
}

public class ShaderEffects : MonoBehaviour
{
    [SerializeField] private Material[] _materialPhase;
    [SerializeField] private Material[] _materialOrigin;
    [SerializeField] private Material[] _materialDisolve;
    private Renderer[] _renderers;
    //...
   
    //Hash
    private readonly int _splitValueHash= Shader.PropertyToID("_SplitValue");
    private readonly int _baseMapHash = Shader.PropertyToID("_Base_Map");

    // fade Mode Setting 

    private eFadeMode _fadeMode=eFadeMode.END;
    private eShaderEffect _nowShaderEffect = eShaderEffect.END; 
    bool _isFadeEffectDone = false;  public bool IsFadeEffectDone { get =>_isFadeEffectDone; }

    public void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();

    }
    public void SetMaterialAlpha(float alpha)
    {
        foreach (var renderer in _renderers)
        {
            renderer.material.SetFloat(_baseMapHash, alpha);
        }
    }

    public  void SetNowMarerial(eShaderEffect shaderEffect)
    {
        _nowShaderEffect=shaderEffect;
        switch (shaderEffect)
        {
            case eShaderEffect.DISOLVE:
                {
                    for(int i = 0;i<_renderers.Length;++i)
                    {
                        if (_renderers[i].transform.GetComponent<Poolable>() ||
                            _renderers[i].transform.parent.GetComponent<Poolable>()) continue;
                        _renderers[i].material= _materialDisolve[i];
                    }
                    
                    break;
                }
            case eShaderEffect.ORIGIN:
                {
                    for (int i = 0; i<_renderers.Length; ++i)
                    {
                        if (_renderers[i].transform.GetComponent<Poolable>() ||
                           _renderers[i].transform.parent.GetComponent<Poolable>()) continue;
                        _renderers[i].material= _materialOrigin[i];
                    }
                   
                    break;
                }
            case eShaderEffect.PHASE:
                {
                    for (int i = 0; i<_renderers.Length; ++i)
                    {
                        if (_renderers[i].transform.GetComponent<Poolable>() ||
                           _renderers[i].transform.parent.GetComponent<Poolable>()) continue;
                        _renderers[i].material= _materialPhase[i];
                    }

                    break;
                }
        }

    }
    public  void DoFade(float from, float to , float time, eFadeMode fadeMode=eFadeMode.FADE_OUT)
    {
        foreach (var renderer in _renderers)
        {

            string name = renderer.material.shader.name;
            bool bo = renderer.material.HasProperty(_splitValueHash);
            //  Debug.Log($"shader={_renderer.material.shader.name}, hasSplit={_renderer.material.HasProperty(_splitValueHash)}");


            _fadeMode = fadeMode;
            _isFadeEffectDone = false;
            iTween.ValueTo(gameObject, iTween.Hash(

                "from", from, "to", to, "time", time, "onupdatetarget", gameObject,
                "onupdate", "TweenOnUpdate", "oncomplete", "TweenOnComplete",
                "easetype", iTween.EaseType.easeInCubic
                ));
        }
    }
    public void TweenOnUpdate(float value)
    {
        foreach (var renderer in _renderers)
        {
            renderer.material.SetFloat("_SplitValue", value);
        }

    }
    public void TweenOnComplete()
    {
        _isFadeEffectDone = true; 
        //����
        switch(_nowShaderEffect)
        {
            case eShaderEffect.DISOLVE:
            {
                    break;
            }
            case eShaderEffect.PHASE:
                {
                    foreach (var renderer in _renderers)
                    {
                        renderer.material.SetFloat(_splitValueHash, -0.5f);
                    }
                    
                    break;
            }
        }
        switch (_fadeMode)
        {
            case eFadeMode.FADE_OUT:
            {
                    break;
            }
            case eFadeMode.FADE_IN:
            {
                    for(int i=0;i< _renderers.Length;++i)
                    {
                        _renderers[i].material=_materialOrigin[i];
                    }
                   
                    break;
            }
            case eFadeMode.FADE_STAY:
            {
                    break;
            }
            default:
            {
                    break; 
            }
        }

        
    }

    


}
