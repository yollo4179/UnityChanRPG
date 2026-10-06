# 대화·퀘스트 저장 상태 트러블슈팅 (2026-09-29)

## 데이터 구조

- 퀘스트 정의, 단계, 보상, 자동 수락 여부는 `QuestData` ScriptableObject에 있다. 수락 선행 조건은 `QuestPrerequisite` ScriptableObject가 이전 퀘스트 완료 여부와 플레이어 레벨로 판단한다. `QuestCatalog`가 게임에서 사용할 퀘스트를 모은다.
- `DialogueNodes.csv`와 `DialogueEdges.csv`는 대사와 선택지를 정의한다.
- `Application.persistentDataPath`의 `QuestProcess.json`과 `QuestTaskProcess.json`은 실행 중 퀘스트·단계 상태를 저장한다. 플레이어, 인벤토리, 상점 등은 별도 JSON이다.

## 재현과 원인

1. 원본 `D:\UnityProjects\Data\UnityChanRPG` 저장본에서 `EQUIP_ITEMS` 퀘스트는 `QUEST_COMPLETED`인데 `EQUIP_ITEMS` 단계는 `ACCEPTED`, 진행도는 1/5였다.
2. `MONITORING_WOLF_AREA`는 `Get Back to TheGateKeeper`와 마지막 `Get Back to Mike and get some rewards`가 동시에 `ACCEPTED`였다. 그 사이 `Kill_Wolf Monster`는 이미 `COMPLETED`이고 진행도는 26/5였다.
3. 기존 `QuestManager.LoadAndJoinTasks`는 저장값을 그대로 사용했다. `ActivateQuest`는 단계 객체를 다시 만들어 진행도를 잃을 수 있었다. 단계 완료 코드는 바로 다음 단계를 무조건 `ACCEPTED`로 덮어써 완료된 단계가 되돌아갈 수 있었다. 일부 평가자는 생성자에서 구독해 READY 퀘스트도 이벤트를 받았다.
4. `UIDialogue.SetDialogues`가 팝업을 다시 열고, 수락 콜백을 버튼에 직접 추가했다. 재사용한 팝업에는 이전 콜백이 남을 수 있었다. `NEXT`로 분기 노드에 들어갈 때 선택지 계산을 건너뛰는 경로도 있었다.
5. 대사 CSV가 없으면 `DialogueParser`가 null 검사 전에 `EdgeAsset.text`를 읽었다. 첫 실행에서 퀘스트 저장 JSON이 없으면 `File.ReadAllText`가 예외를 냈다.

## 적용한 수정

- 원본 D 드라이브 파일 7개(`InvenInfo`, `QuestProcess`, `QuestTaskProcess`, `ShopInfo`, `UserInfo`, `UserLevelData`, `UserSkillInfo`)를 실제 런타임 저장 경로 `C:\Users\SSAFY\AppData\LocalLow\DefaultCompany\UnityChanRPG`로 복사했고 SHA-256 일치를 확인했다. 기존 런타임 JSON은 `Temp\save-backup-before-d-sync-20260929-102447`에 백업했다. D 드라이브 원본은 수정하지 않았다.
- 로드 시 완료된 퀘스트의 모든 단계를 완료로 맞추고, 수락 중인 퀘스트는 **가장 앞의 미완료 단계 하나**만 활성화한다. 완료 기록과 초과 진행도는 보존한다. 따라서 위 늑대 퀘스트는 문지기 대화 하나만 활성화되고, 그 대화를 완료하면 이미 끝난 늑대 처치 단계를 건너뛰어 마이크 대화로 간다.
- 퀘스트를 수락할 때 기존 단계 객체를 재생성하지 않는다. 현재 활성 단계만 이벤트를 구독하며, 완료 시 구독을 해제하고 다음 미완료 단계를 구독한다. 중복 완료 이벤트가 보상을 두 번 주지 않도록 완료 상태를 확인한다.
- `useAutoAcception`을 초기 로드, 선행 퀘스트 완료, 레벨 상승 후에 적용한다. 저장 파일이 없는 첫 실행도 빈 상태로 시작한다.
- 대사 팝업의 수락 콜백을 대화별로 교체하고 닫을 때 지운다. 분기 노드 진입 시 선택지를 즉시 표시한다. 존재하지 않는 대사 키와 CSV 누락을 처리한다. NPC는 대화 하나만 열고 퀘스트 활성화 이벤트에도 반응한다.
- 아이템 획득 이벤트를 발행하고 수집·소비·스킬 단계의 대상 검사를 보강했다. 퀘스트 보상의 경험치를 지급하고 여러 레벨 분량의 경험치도 순서대로 처리한다. UI가 아직 없을 때 인벤토리·골드·경험치 반영이 예외로 중단되지 않도록 했다.

## 검증 결과

- `dotnet build Assembly-CSharp.csproj --no-restore -v:q -clp:ErrorsOnly`: **오류 0개**. 기존 경고 52개.
- 실제 CSV를 사용하는 격리된 파서 테스트: 이벤트 7개, 노드 22개, 연결 17개. CSV 누락 시 빈 결과를 반환했다.
- 실제 D 퀘스트 JSON을 사용하는 격리된 .NET 테스트: 첫 실행 자동 수락, 완료 퀘스트 단계 보정, 늑대 퀘스트 활성 단계 하나 유지, 완료된 처치 단계 건너뛰기, 보상 1회 지급을 확인했다.
- `git diff --check` 통과. Unity 에디터 플레이 모드에서의 NPC 버튼 클릭은 아직 직접 검증하지 않았다.

## 남은 주의점

- 퀘스트와 단계 JSON은 별도 파일이므로 저장 도중 종료하면 두 파일의 시점이 달라질 수 있다. 로드 시 상태를 보정하지만, 모든 단계가 완료이고 퀘스트만 수락 중인 경우 보상을 이미 받았는지 판별할 정보가 없다. 이 경우 경고를 기록하고 자동으로 보상을 다시 주지 않는다.
- `useAutoComplete` 필드는 현재 사용되지 않는다. 완료 시 자동 보상 동작이 계속 적용된다. 수동 보상 수령이 필요하면 별도 UI와 저장 상태가 필요하다.
- `Task_ReinforceItem`에는 완료 평가가 없고, `Quest_LearnSkill.asset`은 카탈로그에 등록되지 않았으며 선행 조건 참조도 비어 있다. 현재 카탈로그의 네 퀘스트 진행에는 영향을 주지 않는다.
- 수집 퀘스트는 새 아이템 획득 이벤트에서 현재 보유량을 검사한다. 수락 전에 이미 충분한 아이템을 가진 경우 즉시 완료하는 처리는 아직 없다.
- 기존에 완료된 퀘스트의 미지급 경험치는 자동으로 소급 지급하지 않는다. D 드라이브의 플레이어 상태를 그대로 유지하기 위해서다.
