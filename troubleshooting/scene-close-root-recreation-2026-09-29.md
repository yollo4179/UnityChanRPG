# 씬 종료 중 매니저 루트 재생성 (2026-09-29)

## 증상

씬을 닫을 때 Unity가 `@UI_Root`, `{Pool_Root}`, `@Managers`가 정리되지 않았다고 경고했다.

## 원인

`Managers.UI`와 `Managers.Event`는 내부에서 `Managers.Instance`를 사용한다. 매니저가 이미 파괴된 상태라면 `Instance`는 새 `@Managers`를 만들고 `Awake`에서 `Pool.Init()`과 `UI.Init()`을 실행한다. 이 과정에서 `{Pool_Root}`와 `@UI_Root`도 생성된다.

드래그 UI의 `OnDisable()`과 퀘스트 NPC의 `OnDestroy()`가 이 생성형 접근자를 사용했다. 이벤트 구독 핸들의 `Dispose()`에도 같은 접근 경로가 있었다.

## 수정

종료 콜백용 `Managers.UIIfExists`와 `Managers.EventIfExists`를 추가했다. 두 속성은 현재 살아 있는 매니저만 반환하며 새 오브젝트를 생성하지 않는다. 드래그 상태 해제, NPC 이벤트 해제, 이벤트 핸들 해제에 이 속성을 사용한다. 정상 게임 진행 중 사용하는 `Managers.UI`와 `Managers.Event`의 초기화 동작은 유지한다.

## 확인

`dotnet build Assembly-CSharp.csproj --no-restore -v:q -clp:ErrorsOnly`로 C# 컴파일을 확인한다. Unity 에디터에서 플레이 모드 진입 후 인벤토리 아이템 드래그와 퀘스트 NPC가 있는 씬을 종료하여 동일한 경고가 재발하는지 확인해야 한다.
