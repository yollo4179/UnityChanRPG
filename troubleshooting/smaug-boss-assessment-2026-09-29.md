# Smaug 보스 적합성 및 패턴 확장 검토 (2026-09-29)

초기 판단과 데이터 분석은 수정 전 상태를 기록한다. 현재 적용 내용은 문서 끝의 **이번 구현**을 참고한다.

## 판단

Smaug의 DragonTerrorBringer 모델은 보스로 사용할 수 있다. 현재 자산에는 지상 기본 공격, 발톱, 화염, 비행 공격, 이륙, 활공, 착지, 포효, 방어, 피격, 사망 애니메이션이 있다. 게임플레이 씬에는 Smaug 전용 보스 스폰 지점과 비행 웨이포인트 9개가 있다. 새 모델이나 새 애니메이션을 사기 전에 기존 동작을 조합해 전투 단계를 만들 수 있다.

현재 구현 상태는 보스 완성도에 못 미친다. 실제 공격은 발톱, 머리, 지상 화염, 공중 화염 4개이고, 별도 체력 단계·패턴 가중치·빈틈 설계는 없다. 포효·방어 애니메이션과 `SmaugTornado` 효과는 공격 패턴에 연결되지 않았다. 근접 정지 반복과 패턴 종료 대기는 `smaug-stuck-after-pattern-2026-09-29.md`에 기록했다.

## 중요한 데이터 불일치

`Assets/06.MonsterModels/FourEvilDragonsPBR/Prefab/Smaug.prefab`의 `StatusScript._characterID`와 `SmaugController.m_ID`는 모두 **1003**이다. `Assets/Resources/Data/Json/MonstersData.json`의 1003은 이름이 **미노타우르스**, 레벨 15, 최대 체력 **650**이다. `StatusScript.LoadScriptInfo`는 `_characterID`로 이 데이터를 읽는다. 따라서 현재 Smaug는 실제 실행 시 해당 미노타우르스 체력·방어·경험치 데이터를 쓰고, 처치 이벤트도 ID 1003으로 발행한다. Smaug 전용 ID와 데이터가 필요하다.

## 같은 애니메이션을 다른 패턴으로 재사용할 수 있는가?

가능하다. 현재 `SkillSO`의 `EventSequence`는 애니메이션 정규화 시간, 이펙트 풀 키, 소켓 이름, 히트박스 크기와 중심, 타격 횟수·피해량을 데이터로 가진다. 예를 들어 같은 `Flame Attack` 동작을 근거리 지속 화염과 바닥 위험 구역의 예고 동작으로 각각 사용할 수 있다.

하지만 현재 `MonsterSkillAction`은 **Blackboard SkillNO 하나로 Animator의 SkillNO, SkillSO handle, 애니메이션 해시 배열 인덱스를 모두 선택**한다. 배열에는 지상 공격 해시 3개뿐이고 종료 시 `SkillNO % 3`으로 순환한다. 기존 3개 바깥에 새 패턴 ID를 바로 추가하면 해시 배열 범위를 넘거나 애니메이터 전이와 맞지 않는다. `_setSpecificSkillSOHandleOption`도 코루틴에서 `_skillNo.Value`를 배열 인덱스로 사용해 안전한 분리 수단이 아니다. 새 변형을 추가하려면 `PatternID`, `AnimatorSkillNO`, `SkillSO`를 분리해야 한다.

`FireEvents`는 `eventClips[0]`만 실행하며 현재 `COLLIDER`, `EFFECT`, `ON_SKILL`만 처리한다. `SUMMON_PROJECTILES`는 enum에는 있지만 이 노드에 구현되지 않았다. 효과 핸들도 `_effectHandle` 하나라 동시 실행되는 효과 이벤트 여러 개를 그대로 추가하면 정리 시 서로 덮일 수 있다. 복합 이펙트 프리팹 또는 핸들 목록 관리가 필요하다.

## 기존 자산을 활용한 추천 패턴

| 패턴 | 사용하는 애니메이션·자산 | 필요한 구현 |
| --- | --- | --- |
| 발톱 쓸기 / 강화 발톱 쓸기 | `Claw Attack` + 기존 타격 효과 | 패턴별 이펙트·판정 폭·피해량 데이터 분리. 강화판은 긴 예고와 긴 후딜을 둔다. |
| 직선 화염 / 바닥 잔불 | `Flame Attack` + `SmaugBreath` | 같은 모션에 다른 예고와 위험 구역을 얹는다. 바닥 잔불은 월드 위치의 지속 판정 코드가 필요하다. |
| 공중 화염 / 낙하지점 폭격 | `Fly Flame Attack` + `SmaugMeteor` | 웨이포인트 이동과 표적 표시를 구분한다. 폭격은 월드 위치 생성과 순차 판정 코드가 필요하다. |
| 회오리 장판 | `Scream` 또는 `Flame Attack` + 미사용 `SmaugTornado` | 포효·화염 애니메이션을 예고로 사용하고, 월드 위치 장판 및 지속 판정 코드를 추가한다. |
| 방어 후 빈틈 | `Defend` | 방어 전환, 체력 단계, 반격 또는 경직 조건을 구현한다. |

추천 순서는 `정지·패턴 종료 버그 수정 → Smaug 전용 ID·체력 데이터 → PatternID와 AnimatorSkillNO 분리 → 지상 변형 2개 → 공중/장판 패턴 → 체력 단계`다. 한 번에 많은 패턴을 넣기보다 타격 예고와 회피 가능성을 각 패턴마다 확인해야 한다.

## 변경 상태

## 이번 구현

- Smaug 전용 ID 1006을 `MonstersData.json`, 프리팹의 상태/컨트롤러, GameplayScene의 물기·화염 데미지 소스에 연결했다. 데이터는 레벨 25, 최대 체력 8000, 공격 65, 방어 15, 경험치 250, 골드 2500이다. 1003 미노타우르스 데이터와 Smaug 구역 ID 1003은 유지했다.
- 체력 50% 이하에서 머리 공격의 **기존 Basic Attack 애니메이션**을 그대로 쓰면서 `SmaugBiteEffect` 대신 `Sparks_explode_yellow`를 재생하는 강화 SkillSO를 추가했다. 피해량은 50→60, 히트박스는 3.5×1.76×3.47→4×2×4.25다.
- SkillSO 선택과 애니메이션 번호를 분리했다. 현재 변형은 머리 공격 1개이며, 다른 새 패턴이나 포효/방어/장판은 아직 연결되지 않았다.

실제 게임 밸런스와 이펙트 위치는 플레이 모드에서 확인해야 한다. 특히 2페이즈 물기 공격의 시각적 예고와 회피 가능성을 확인한 뒤 수치를 조정하는 것이 좋다.
