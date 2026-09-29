using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using TMPro;
#if UNITY_ANDROID
using Unity.Android.Gradle.Manifest;
#endif
using Unity.VisualScripting;


using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static EQSManager;
using static Unity.Burst.Intrinsics.X86.Avx;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.MemoryProfiler;
using static UnityEditor.PlayerSettings;
#endif
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.LineRendering;
using static UnityEngine.UI.CanvasScaler;

public class EQSPoint
{
    //Pos를 중심으로하는 원 안의 랜덤한 구역으로 배치하면 더 자연스럽다? 
    public Vector3 pos { get; set; }
    public Vector3 toTargetDir { get; set; }
    /*Reserved or Occupied -> Filtering*/
    public EQSQuerier occupant { get; set; }
    public EQSQuerier reserver { get; set; }

    public int ringIndex { get; set; }
    public int pointIndexInRing { get; set; }

    public bool isAvailable => (null == occupant && null ==reserver);
    public bool isOccupied => (null !=occupant);

    public bool isDifferntPointInSameRing(EQSPoint other) =>
        (other.ringIndex ==this.ringIndex)&&(other.pointIndexInRing !=this.pointIndexInRing);
    public float DeltaTheta(EQSTarget target)
    {
        var vecDir = this.pos -  target.transform.position;
        vecDir.y=0;

        return Mathf.Atan2(vecDir.z, vecDir.x);
    }
}

public class EQSPoints
{
    /*링으로 분류해놓느 것도 괜찮다?*/
    
    public EQSTarget m_target;

    public List<EQSPoint> m_EqsAllPointsList = new List<EQSPoint>();
    public Dictionary<int, List<EQSPoint>> m_DicEqsPoints = new Dictionary<int, List<EQSPoint>>();

    public void Clear() { m_EqsAllPointsList.Clear(); m_DicEqsPoints.Clear(); }
    public void AddPoint(in EQSPoint point)
    {
        if (false == m_DicEqsPoints.ContainsKey(point.ringIndex))
        {
            m_DicEqsPoints.Add(point.ringIndex, new List<EQSPoint>());
        }
        m_DicEqsPoints[point.ringIndex].Add(point);
        m_EqsAllPointsList.Add(point);
    }
    public List<EQSPoint> GetAllPoints() => m_EqsAllPointsList;

    public List<EQSPoint> GetRingPoints(int ringIndex)
    {
        return m_DicEqsPoints.ContainsKey(ringIndex) ? m_DicEqsPoints[ringIndex] : new List<EQSPoint>();
    }

    public EQSPoint GetQuerierCurrentPoint(EQSQuerier querier)
    {
        return m_EqsAllPointsList.FirstOrDefault(p => p.occupant == querier);
    }

    public EQSPoint this[int Index] => m_EqsAllPointsList[Index];
    public int Count => m_EqsAllPointsList.Count;
}



public class EQSManager : MonoBehaviour //Ring
{
#region 싱글톤 세팅 코드 압축
    private static EQSManager m_Instance = null;
   private static EQSManager Instance
    {
        get
        {
            if(null == m_Instance)
            {
                m_Instance =FindFirstObjectByType<EQSManager>();
                m_Instance.Init();
                Debug.Log("<color=#00ffff>슬롯 매니저 생성 </color>");
            }
            if(null == m_Instance)
            {
                var GO = new GameObject( nameof(EQSManager) );
                m_Instance = GO.AddComponent<EQSManager>();
                m_Instance.Init();
            }
            return m_Instance;
        }
    }
    public static EQSManager GetInstance()
    {
        return Instance;
    }
    public static EQSManager GetUnSafeInstance()
    {
        return m_Instance;
    }
    #endregion
    /*슬롯*/
    [Header("Weights(슬롯 배치를 위한 가중치)")]
    public float wPath = 1f;    //경로의 길이
    public float wAngle = 0.6f; //슬롯과 Agent의 각도 차이dot =...1  
    public float wHysteris = 2.5f; //슬롯 변경 패널티 
    public float wCongestion = 0.4f;  //이웃 혼잡 방지


    [Header("Runtime")]
    [SerializeField] List<EQSQuerier> querierList = new List<EQSQuerier>();
    List<EQSQuerier> _chasingQuerierList = new List<EQSQuerier>();
    Dictionary<EQSQuerier, int>_dicChasingQuerierList = new Dictionary<EQSQuerier, int>();
    public List<EQSQuerier> GetAllQueriers() { return querierList; }

    [Header("Target")]
    [SerializeField] EQSTarget m_Target;
    public EQSTarget GetTarget() { return m_Target; }
    //Ring에 해당
    EQSPoints m_Points;
    public EQSPoints GetPoints() { return m_Points; }

   
    
    
    [Header("EQS POINTS Setings")]
    public float _numRings ;
    public float _numPoints;
    public float _spacing;
    public float _innerCircleRadius;
    public bool _useSpiral;
    public float _spiralAngle;
    [Header("DEBUG_WORLDUI")]
    [SerializeField]
    TextMeshPro m_TextMeshProPrefab;



    private void Awake()
    {
        
        GetInstance();
    }
    public void Init() {
        if (null ==m_Points) m_Points = new EQSPoints();     
    }

    public void ReAllocSlots (EQSTarget _Target)
    {
        if (null ==m_Points) m_Points = new EQSPoints();
        GenerateEQSPoints();
        

    }
    public void ReservePositionBasedDistance2Target()
    {
        /*Sort*/
        _chasingQuerierList.Sort((first,second)=>
        (first.transform.position - m_Target.transform.position).sqrMagnitude
        .CompareTo((second.transform.position - m_Target.transform.position).sqrMagnitude));
        /*Mapping*/
        EQSQuerier key = null; 
        for (int idx= 0; idx<_chasingQuerierList.Count ; ++idx)
        {
            key = _chasingQuerierList[idx];
            if (_dicChasingQuerierList.ContainsKey(key))
               _dicChasingQuerierList[key] =idx;
        }
        /*each Behabior Try to Query Newly*/
        for (int idx = 0; idx<_chasingQuerierList.Count; ++idx)
        {
            EQSQuerierBehaviour_Battle Behavior = _chasingQuerierList[idx].GetComponentInChildren<EQSQuerierBehaviour_Battle>();
            Behavior?.QueryNewPoint();
        }
        //qUERIER의 타겟으로 바꿔도 ㄱㅊ
    }
    public void AddChasingQuerier(EQSQuerier querier) {
    
        if(_dicChasingQuerierList.ContainsKey(querier)) return;
        _chasingQuerierList.Add(querier);
        _dicChasingQuerierList.Add(querier, _chasingQuerierList.Count-1);
    } 
    public void DeleteChasingQuerier(EQSQuerier querier)
    {

        if(_dicChasingQuerierList.TryGetValue(querier, out int idx)==false)  return;



        _chasingQuerierList.RemoveAt(_dicChasingQuerierList[querier]);
        _dicChasingQuerierList.Remove(querier);

        for (int i = 0; i<_chasingQuerierList.Count; ++i)
        {
            var key = _chasingQuerierList[i];
            if (_dicChasingQuerierList.ContainsKey(key))
                _dicChasingQuerierList[key] =i;
        }
    }

    
    private Dictionary<(int, int), (EQSQuerier, EQSQuerier)> BackupOccupancy()
    {
        Dictionary<(int, int), (EQSQuerier, EQSQuerier)> dicBackUpResrverAndAccupancy = new();
        //(RingIdx,PointIdx) ,( Res, Occ)//
        if (null ==m_Points) return null; 

        foreach(var eqsPoint in m_Points.m_EqsAllPointsList)
        {
            int ringIdx = eqsPoint.ringIndex;
            int pointIdx = eqsPoint.pointIndexInRing;
            if ((null == eqsPoint.occupant) && (null== eqsPoint.reserver)) continue;
            dicBackUpResrverAndAccupancy.Add((ringIdx, pointIdx), (eqsPoint.reserver, eqsPoint.occupant));
        }
        if (0==dicBackUpResrverAndAccupancy.Count) return null; 
        return dicBackUpResrverAndAccupancy;

    }
    
    public void GenerateEQSPoints()
    {
       // Dictionary<(int ringIdx, int pointIdx), (EQSQuerier reserver, EQSQuerier occupant)> Backup = BackupOccupancy();

        m_Points.Clear();

        Vector3 TargetPos = m_Target.transform.position;
        
        float Offset = _useSpiral? Mathf.Deg2Rad* _spiralAngle :0;

        float R=_innerCircleRadius;
        for (int i = 0; i<_numRings; ++i)
        {
            
            for (int j = 0; j<_numPoints; ++j)
            {
                Vector3 myPos = TargetPos+ new Vector3(Mathf.Cos(Offset*i+2*Mathf.PI/ _numPoints * j), 0f, Mathf.Sin(Offset*i+2*Mathf.PI/ _numPoints * j))*R;


                if (Physics.Raycast(myPos + Vector3.up*0.1f, Vector3.down, out var rh, 200f, LayerMask.GetMask("Terrain","Obstacles")))
                {
                }
                NavMeshHit hit;
                if (NavMesh.SamplePosition(rh.point,out hit, 1f, NavMesh.AllAreas))
                {
                    EQSPoint point = new EQSPoint();
                    point.pos = hit.position;
                    Vector3 Dir = TargetPos - point.pos;
                    Dir.Normalize();
                    point.toTargetDir = Dir;

                    point.pointIndexInRing = j;
                    point.ringIndex= i;

                    m_Points.AddPoint(point);
                    //if (null!= Backup&& Backup.ContainsKey((i, j)))
                    //{
                    //    point.occupant = Backup[(i, j)].occupant;
                    //    point.reserver = Backup[(i, j)].reserver;
                    //}


                }
            }
            R += _spacing;
        }
        ReservePositionBasedDistance2Target();
    }
    

}
