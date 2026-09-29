using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EQSQuerierBehavior_WanderAroundCell : MonoBehaviour, ISpawnerConsumer, IIndexConsumer
{
    //ActionNode에서 Injection   
    EnemySpawner _enemySpawner;
    
    int _cellIndex = -1;
     EQSPoints _points;

    EQSPoint _goalPoint = null; 
     EQSQuery _query;
     EQSQuerier _querier;
     NavMeshAgent _agent;
     

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
       
        _query = GetComponent<EQSQuery>(); 
        _querier = GetComponent<EQSQuerier>();

    }

    
    public EQSPoint GetBestPoint()
    {
        EQSContext context = new EQSContext
        {
            target              = EQSManager.GetInstance().GetTarget().transform,
            allQueriers         = EQSManager.GetInstance().GetAllQueriers(),
            points              = _points,
            currentRingIndex    = 0,
        };
       // EQSPoint _point = null;
        return _points.GetAllPoints()[UnityEngine.Random.Range(0, 9)];
       // return (_point =_query.ExecuteQuery(_querier, _points, context));
    }

    public void InjectSpawner(EnemySpawner spawner)
    {
        if (null!= _enemySpawner) return;
        _enemySpawner =spawner;
        if (-1 !=_cellIndex)
        {

            _points = _enemySpawner.GetSpawnTileByIndex(_cellIndex).Points;
        }
        else
            Debug.Log("<color =#ff0000>Spawner 주입 실패</color>");
    }

    public void SetIndex(int idx)
    {
        _cellIndex = idx; //GetComponent<SpawnerTileIndex>().TileIndex;
    }
}
