
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshUtil : MonoBehaviour
{
    public static NavMeshUtil Instance { 
        get
        {
            if(null ==m_Instance)
            {
                m_Instance = FindFirstObjectByType<NavMeshUtil>();
                
            }
            if(null ==m_Instance)
            {
                var GO = new GameObject(nameof(AtlasManager));
                m_Instance=GO.AddComponent<NavMeshUtil>();
            }
            return m_Instance;
        }
            
    }
    private static NavMeshUtil m_Instance =null;

        public static NavMeshUtil  GetInstance()
        {
            return Instance; 

        }
        

    [SerializeField] private NavMeshSurface _surface;
    public NavMeshSurface Surface { get => _surface; }

    public void RebuildSurface()
    {
        if (null!=_surface)
        {
            _surface.layerMask          = LayerMask.GetMask("Terrain")|LayerMask.GetMask("Enemies");
            _surface.ignoreNavMeshAgent = true;
            _surface.BuildNavMesh();
        }
    }

}
