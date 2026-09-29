using System;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;



public class MovementCom_CBS : MovementCom
{
    [Header("Rays")]
    /*RayInfo*/
    [SerializeField]
    int NumRays = 8;

    [Header("TargetPlayer")]
    [SerializeField]
    Transform Target;

    [Header("Weights")]
    [SerializeField]
    public float DesiredRadius = 5f;
    [SerializeField]
    public float WP_Chase = 1f;//For Interests
    [SerializeField]
    public float WP_Cohesion = 1f;

    [SerializeField]
    public float WP_Obstacles = 1f;//For Avoid
    [SerializeField]
    public float WP_Enemies = 1f;
    [SerializeField]
    public float WP_Walls = 1f;
    [SerializeField]
    public float WP_PathDir = 0.8f;

    [Header("Motion")]
    public float maxSpeed = 4f;
    public float maxAccel = 20f;
    [Range(0, 1)] public float mapSmoothAlpha = 0.25f;  // 맵 시간 스무딩
    [Range(0, 1)] public float dirHysteresis = 0.1f;    // 직전 방향 보너스

    
    public Vector3 ExtraDir;//군집 알고리즘
    public LayerMask DangerMask;
    public float RayProvingRadius = 1f;
    public float DangerRange = 1f;

    /*Contecxt Arrays*/
    Vector3[] RayDirections;
    float[] SlotsInterests, SlotsSmoothInterests;
    float[] SlotsDangers, SlotsSmoothDangers;
  

    /*ContextChosen*/
    Vector3 DesiredDir = Vector3.zero;
    Vector3 Velocity;
    float PreviousAffectBias = 0.1f;

    float MaxSpeed = 1f;
    float RotSpeed = 0.1f;


    protected override void Awake()
    {
        
        base.Awake();
        DangerMask = LayerMask.GetMask("Enemies", "Obstacles");
        Init();
    }

    public void Update()
    {

        isOnGround();
        ApplyGravity();

        ClearMaps();

        /*Idle*/

        /*Chase*/

        /*Attack*/

        /*국소 경향*/
        AddInterestsContextMap_Chase(Target.position, WP_Chase);
        //AddPathDirInterests()Astar 누적
        FillDangerByRayCast(WP_Chase);

        ApplySmooth(SlotsSmoothInterests, SlotsInterests,0.25F);
        ApplySmooth(SlotsSmoothDangers, SlotsDangers, 0.25F);

        Vector3 Force = ChooseDirection(SlotsSmoothInterests, SlotsSmoothDangers)*MaxSpeed;
        Force.y+=m_MovementVector.y;

        Vector3 VectorXZ = (Target.position -transform.position);
        VectorXZ.y=0;
        float DistanceXZ = VectorXZ.magnitude;
        if (CharacterController &&DistanceXZ>1.2f)
        {
            CharacterController.Move(Force*Time.deltaTime);
            m_Animator.SetFloat("MovementSpeed", Force.magnitude*Time.deltaTime);

        }
        else
        {
            m_Animator.SetFloat("MovementSpeed",0);
        }
        


        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(DesiredDir), RotSpeed);

    }
    public void Init()
    {
        SlotsInterests = new float[NumRays];
        SlotsSmoothInterests = new float[NumRays];
        SlotsDangers = new float[NumRays];
        SlotsSmoothDangers= new float[NumRays];
        RayDirections = new Vector3[NumRays];

        float Angle=0;
        for (int i = 0; i<NumRays; ++i)
        {
            Angle += 360f/ NumRays;
            RayDirections[i] = (Quaternion.Euler(0f, Angle, 0f)* Vector3.right).normalized;
        }

    }
    public void UpdatePhysics() {

    }
    /*Chase _Battle*/
    void AddInterestsContextMap_Chase(Vector3 TargetPos, float Weight)
    {
        Vector3 ToTargetDir = TargetPos -transform.position;
        ToTargetDir.y=0;
        float dist = ToTargetDir.magnitude;

        if (dist<=0) return;
        Vector3 BaseToTargetDir = ToTargetDir.normalized;//기저 벡터 처리  

        for (int i = 0; i<NumRays; ++i)
        {
            float NowRayWEight =Vector3.Dot(BaseToTargetDir, RayDirections[i]);
            SlotsInterests[i] += (1+NowRayWEight)*0.5f;//(-1 ~ 1 -> 0~2) 
                                             // SlotsSmoothInterests[i] =Mathf.SmoothStep(SlotsSmoothInterests[i], SlotsInterests[i], 0.5f); 
        }

    }

    public Vector3 RaySource()
    {
        return transform.position +Vector3.up*0.2f;
    }
    /*Danfger Avoid ->Blcok or Obstacles*/
    void FillDangerByRayCast(float Weight)
    {
        Vector3 MyPos = RaySource();
        /*Wall 인식*/



        for (int i = 0; i<NumRays; ++i)
        {
           

            if (Physics.Raycast(RaySource(), RayDirections[i], out RaycastHit Hit, DangerRange, DangerMask))
            {
                //벽에 레이가 닿아도 가중치 부여
                float t = Hit.distance/DangerRange;
                SlotsDangers[i] = 1-t + 0.125f;
            }
        }

    }

    void ClearMaps()
    {
        System.Array.Fill(SlotsInterests, 0f);
        System.Array.Fill(SlotsDangers, 0f);
    }

    Vector3 ChooseDirection(float[] interstsMap, float[] DangerMap)
    {

        Vector3 Sum = Vector3.zero;
        for (int i = 0; i<NumRays; ++i)
        {
            float Weight = Mathf.Max(0, SlotsInterests[i] - SlotsDangers[i]);
            /*이전 방향으로의 보정*/
            if (DesiredDir.sqrMagnitude >0)//처음 이후로 
            {
                Weight += PreviousAffectBias* Mathf.Max(0f, Vector3.Dot(DesiredDir, RayDirections[i]));
            }
            /*이전 방향과 비슷하면 점수 더 준다. Jiterring 방지(경향성)*/

            Sum +=Weight*RayDirections[i];

            if (0 < SlotsInterests[i] - SlotsDangers[i])
            {
                UnityEngine.Debug.DrawLine(RaySource(), RaySource()+Weight * RayDirections[i], Color.green);
            }
            else
            {
                UnityEngine.Debug.DrawLine(RaySource(), RaySource()+ SlotsDangers[i]*RayDirections[i], Color.red);
            }
        }
        UnityEngine.Debug.DrawLine(RaySource(), RaySource()+DesiredDir, Color.cyan);
        return DesiredDir = Sum.normalized;
    }
    public void ApplySmooth(float[] dst,float[] Newdst, float T)
    {
        T =Mathf.Clamp(T,0, 1);
       for(int i=0; i<dst.Length;++i)
        {
            dst[i] = Mathf.Lerp(dst[i], Newdst[i], T);
        }

    }
}


