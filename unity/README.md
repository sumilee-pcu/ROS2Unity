# Unity 프로젝트 설정

이 폴더는 완성된 Unity 프로젝트 전체가 아니라, 새 Unity 프로젝트에 적용할 프로젝트 고유 자산과 설정 지침을 보관합니다. Unity의 `Library`, `Temp`, 외부 패키지 원본은 Git에 넣지 않습니다.

## 기준 버전

- 1차 검증: Unity 2022.3 LTS
- ROS#: 2.2.2 고정
- Unity 6: 이후 호환성 트랙에서 별도 검증

## 새 프로젝트 만들기

1. Unity Hub에서 3D Core 프로젝트를 생성합니다.
2. Package Manager에서 `Add package from git URL`을 선택합니다.
3. 다음 URL로 ROS#를 설치합니다.

```text
https://github.com/siemens/ros-sharp.git?path=/com.siemens.ros-sharp#2.2.2
```

4. ROS#의 ROS 2 설정을 선택하고 샘플의 `RosConnector`를 씬에 추가합니다.
5. WebSocket 주소를 `ws://localhost:9090`으로 지정합니다.
6. 이 저장소의 `Assets/ROS2Unity`를 Unity 프로젝트의 `Assets` 아래에 복사합니다.
7. Player Settings의 Scripting Define Symbols에 `ROSSHARP_PRESENT`를 추가합니다.

## 첫 씬

1. 바닥용 Plane과 로봇용 Cube를 만듭니다.
2. Cube에 `Rigidbody`와 `DifferentialDriveController`를 추가합니다.
3. 같은 오브젝트에 `RosSharpTwistSubscriber`를 추가합니다.
4. ROS# `RosConnector`의 주소가 `ws://localhost:9090`인지 확인합니다.
5. Docker Compose를 실행한 뒤 Unity Play를 누릅니다.
6. `/cmd_vel`을 발행해 Cube가 전진·회전하는지 확인합니다.

> ROS# 2.2.2의 Unity 6 지원은 공식적으로 충분히 검증되지 않았습니다. 첫 성공 기준은 Unity 2022.3 LTS로 고정합니다.

