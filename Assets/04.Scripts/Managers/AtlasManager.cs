
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
            if(null ==m_Instance)//싱글톤으로서 이미 존재하면 걍 반환
            {
                m_Instance= UnityEngine.Object.FindFirstObjectByType<AtlasManager>();//활성화된 로드된 오브젝트가 같은타입으로 존재하는가? (싱글 톤 조건 )
                m_Instance.Init();
            }
            if(null ==m_Instance)
            {//디버깅용 name 
                var GO = new GameObject(nameof(AtlasManager));
                m_Instance=GO.AddComponent<AtlasManager>();
                m_Instance.Init();
            }
            return m_Instance;
        }
        
    }


    /*Atlas는 외부에 서 등록하겠다.*/
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
       
    }

    private void Init()
    {
        try
        {
            for (int i = 0; i<(int)eATLAS.ATLAS_END; ++i)
            {
                this.m_DicSprite[i]=new Dictionary<string, SpriteAtlas>();
            }
            foreach (var Atlas in m_Instance.m_arrAtlassed)
            {
                this.m_DicSprite[(int)Atlas.key].Add(Atlas.value.name, Atlas.value);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    /*주의 : 같은 그룹의 아틀라스에 같은 이름의 스프라이트가 중복되면 꼬입니다 +O(N).*/
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