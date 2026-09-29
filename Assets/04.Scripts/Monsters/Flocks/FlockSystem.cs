using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public static class FlockSystem//NameSpace
{
    /* 그룹 객체가 가질 것*/
    public struct SteeringContext
    {
        public Transform Self;
        public Vector3 Velocity;
        public Vector3 TargetPos;
        public List<Transform> Neighbors;   // 같은 플록
        public List<Transform> Blocks; 
        public SteeringData FlockData;
    }
    /*Spawner가 가진다. 직렬화 시켜도 괜찮나?*/
    public class SteeringData
    {
        /*이동*/
        public float neighborRaduius = 6f;
        public float SeperationRadius = 2f;

        /*가중치*/
        /*Basic*/
        public float wSeperation = 1.5f;
        public float wAlignment = 1f;
        public float wCohesion = 0.8f;

        public float wSeek = 1.2f;
        public float wArrive = 1f;
        public float wFlee = 1.2f;
        public float wAvoid = 2f;
  
        public LayerMask Neighbor;
        public LayerMask Block;
    }

    public static Vector3 Allignment(SteeringContext Context)
    {
        Vector3 AvgDir = Vector3.zero;

        if (0==Context.Neighbors.Count) return AvgDir;

        foreach(var Neighbor in Context.Neighbors)
        {
            AvgDir+=Neighbor.forward; //아니면 Velocity 꺼내와
        }
        AvgDir /= Context.Neighbors.Count;
        return AvgDir;
    }
    public static Vector3 Cohesion(SteeringContext Context)
    {
        Vector3 Center  =Vector3.zero;
        if (0==Context.Neighbors.Count) return Center;

        foreach (var Neighbor in Context.Neighbors)
            Center +=Neighbor.position;
        Center/=-Context.Neighbors.Count;
        /*Center 구하고 -> 그래서 내가 갈 방향은*/
        
        Vector3 Goal = (Center -Context.Self.position);
        return (Goal-Context.Velocity).normalized;

    }
    public static Vector3 Seperation(SteeringContext Context)
    {
        Vector3 force = Vector3.zero;
        int count = 0;
        foreach (var n in Context.Neighbors)
        {
            Vector3 TowardDir = Context.Self.position - n.position;
            float d = TowardDir.magnitude;
            if (d > 0f && d < Context.FlockData.SeperationRadius)
            {
                force += TowardDir.normalized; // 더 가까울수록 강하게 밀기
                count++;
            }
        }
        if (count > 0) force /= count;
        return force * Context.FlockData.wSeperation;

    }
    public static Vector3 CalculateDesiredDirection(SteeringContext Context)
    {
        Vector3 GroupSteeringDir = Vector3.zero;

        GroupSteeringDir +=  Seperation(Context)*Context.FlockData.wSeperation;
        GroupSteeringDir += Cohesion(Context)*Context.FlockData.wCohesion;
        GroupSteeringDir += Allignment(Context)*Context.FlockData.wAlignment;



        return GroupSteeringDir; 

    }
}

