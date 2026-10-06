# 팝업 사용 중 카메라 회전 (2026-09-29)

## 원인

카메라의 yaw와 spring arm은 `UIManager.BlocksGameplayInput`만 확인했다. 이 속성은 인벤토리·퀘스트·장비·스킬창을 이동 가능한 일반 팝업으로 취급하므로, 창이 열린 동안에도 카메라가 마우스 이동과 휠 입력을 처리했다.

## 수정

`UIManager.BlocksCameraInput`을 추가했다. 일반 팝업에 포커스가 있거나 모달·UI 드래그·텍스트 입력 중이면 카메라 입력을 차단한다. `CameraPiivotYaw`와 `SpringArmCom`이 이 속성을 확인한다. 일반 팝업의 포커스를 월드 클릭으로 해제하면 창을 닫지 않고 카메라 조작이 다시 가능하다.

## 검증

`dotnet build Assembly-CSharp.csproj --no-restore -v:q -clp:ErrorsOnly`: 오류 0개. 에디터에서 I/Q/E/K로 각 팝업을 열고 마우스 이동·우클릭 드래그·휠로 카메라가 멈추는지, 월드 클릭으로 포커스만 해제한 뒤 다시 움직이는지 확인해야 한다.
