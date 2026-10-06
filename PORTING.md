# UnityChan RPG 포팅 가이드

## 1. 실행 환경

| 항목 | 프로젝트 설정 |
| --- | --- |
| Unity Editor | 6000.5.4f1 |
| 대상 | Windows x64 |
| 그래픽스 | URP 17.5.0, Shader Graph 17.5.0 |
| AI | AI Navigation 2.0.13, Behavior 1.0.16 |
| 입력 | Input System 1.19.0 |
| 형상 관리 | Git, Git LFS |

버전의 기준은 `ProjectSettings/ProjectVersion.txt`, 의존성의 기준은 `Packages/manifest.json`과 `Packages/packages-lock.json`이다. Unity Hub에서 해당 에디터와 Windows Build Support를 설치한다. IL2CPP를 선택할 경우 Visual Studio의 C++ 데스크톱 개발 도구와 Windows SDK도 필요하다.

## 2. 소스와 에셋 복원

```powershell
git lfs install
git clone https://github.com/yollo4179/UnityChanRPG.git
Set-Location UnityChanRPG
git lfs pull
git lfs fsck
```

Unity Hub의 프로젝트 추가에서 저장소 루트를 지정한다. 최초 실행 시 패키지 다운로드와 에셋 임포트가 끝날 때까지 기다린다. 모델이나 텍스처 파일에 `version https://git-lfs.github.com/spec/v1`만 보이면 에셋이 아니라 LFS 포인터이므로 `git lfs pull`을 다시 실행한다.

`Assets`, `Packages`, `ProjectSettings`와 모든 에셋의 `.meta` 파일이 필요하다. `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `.vs`는 재생성되는 로컬 파일이다. `Assets/_Recovery`의 복구용 씬과 `LocalBackups`의 개인 저장 백업은 게임 실행에 필요하지 않다.

## 3. 에디터 실행

1. Console의 컴파일 오류가 없는지 확인한다.
2. `Assets/Scenes/LoginScene.unity`를 연다.
3. Play를 눌러 로그인 화면의 게임 시작 동작을 실행한다.
4. 로딩 후 GameplayScene 진입, 캐릭터 이동·공격과 HUD 표시를 확인한다.

시작 씬부터 실행해야 매니저, 아틀라스, UI와 풀링 리소스의 초기화 흐름을 함께 확인할 수 있다. 이 프로젝트에는 별도 게임 서버나 DB 설치 단계가 없다.

## 4. Windows 빌드

`File > Build Profiles`에서 Windows를 선택하고 씬을 아래 순서로 활성화한다.

1. `Assets/Scenes/LoginScene.unity`
2. `Assets/Scenes/LoadingScene.unity`
3. `Assets/Scenes/GameplayScene.unity`

비어 있는 출력 폴더를 지정해 Build한다. 실행 파일과 함께 생성된 `_Data` 폴더, UnityPlayer DLL 및 나머지 출력물을 같은 디렉터리에 유지한다.

회귀 검사와 Windows 빌드를 함께 실행하려면 에디터에서 프로젝트를 닫은 뒤 저장소 루트의 PowerShell에서 실행한다.

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Unity.exe'
& $unityEditor -batchmode -quit -projectPath (Get-Location).Path `
  -executeMethod GameplayRegressionBuild.Run -logFile gameplay-regression-build.log
```

`Assets/Editor/GameplayRegressionBuild.cs`는 CBS 조향, 팝업 순서, 스킬 이펙트 종료, 체인 라이트닝 종료를 검사하고 `Build/UnityChanRPG.exe`를 생성한다. 로그의 `GAMEPLAY_REGRESSIONS_PASSED`와 `GAMEPLAY_BUILD_SUCCEEDED`를 모두 확인한다. 자동 검사는 실제 플레이 검증을 대체하지 않는다.

## 5. 플레이 확인

- 로그인 → 로딩 → 게임 진입과 종료 메뉴
- 이동·대시·점프·기본 공격·근접/원거리 스킬 및 이펙트 종료
- UI 열기·닫기, 드래그, 화면 밖 이탈 방지, UI 조작 중 공격·카메라 입력 차단
- 퀘스트 수락·진행·완료, NPC 대화, 재실행 후 진행 상태 복원
- 몬스터 추적·공격·사망·리스폰, Smaug 비행·착지 이후 다음 패턴
- 아이템 획득, 경험치·레벨업과 HP 바 표시

## 6. 문제 해결

| 증상 | 확인 항목 |
| --- | --- |
| 패키지·API 컴파일 오류 | 에디터 버전과 패키지 잠금 파일 확인. 임의로 패키지 버전을 올리지 않는다. |
| 분홍색 머티리얼 | URP 패키지와 Graphics/Quality의 Render Pipeline Asset 확인 |
| 모델·텍스처 임포트 실패 | Git LFS 다운로드와 `.meta` 파일 확인 |
| 아틀라스·풀링 리소스 누락 | `Assets/Resources/Prefabs/AtlasManager.prefab`, `GameplayResourceBootstrap`의 리소스 경로 확인 |
| 플레이 결과가 이전 실행과 다름 | 게임 저장 데이터와 개인 백업 여부 확인. 기존 저장은 백업 후 분리한다. |
| 배치 빌드가 프로젝트 사용 중이라고 종료 | 해당 프로젝트를 연 Unity Editor를 닫고 다시 실행 |

이 문서는 저장소 설정과 빌드 스크립트를 기준으로 작성했다. 다른 PC에서의 새 clone 빌드와 실제 플레이는 별도로 확인해야 한다.
