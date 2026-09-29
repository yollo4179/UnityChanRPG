using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
[CreateAssetMenu(fileName = "EQSDirectionTendencyTestSO", menuName = "Scriptable Objects/EQSDirectionTendencyTestSO")]
public class EQSDirectionTendencyTestSO : ScriptableObject, IEQSTest
{
    [SerializeField] private float _weight = 1.8f;
    public float Weight { get =>_weight; set=> _weight =value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        Vector3 targetPos=  context.target.transform.position;
        Vector3 querierPos = querier.transform.position;
        Vector3 pointPos = point.pos;


        Vector3 target2Point = (pointPos - targetPos).normalized;
        Vector3 Querier2Point = (querierPos - pointPos).normalized;
        
        float dot = Vector3.Dot(target2Point, Querier2Point);


        //if (dot < 1e-6f)
        //{
        //    Vector3 toTarget = VectorUtil.PlatVector(context.target.position - querier.transform.position).normalized;
        //    Vector3 tanLine = new Vector3(-1* toTarget.z, 0, toTarget.x);
        //    dot = Vector3.Dot(tanLine, Querier2Point);
        //}
        float score01 = dot*0.5f+0.5f;
        return score01;
    }



}
