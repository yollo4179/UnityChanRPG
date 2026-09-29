using Unity.VisualScripting;
using UnityEngine;
using static EQSManager;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "EQSIdealDistanceTestSO", menuName = "Scriptable Objects/EQSIdealDistanceTestSO")]
public class EQSIdealDistanceTestSO : ScriptableObject, IEQSTest
{
    [SerializeField] private float _weight = 100f;
    [SerializeField] private float _idealDistance = 1.5f; //몬스터마다 Ideal Distance를 따로 설정한다.
    [SerializeField] private float _maxDistance = 1;
    [SerializeField] private AnimationCurve _scoreCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float Weight { get => _weight; set => _weight = value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        //List<EQSPoint> ring0list= context.points.GetRingPoints(0);
        //Vector3 avg=Vector3.zero;
        //foreach(var p in ring0list)
        //{
        //    avg+=p.pos;
        //}
        //avg= avg / ring0list.Count;
        Vector3 vector = point.pos -context.target.position;
        vector.y=0;
        float distXZ = Vector3.Magnitude(vector);
        
        float normalizedDistance = /*/*Mathf.Abs*/(distXZ - _idealDistance) / _maxDistance;
        normalizedDistance = Mathf.Clamp01(normalizedDistance);

        // 이상적인 거리에 가까울수록 높은 점수
        return _scoreCurve.Evaluate(1f - normalizedDistance);// Distance면1 멀어질수록 0( 산 모양 그래프)
    }
}