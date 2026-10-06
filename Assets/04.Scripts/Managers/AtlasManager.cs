
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public enum eATLAS
{
    ATLAS_ITEMIST_INGREDIENT,
    ATLAS_ITEMLIST_CONSUMABLE,
    ATLAS_ITEMLIST_EQUIPMENT,
    ATLAS_UI,
    ATLAS_END,
}
[System.Serializable]
public struct AtlasEntry
{
    public eATLAS key;
    public SpriteAtlas value;
}

public class AtlasManager : MonoBehaviour
{
    private AtlasManager() { }
    private static AtlasManager m_Instance= null ;

    /*Mono Singleton*/

    public static AtlasManager Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = UnityEngine.Object.FindAnyObjectByType<AtlasManager>();
            if (m_Instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/AtlasManager");
                if (prefab == null)
                    throw new InvalidOperationException("Missing Prefabs/AtlasManager resource.");
                m_Instance = Instantiate(prefab).GetComponent<AtlasManager>();
            }
            m_Instance.Init();
            return m_Instance;
        }
    }

    private bool _initialized;

    [SerializeField]
    public AtlasEntry[] m_arrAtlassed;
    Dictionary<string, SpriteAtlas>[] m_DicSprite = new Dictionary<string, SpriteAtlas>[(int)eATLAS.ATLAS_END];
    //Dictionary<(eATLAS, string), Sprite> Cache;

    public static AtlasManager GetInstance()
    {
   
      return Instance;
    }
    private void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        m_Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void OnDestroy()
    {
        if (m_Instance == this) m_Instance = null;
    }

    private void Init()
    {
        if (_initialized) return;
        for (int i = 0; i < (int)eATLAS.ATLAS_END; i++)
            m_DicSprite[i] = new Dictionary<string, SpriteAtlas>();

        if (m_arrAtlassed != null)
        {
            foreach (AtlasEntry atlas in m_arrAtlassed)
            {
                int key = (int)atlas.key;
                if (atlas.value == null || key < 0 || key >= m_DicSprite.Length) continue;
                m_DicSprite[key][atlas.value.name] = atlas.value;
            }
        }
        _initialized = true;
    }

    public SpriteAtlas GetAtlasByName(eATLAS ATLASTYPE, string AtlasName)
    {
        try
        {
            if (this.m_DicSprite[(int)ATLASTYPE].ContainsKey(AtlasName))
            {

                return this.m_DicSprite[(int)ATLASTYPE][AtlasName];
            }
        }
        catch (NullReferenceException exception)
        {
            Debug.LogError("그런 아틀라스가 없다");
            Debug.LogError(exception);
        }
        return null;

    }
    public Sprite GetSpriteByName(eATLAS ATLASTYPE,string SpriteName)
    {
        Sprite RetVal=null;
        try
        {
            foreach (var AtlasPair in this.m_DicSprite[(int)ATLASTYPE])
            {
                if (null != (RetVal =AtlasPair.Value.GetSprite(SpriteName+"(Clone)")))
                {
                    break;
                }
                if (null != (RetVal =AtlasPair.Value.GetSprite(SpriteName)))
                {
                    break;
                }


            }
        }
        catch (NullReferenceException exception)
        {
            Debug.LogError("그런 아틀라스가 없다");
            Debug.LogError(exception);
        }


        return RetVal;
    }
    /*DB에 스프라이트가 속한 아틀라스 그룹이름도 저장할때 o(1)*/
    public Sprite GetSpriteByName(eATLAS ATLASTYPE, string AtlasName, string SpriteName)
    {
        try
        {
            if (this.m_DicSprite[(int)ATLASTYPE].ContainsKey(AtlasName))
            {

                return this.m_DicSprite[(int)ATLASTYPE][AtlasName].GetSprite(SpriteName);
            }
        }
        catch (NullReferenceException exception)
        {
            Debug.LogError("그런 아틀라스가 없다");
            Debug.LogError(exception);
        }
        return null;
    }
}