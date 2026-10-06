# Smaug 패턴 종료 후 정지 조사 (2026-09-29)

아래 원인 분석은 수정 전 상태를 기록한다. 현재 적용 내용은 문서 끝의 **적용한 수정**을 참고한다.

## 확인 범위

`SmaugBattleAction`, `SmaugBattleScript`, 지상/공중 스킬 노드, `SmaugBehavior.asset`, Smaug 프리팹과 Animator Controller를 대조했다. 실제로 멈춘 프레임의 Behavior Graph/Animator 상태 기록은 없어 런타임 원인 하나로 확정하지는 않았다.

## 코드상 재현되는 정지 경로

Smaug 프리팹의 `_rangeOffset`은 **3.84m**, `_stoppingDuration`은 **4.5초**다. `SmaugBattleScript.MovingLogic`은 `Chase` 중 플레이어까지의 수평 거리가 3.84m 미만이면 `Stop`으로 바꾸고 이동 방향을 0으로 반환한다. 4.5초 후 다시 `Chase`가 되지만, 다음 호출에서도 플레이어가 여전히 3.84m 안이면 즉시 `Stop`으로 되돌아간다. 결과적으로 가까이 있는 플레이어 앞에서는 패턴 종료 후에도 이동이 계속 정지한다. `_nowMovementType == Stop`이면 회전도 하지 않는다. 해당 위치: `Assets/04.Scripts/Monsters/Smaug/SmaugBattleScript.cs:63-97`, 프리팹 `Assets/06.MonsterModels/FourEvilDragonsPBR/Prefab/Smaug.prefab:2987-2989`.

지상 공격 범위는 **3.5m**다. 따라서 플레이어가 **3.5~3.84m**에 있으면 Smaug가 정지하지만 근접 공격 거리에는 들어오지 않는다. 플레이어가 정지 중 옆으로 이동하면 회전도 멈춰 근접 공격의 30도 각도 조건을 만족하지 못할 수 있다. 원거리 화염이나 비행 공격은 별도 쿨타임이 있어, 이 구간이 긴 무반응으로 보일 수 있다. 관련 위치: `Assets/04.Scripts/Monsters/Smaug/SmaugSkillPrerequisiteChecker.cs:25-80`, 프리팹 `:3010-3048`.

## 패턴 종류에 따라 추가로 확인할 경로

- **공격 뒤 아무 행동도 하지 않음:** `MonsterSkillAction`은 원하는 애니메이션의 `fullPathHash`가 나올 때까지 제한 없이 기다린다. 상태가 진입하지 못하거나 코루틴 안에서 예외가 발생하면 `_animationDone`이 설정되지 않아 Action이 `Running`에 남을 수 있다. 이때 `SmaugBattleAction.UpdateBattle`도 실행되지 않으므로 실제로 움직이지 않는다. 해당 위치: `Assets/04.Scripts/Monsters/MonsterWolf/TreeNodes/MonsterSkillAction.cs:105-132`, `:199-241`, `:243-303`.
- **비행 패턴 뒤 공중에서 정지:** `SmaugFlyAction`은 Blackboard의 `FlySkillNO`를 1로 바꾸지만 Animator의 동명 정수 매개변수에는 값을 보내지 않는다. Animator의 Take Off 전이는 `FlySkillNO` 값으로 Fly Forward 또는 Fly Float를 선택하므로, 코드가 의도한 웨이포인트 모드와 애니메이션 분기가 어긋날 수 있다. `SmaugFlyAction`의 종료는 `_isWaitingTimeEnd`와 Fly Float 상태 및 플레이어 방향 10도 이내라는 조건을 모두 요구한다. 해당 위치: `SmaugFlyAction.cs:60-106`, `:170-213`, `SmaugController.controller:203-245`.
- `SmaugBattleAction.OnStart`는 `OnBattle` 트리거를 보낸다. Smaug Animator Controller에는 `OnBattle` 매개변수가 없다. 콘솔 오류의 원인이 될 수 있으며 제거하거나 실제 매개변수에 맞춰야 한다. 이것만으로 패턴 종료 뒤 정지를 확정할 수는 없다. 해당 위치: `SmaugBattleAction.cs:32`, `SmaugController.controller:589-682`.

## 재현 중 판별 기준

1. 플레이어가 **3.84m 밖으로 이동했을 때 Smaug가 다시 따라오면** `MovingLogic`의 정지 반복이 원인이다.
2. 3.84m 밖에서도 계속 정지하면 멈춘 순간의 Behavior Graph 현재 노드, Blackboard `EnemyState`, Animator 현재/다음 상태, Console 예외를 확인한다. `MonsterSkillAction`이 `Running`이면 애니메이션 해시 대기 또는 코루틴 예외를 우선 본다.
3. 공중에서 멈추면 `SmaugFlyAction`의 `FlySkillNO` Blackboard 값과 Animator 매개변수 값을 비교하고, Fly Float 진입 여부를 확인한다.

## 변경 상태

## 적용한 수정

- 정지 반경을 3.2m로 낮춰 근접 공격 범위(3.5m) 안에 넣었다. 정지 중에도 플레이어를 바라보고, 3.6m 밖으로 나가면 즉시 추격한다. 기존 4.5초 정지 타이머와 재정지 반복을 제거했다.
- 지상/공중 공격에 각각 12초 종료 제한을 두었다. 애니메이션 상태에 진입하지 못하거나 코루틴이 멈추면 이펙트와 히트박스를 회수하고 다음 상태로 넘어간다. 지상 공격은 Blackboard의 애니메이션 번호와 SkillSO 선택 번호를 분리했다.
- 비행 시작 시 Animator의 `FlySkillNO`도 설정한다. 반복 비행에서도 `OnFly` 트리거를 다시 보내며, 마지막 웨이포인트 선택과 이동 초과를 고쳤다. 비행은 30초, 착지는 12초 종료 제한을 둔다.
- Smaug Animator에 없는 `OnBattle` 트리거 호출을 제거했다.

`dotnet build Assembly-CSharp.csproj --no-restore -v:q -clp:ErrorsOnly`는 오류 0개로 통과했다. 실제 플레이 모드에서 지상 공격 종료, 비행 공격, 착지, 패턴 반복을 확인하는 작업은 아직 남아 있다. 종료 제한이 발동하면 Unity Console에 경고를 남긴다.
