using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using Unity.AI.Navigation;
using Unity.Behavior;
/*Base on EQS point*/
public class SpawnTile : MonoBehaviour
{

    protected GameObject _monsterEnemy;
  
    

    [SerializeField] protected float _cellDepth = 5f;
    [SerializeField] protected float _cellWidth = 5f;
    float _cellHeight = 5f;
    public float CellDepth { get => _cellDepth; }
    public float CellWidth { get => _cellWidth; }

    EQSPoints _points ;
    public EQSPoints Points{get=>_points;} 

    /*Wandering*/
    [SerializeField] List<MonoBehaviour> _tests;



    /*Spawner에서 주입받는다.()외부에서*/
    TextMeshPro _IndexTextInstance;
  
    bool _isCellFixed;
    

    LayerMask _targetMask;
    bool isInitialized = false; 
    
    private Vector3[] _wayPoints;/*필수 */
    private Vector3[] _wayPointsMask;
    protected int _cellIndex = -1;/*필수 */
    protected Vector3 _cellPosition;/*필수 */

    public void SetCellPosition(Vector3 cellPosition) {  _cellPosition = cellPosition; }
    #region   유틸리티
    public virtual void SetMonster(GameObject _enemyObject, MonsterDesc monsterDesc)
    {
        _monsterEnemy =_enemyObject; 
        _monsterEnemy.transform.position = _cellPosition;

        int a = -1; 
       bool isSucceeded= _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.SPAWNER, monsterDesc.Spawner.gameObject);
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.CELL_INDEX, _cellIndex);
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.ANIMATOR, _enemyObject.GetComponentInChildren<Animator>());
        _enemyObject.GetComponent<BehaviorGraphAgent>().SetVariableValue(BlackboardKeys.SKILL_NO, a=UnityEngine.Random.Range(0, monsterDesc.NumSkills)); //0,1,2

        _enemyObject.GetComponent<BehaviorGraphAgent>().BlackboardReference.GetVariableValue<int>(BlackboardKeys.SKILL_NO, out a);


        _enemyObject.GetComponent<SpawnerTileIndex>().SetTileIndex(_cellIndex);

        var arrIndexConsumer = _monsterEnemy.GetComponentsInChildren<IIndexConsumer>();
        foreach (var Consumer in arrIndexConsumer)
            Consumer.SetIndex(_cellIndex);

        var arrSpawnerConsumer =  _monsterEnemy.GetComponentsInChildren<ISpawnerConsumer>();
        foreach (var Consumer in arrSpawnerConsumer)
            Consumer.InjectSpawner(monsterDesc.Spawner);



        /*트랜스폼은 네브 매시 내부 트랜스폼(nextPosition)을 업뎃 시점에 덮어쓴다.updatePosition=true가 기본값임*/
        //_monsterEnemy.GetComponent<NavMeshAgent>().updatePosition = false;
        //_monsterEnemy.transform.position =_cellPosition;
        //_monsterEnemy.GetComponent<NavMeshAgent>().nextPosition = _cellPosition; 
        
        /*네브 매시 내부 트랜스폼*/
        _monsterEnemy.GetComponent<NavMeshAgent>().Warp(_cellPosition+Vector3.up*0.5f);

        
    }

    public void SetMonPos()
    {
        if (_monsterEnemy)
        {
            _monsterEnemy.transform.rotation = Quaternion.identity;
            _monsterEnemy.transform.position = _cellPosition+Vector3.up;
            _monsterEnemy.GetComponent<NavMeshAgent>().Warp(_cellPosition);
        }
    }
    public void GoBackToCell()
    {
        if (_monsterEnemy)
        {
            

            _monsterEnemy.transform.position = _cellPosition+Vector3.up;
            _monsterEnemy.GetComponent<NavMeshAgent>().SetDestination(_cellPosition);
        }
    }
    #endregion  


    #region 초기화
    public void Init()
    {
        _points=new EQSPoints();
        _targetMask =LayerMask.GetMask("Terrain");
        _wayPointsMask= new Vector3[9];
        for (int i=0;i<9;++i)
        {
            Vector3 offsetMask = Vector3.zero;
            int row = i/3; //0~2
            int col = i%3;//0~2
            offsetMask.x= row*(_cellWidth*0.5f)  - _cellWidth * 0.5f;
            offsetMask.z = col*(_cellDepth*0.5f) - _cellDepth * 0.5f;            
            _wayPointsMask[i] =  offsetMask; //0~8
        }
        isInitialized =true;
    }
    public void InitDebugSettings(TextMeshPro idxTextMesh)
    {
        _IndexTextInstance = idxTextMesh;
        
    }
    
    public bool TestValidityAndSet(Vector2 volumeSize ,int nowIndex,Vector3 posLT, NavMeshSurface surface )
    {
        _isCellFixed =false;
        if (!isInitialized) { Debug.Log("<color=#ff0000>스포너 셀을 먼저 초기화하세요.</color>"); return false; }
        transform.localRotation = surface.transform.localRotation;

        int row = (int)volumeSize.x / (int)_cellWidth; //x
        int col = (int)volumeSize.y /(int)_cellDepth; //z
        /*CellIndex 초기화*/
        _cellIndex = nowIndex;

        int idxX = _cellIndex/col;
        int idxZ = _cellIndex%col;

        _cellPosition =
            posLT
            +_cellWidth*(0.5f + idxX)* surface.transform.right.normalized
            +_cellDepth*(0.5f + idxZ)* surface.transform.forward.normalized;
        _cellPosition.y+=_cellHeight;
        transform.position= _cellPosition; 
        
        
        if (! Physics.Raycast(_cellPosition, Vector3.down, out var rayhit,20, _targetMask,QueryTriggerInteraction.Ignore))
        {
            /*도달 실패*/
            return false; 
        }
        for(int i =0; i<_wayPointsMask.Length;++i)
        {
            _wayPointsMask[i] =  surface.transform.localRotation*_wayPointsMask[i];
        }

        if ( NavMesh.SamplePosition(rayhit.point, out var hit,10, NavMesh.AllAreas)) // MaxDistance는 Src로부터 탐색 범위
        {
            /*CellPosition 초기화*/
            _cellPosition = hit.position;
            transform.position = _cellPosition;
            
            List< Vector3 > candidatesList  =new List<Vector3>();
            int pointIdx = 0;
            foreach(var point in _wayPointsMask)
            {
                
                if (NavMesh.SamplePosition(_cellPosition+point, out var samplehHit, 1, NavMesh.AllAreas))
                {
                    candidatesList.Add(samplehHit.position);

                    EQSPoint eqsPoint = new EQSPoint
                    {
                        pos = _cellPosition+point*0.75f,
                        ringIndex = 0,
                        pointIndexInRing = pointIdx++,
                        reserver =null,
                        occupant =null
                    };
                    _points.AddPoint(eqsPoint); 
                }
            }
            if (candidatesList.Count<9) return false;

            /*WAYPOINT 초기화*/
            _wayPoints = candidatesList.ToArray();
            _isCellFixed = true;
            return true; /*테스트 통과*/
        }
        
        return false;

    }
    #endregion


    #region 디버그
    public void OnDrawGizmos()
    {
        //return;
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);

        if (_isCellFixed)
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        var prev = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(_cellWidth, _cellHeight, _cellDepth) );
        if (_isCellFixed)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.purple;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_cellWidth, _cellHeight, _cellDepth));

        Gizmos.matrix = prev;
        if (_IndexTextInstance)
        {
            _IndexTextInstance.font = TMP_Settings.defaultFontAsset; // 폰트 보장
            _IndexTextInstance.alignment = TextAlignmentOptions.Center;
            _IndexTextInstance.color= Color.purple;
            _IndexTextInstance.fontSize =10;
            _IndexTextInstance.text =_cellIndex.ToString();
           
           
            _IndexTextInstance.transform.rotation = CameraUtil.GetCameraRotation(_cellPosition);
            _IndexTextInstance.transform.position = _cellPosition+Vector3.up*1f;
        }
        if (_isCellFixed)
        {
            foreach (var point in _wayPoints)
            {
                Gizmos.color= Color.aliceBlue;
                Gizmos.DrawWireSphere(point,0.1f);
            }
        }

    }
    #endregion
}
