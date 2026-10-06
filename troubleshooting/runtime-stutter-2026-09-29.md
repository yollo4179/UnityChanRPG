# 런타임 간헐적 멈춤 조사 (2026-09-29)

## 질문과 결론

게임 중 가끔 크게 멈추는 현상이 객체 메모리 해제나 GC 때문인지 정적 코드와 현재 로그를 확인했다. **실제 멈춘 프레임의 Profiler 기록이 없어 GC가 직접 원인이라고 확정할 수는 없다.** 다만 추격 전투 코드가 매 프레임 여러 배열을 새로 만들고, 게임플레이 씬의 EQS가 2초마다 점수 계산용 객체를 다시 만든다. 이는 GC 압력을 만들 수 있다. 같은 경로에서 물리 및 NavMesh 계산도 집중되므로, 멈춤이 반드시 GC에서만 발생하는 것은 아니다.

## 코드에서 확인한 반복 작업

1. `Assets/Scenes/GameplayScene.unity`의 `ReAllocSlotsDuration`은 **2초**, EQS 설정은 **4링 × 8점**이다. `EQSTarget.Update`가 주기마다 `EQSManager.ReAllocSlots`을 호출한다. `GenerateEQSPoints`는 기존 목록을 비우고 최대 32개의 `EQSPoint`를 생성하며, 각 후보에서 `Physics.Raycast`와 `NavMesh.SamplePosition`을 호출한다. 이어 추격 중인 적마다 `QueryNewPoint`를 실행한다. 이전 `EQSPoint`와 링 목록은 참조가 사라져 이후 GC 대상이 된다. 관련 위치: `EQSTarget.cs:36-44`, `EQSManager.cs:173-199`, `EQSManager.cs:245-293`.
2. 추격 상태의 `ChaseAction.OnUpdate`는 `BattleBehavior`를 매 프레임 호출한다. `DoContextBassedSteering`는 그때마다 32칸 `Vector3[]`, `float[]` 2개와 3칸 `Vector3[]`를 생성하고, 목적지가 있으면 32방향 `Physics.SphereCast`를 수행한다. 회피 상황에서는 `FindConsecutiveSafeSlots`가 LINQ 목록과 `int[]`도 만든다. 관련 위치: `ChaseAction.cs:56`, `EQSQuerierBehaviour_Battle.cs:282`, `:423-443`, `:481`, `:712-742`.
3. 같은 추격 코드의 `_queryInterval` 기본값은 **0.2초**다. `EQSQuery.ExecuteQuery`는 매회 새 `List<ScoredPoint>`와 포인트별 `ScoredPoint`를 생성하고 정렬한다. `CalculatePoint`는 각 점마다 LINQ `Sum`을 호출한다. 2초 주기의 전체 재배치와 적별 0.2초 쿼리가 한 프레임에 겹칠 수 있다. 관련 위치: `EQSQuerierBehaviour_Battle.cs:27`, `:243-247`, `:981-991`, `EQSQuery.cs:146`, `:192-216`.
4. `DetectPlayer.Update`는 매 프레임 `Physics.OverlapSphere`의 결과 배열을 생성한다. 결과가 있을 때 `arrCol.ToArray<Collider>()`로 배열을 한 번 더 복사한다. 이어지는 `Where(...)`는 열거하지 않아 필터가 적용되지 않으며, 배열과 LINQ 객체만 추가로 생긴다. 이 컴포넌트는 Wolf, Wolf Cmpl, Smaug, Minotaur, Alian 프리팹에서 참조된다. 관련 위치: `DetectPlayer.cs:26-38`.

## 생성과 해제를 구분한 결과

- `Assets`의 C# 코드에서 명시적인 `GC.Collect`, `Resources.UnloadUnusedAssets`, `DestroyImmediate` 호출은 찾지 못했다.
- `PoolingManager.GetBack`은 오브젝트를 비활성화하고 큐에 보관한다. 이 순간 메모리를 해제하지 않는다. 풀이 부족할 때 `Pool.Create`에서 `Instantiate`가 발생할 수 있다. 관련 위치: `PoolingManager.cs:25-55`.
- `EnemySpawner`의 타일 및 적 `Instantiate`는 `Awake`에서 실행된다. 전투 중 주기적 스폰 경로는 아니다. `SceneGamePlay.Init`의 효과와 UI 풀 사전 생성도 게임플레이 씬 진입 때 실행된다. 씬 진입 순간의 멈춤 후보지만 주기적 멈춤 근거는 아니다.
- `Logs/Editor.log`에는 씬 열기 및 에디터 초기화 과정의 `Unloading ... unused Assets` 기록이 있다. 로그 맥락상 실제 게임플레이 프레임의 해제 기록으로 볼 수 없다. 게임플레이 Profiler 캡처 및 플레이어 로그는 찾지 못했다.
- `ProjectSettings/ProjectSettings.asset`의 `gcIncremental: 1`로 증분 GC가 설정되어 있다. 이 설정만으로 GC 정지가 없다고 판단할 수는 없다.

## 재현 시 확인 순서

1. 멈춤이 정확히 **2초 간격**인지 관찰한다. 맞으면 EQS 재배치 경로를 우선 본다. 추격 적 수가 늘 때 심해지는지도 확인한다.
2. Unity Profiler의 CPU 타임라인과 메모리 할당량을 같은 프레임에서 확인한다. 멈춘 프레임에 `GC.Collect`/GC 처리 시간이 두드러지면 누적 할당이 원인일 수 있다. `EQSManager.GenerateEQSPoints`, `EQSQuery.ExecuteQuery`, `EQSQuerierBehaviour_Battle.DoContextBassedSteering` 시간이 두드러지면 물리 및 NavMesh 계산도 원인이다.
3. 에디터에서만 멈춘다면 플레이 빌드와 비교한다. `SpawnTile.OnDrawGizmos`가 디버그 TMP의 텍스트와 속성을 계속 갱신하므로, Scene 뷰와 Gizmos가 켜진 에디터에서는 추가 비용이 있다. 관련 위치: `SpawnTile.cs:202-239`.

## 코드 변경 상태

이 조사는 읽기와 기록만 수행했다. 런타임 동작을 바꾸는 최적화는 아직 적용하지 않았다.
