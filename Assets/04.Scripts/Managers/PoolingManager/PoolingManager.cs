#if UNITY_EDITOR
using UnityEditor.EditorTools;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
#endif
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.Image;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;

public class Pool
{
    public GameObject _originalPrefab { get; private set; }
    public Transform Root { get; set; }

    Queue<Poolable> _watingQueue = new Queue<Poolable>();
    public bool _worldPosStays;

    public void Init(GameObject original, int count)
    {
        _originalPrefab = original;
        Root= new GameObject($"Root_{original.name}").transform;
        for (int i = 0; i<count; i++) {
            GetBack(Create());
        } 
        
    }
    public void GetBack(Poolable instance)
    {
        if (null ==instance) return;
        instance.transform.SetParent(Root); //대기중이라면 Root의 자식으로둔다.
        instance.gameObject.SetActive(false);
        instance._nowUsing=false; 
        _watingQueue.Enqueue(instance);
    }
    public Poolable Create() {
        GameObject go = Object.Instantiate<GameObject>(_originalPrefab);
        go.name = _originalPrefab.name; // 뒤에 붙는 (Clone) 없앰. 원본 프리팹과 이름 같게.
        return go.GetOrAddComponent<Poolable>();//컴포넌트로 추가
    }

    public Poolable LendPoolableTo(Transform parent) // 풀로부터 꺼내오기 (오브젝트 활성화)
    {
        Poolable poolable;

        if (_watingQueue.Count > 0) // 스택(대기상태)이 빈 크기 X 즉 하나라도 재활용 할 수 있는 애가 있다면 
            poolable = _watingQueue.Dequeue();
        else // 스택(대기상태)이 지금 비었다면 재활용 할 수 있는 애가 없으므로 새로 만들어야
            poolable = Create();
        poolable.Init();
        poolable.gameObject.SetActive(true);  // 활성화 (poolable.gameObject로 접근해서 활성화)

        // 부모가 없으면 Scene안에 ()Dont Destroy 밖을 벗어나도록 부모 설정
        if (parent == null)
            poolable.transform.SetParent( Managers.Scene.CurrentScene.transform);

        else 
             poolable.transform.SetParent( parent,worldPositionStays: _worldPosStays); // 파라미터로 받은 parent 를 부모로 설정
        poolable._nowUsing = true;

        return poolable;
    }
}
public class PoolingManager
{
   
   
    Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();
    Transform _root;
    public void Init() {
        _root  =new GameObject("{Pool_Root}").transform; //최상위 부모 노드
        Object.DontDestroyOnLoad(_root.gameObject);
    }
    public void GetBack(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if(false==_pools.ContainsKey(name))
        {
            //존재하지 않는키를(poolable)을 삽입하면 (등록안함)
            GameObject.Destroy(poolable.gameObject);
            return;
        }
        if (false ==_pools[name]._worldPosStays)
        {
            poolable.transform.SetParent(null, false);//중요 부모 기준에서는 로컬을 바꿔도 트랜스폼은 부모만큼 다시 스켈일링 됨
            poolable.transform.localPosition = Vector3.zero;
            poolable.transform.localRotation = Quaternion.identity;
            poolable.transform.localScale = Vector3.one;

            poolable.transform.position = Vector3.zero;
            poolable.transform.rotation = Quaternion.identity;
           
        }
        _pools[name].GetBack(poolable);
    }
    public void CreatePool(GameObject original,bool worldPosStays=true, int count=5)
    {
        Pool pool = new Pool();
        pool._worldPosStays= worldPosStays;
        pool.Init(original, count);
        pool.Root.transform.SetParent(_root, false);
        _pools.Add(original.name, pool);//프리팹 이름을 키로 사용한다. 

    }
    //처음 꺼내올때 Create Pool하므로 처음 설정 그대로
    public Poolable LendPoolableTo(GameObject original , Transform borrower, bool worldPosStays=true,int initialPoolingCount =5)
    {
        if(false == _pools.ContainsKey(original.name))
        {
            CreatePool(original,worldPosStays, initialPoolingCount);
        }

        return _pools[original.name].LendPoolableTo(borrower);
    }
    public Poolable LendPoolableTo(string key, Transform borrower, bool worldPosStays = true, int initialPoolingCount = 5)
    {

        if (false == _pools.ContainsKey(key))
        {
            var go = Managers.Resource.Instantiate(key, borrower,worldPosStays, initialPoolingCount);
            if (null==go|| go.GetComponent<Poolable>() == null)
            {
                Debug.Assert(true, $"There's no mapping Poolable Object with the key name{key}");
                return null;
            }
            return _pools[go.name].LendPoolableTo(borrower);//등록
        }

        return _pools[key].LendPoolableTo(borrower);
    }
    public GameObject GetOriginal ( string name )
    {
        if (_pools.ContainsKey(name) == false)
            return null;
        return _pools[name]._originalPrefab;
    }
    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);
        _pools.Clear();
    }


}
