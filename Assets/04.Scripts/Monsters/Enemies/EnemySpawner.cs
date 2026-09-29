using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

[System.Serializable]
public class  MonsterDesc
{
    [SerializeField]private  int _numSkills; public int NumSkills { get => _numSkills; }
    EnemySpawner _spawner; public EnemySpawner Spawner { get => _spawner; set => _spawner = value; }
}
public class EnemySpawner : MonoBehaviour
{
    /*NavMesh And Cell Info*/
 
    [SerializeField] SpawnTile _spawnTilePrefab;
    [SerializeField] NavMeshSurface  _surface;
    /*MonsterInfo*/
    [SerializeField] GameObject _monsterPrefab; //아니명 배열로 랜덤 배치 //혹은 원하느 구역을 피킹으로 설정? 

    /*Debug*/
    [SerializeField] TextMeshPro _idxTextInstancePrefab;
    [Header("Monster Description")]
    [SerializeField] MonsterDesc _monsterDesc;

    Dictionary<int, SpawnTile> _dicSpawnCells=new();
    /*For Boss*/

    [Header("Boss Monster Description")]
    [SerializeField] bool _isBossSpawner= false; 
    [SerializeField] GameObject BossPoint;

    public SpawnTile GetSpawnTileByIndex(int idx) { _dicSpawnCells.TryGetValue(idx, out var GO); return GO; }

    public void SpawnBossMonsterCell()
    {
        if (false == _isBossSpawner) return;
        if (null == _spawnTilePrefab) return;
        if (null == _surface) return;
        if (null ==BossPoint) return;

        SpawnTile nowTile = null;
        nowTile =  Instantiate(_spawnTilePrefab, Vector3.zero, Quaternion.identity);

        float sizeX = _surface.size.x;
        float sizeZ = _surface.size.z;
        nowTile.SetCellPosition(BossPoint.transform.position);
        _dicSpawnCells.Add(0, nowTile);
        nowTile.SetMonster(Instantiate(_monsterPrefab, nowTile.transform.position, nowTile.transform.rotation), _monsterDesc);
        
    }
    public void SpawnMonsterCells()
    {
        if (null ==_spawnTilePrefab) return;
        if (null == _idxTextInstancePrefab) return;
        

        float sizeX = _surface.size.x;
        float sizeZ = _surface.size.z;
        Vector3 navMeshSurfaceLTPos = _surface.transform.position  + _surface.transform.rotation*(new Vector3(-sizeX*0.5f, 0, -0.5f*sizeZ)) ;
        Debug.DrawLine(_surface.transform.position, navMeshSurfaceLTPos,Color.bisque,float.PositiveInfinity);

        int maxNumRowCell = (int)sizeX /(int)_spawnTilePrefab.CellWidth;
        int maxNumColCell = (int)sizeZ /(int)_spawnTilePrefab.CellDepth;
        int numIdx = maxNumRowCell *maxNumColCell;

        SpawnTile nowTile = null;
        int Cnt = 2; 
        for (int i = 0; i<numIdx; ++i)
        {
            if(null==nowTile)
            nowTile =  Instantiate(_spawnTilePrefab,Vector3.zero,Quaternion.identity);
            nowTile.Init();
            if (false  == nowTile.TestValidityAndSet(new Vector2(sizeX, sizeZ), i, navMeshSurfaceLTPos,_surface))
            {

                nowTile.InitDebugSettings(Instantiate(_idxTextInstancePrefab));
                continue;
            }
            /*먼저 등록해서 Dictionary를 참조한다.*/
            _dicSpawnCells.Add(i, nowTile);
            nowTile.SetMonster(Instantiate(_monsterPrefab, nowTile.transform.position, nowTile.transform.rotation), _monsterDesc);
            nowTile.InitDebugSettings(Instantiate(_idxTextInstancePrefab));
            Cnt--;
            if (Cnt<=0) break;
            
             nowTile =null; 
        }


    }
    void Awake()
    {
        _monsterDesc.Spawner =this;

        switch (_isBossSpawner)
        {
            case false:
                SpawnMonsterCells();
                break;

            case true:
                SpawnBossMonsterCell();
                break;
        }
        


    }
  
    
    void Update()
    {
        
    }
}
