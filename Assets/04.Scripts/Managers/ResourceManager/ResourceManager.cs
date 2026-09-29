using UnityEngine;

public class ResourceManager
{
    //*Path Finder*//
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) ==typeof(GameObject))
        {
            string name = path;
            int idx = name.LastIndexOf('/');
            if (idx>=0)
            {
                name= name.Substring(1 + idx);
            }
            GameObject go = Managers.Pool.GetOriginal(name);
            if (null!= go)
                return go as T; 
        }

        //게임 오브젝트 타입 일라면, 풀에 이름을 키로 풀에 등록됬는지 확인하고 등록 되어있으면  반환 그렇지 않으면 리소스/ ..를 뒤져서 로드 

        return Resources.Load<T>(path);//해당 경로의 프리팹을 로드해서 반환
    }
    public GameObject Instantiate(string path, Transform parent = null, bool worldStayPos=true, int initialPoolingCount = 5)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");//rResources/Prefabs-...이후부터 알아서 분리 폴더안에 Prefabs폴더 만들고 거기에 프리팹 다 관리
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.LendPoolableTo(original, parent, worldStayPos, initialPoolingCount).gameObject;

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;

        // 경로를 넘겨준다 . 풀에,
        // 풀링가능하면 풀에서 가져옴 근데 일단 , 풀에 등록 안되있으면, 프리팹 가져와서 풀링할 수 있으면 얻어온 프리팹 등록하고 빌려옴 ,
        // 아니면 걍 반환 
    }

    
    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Poolable poolable = go.GetComponent<Poolable>(); //dynamic
        if (poolable != null)
        {
            Managers.Pool.GetBack(poolable);
            return;
        }

        Object.Destroy(go);
    }



}
