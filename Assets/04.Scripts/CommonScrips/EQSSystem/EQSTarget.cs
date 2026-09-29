using System;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class EQSTarget : MonoBehaviour
{
    [Header("Ring Slot Settings")]
    [Tooltip("(캡슐 반지름) Base Radius")]
    public float MyCapsuleRadius = 0.15f;
    [Tooltip("Offset Radius")]
    public float OffsetRadius = 0.15f;
    [Tooltip("무기 사거리 추가지름 (Extra Radius For Weapon)")]
    public float ExtraRadius = 0f;
    [Tooltip("Factor For Space Between Agent(충돌 지터링 방지)")]
    public float SpaceFactor = 1.3f;
    [Tooltip("에이전트가 위치할 수 있는 슬롯 수")]
    public int NumSlots = 12;
    [Tooltip("슬롯을 재배치 할 주기 or 캐릭터 이동 할 때마다 슬롯 재배치")]
    public float ReAllocSlotsDuration = 0.5f;//코루틴? 
    float LastAllocTimePoint;
    [Tooltip("디버그 / 슬롯 그리기")]
    public bool DebugMode = true;

    //[Tooltip("네브 메시 볼륨")]
    //public NavMeshModifierVolume _navMeshModiVolume;
    [Tooltip("네브")]
    public NavMeshUtil _navUtil;

    //일단 표준 슬롯 반지름 구하고 표준 기준으로 개체 크기마다 위치 설정
    public float ComputeRingRadius(float AvgAgentRadius)
    {
        return MyCapsuleRadius +ExtraRadius + AvgAgentRadius+ OffsetRadius;//Agent사이와의 평균 거리 
    }
    public void Update()
    {
        if (!Application.isPlaying) return; 

        /*일 정 주기 마다 한번 실행 (에이전트는 살짝 뜸들일것 슬롯에서 바로 벗어나지 않고)*/
        if(Time.time -LastAllocTimePoint >= ReAllocSlotsDuration)
        {
            LastAllocTimePoint =Time.time;

            EQSManager.GetInstance().ReAllocSlots(this);

            //if(null != _navUtil)
            //{
            //    _navUtil.RebuildSurface();
            //}
        }
        Vector3 t = transform.position;
       
    }
    






}

