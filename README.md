# ROS2Unity

ROS 2 Jazzy와 Unity를 연결해 모바일 로봇 시뮬레이션, 센서 데이터, SLAM, Nav2, 강화학습을 단계적으로 학습하는 교재·개발 저장소입니다.

이 프로젝트는 기존 `PhysicalAIcos`의 ROS 2 토픽 중심 학습 흐름과 `ROS2_2026`의 Jazzy 기준 환경을 계승합니다. Gazebo Harmonic 실습을 대체하지 않고, 같은 ROS 인터페이스에 Unity를 두 번째 시뮬레이터로 연결합니다.

## 현재 상태

Unity–Jazzy 연결 PoC를 마치고 첫 실습 프로젝트를 구성한 단계입니다.

- 기준 ROS: ROS 2 Jazzy / Ubuntu 24.04
- 기준 브리지: ROS# 2.2.2 + `rosbridge_suite`
- 기준 Unity: Unity 2022.3.62f3 (macOS Apple Silicon 검증, Windows는 같은 버전 권장), Unity 6는 ML-Agents용 별도 프로젝트에서 검증
- 1차 인터페이스: `/cmd_vel`, `/odom`, `/tf`, `/scan`, `/clock`
- 현재 구현: Jazzy rosbridge 컨테이너, ROS 2 왕복 확인 노드, Unity 데모 씬과 차동구동 제어
- 검증 완료: ARM64 Jazzy 이미지 빌드, rosbridge 2.7.0 기동, ROS# WebSocket 접속과 Unity heartbeat 수신
- 아직 검증하지 않은 항목: Unity 로봇의 `/cmd_vel` 이동 육안 확인, 센서 처리량, SLAM/Nav2 종단 간 실행

## 왜 ROS# + rosbridge인가

Unity 공식 ROS-TCP 계열은 Jazzy를 공식 지원하지 않고 예제가 Foxy 세대에 머물러 있습니다. `ros2-for-unity`는 Galactic/Humble 중심이며 일반 커뮤니티 지원이 제한적입니다. 반면 ROS#는 ROS 2 지원이 유지되고, Jazzy의 `rosbridge_suite`와 WebSocket으로 연결되므로 ROS 배포판 결합도가 낮습니다.

고해상도 카메라나 PointCloud2를 높은 주기로 전송해야 하는 단계에서는 네이티브 DDS 계열을 별도 성능 트랙으로 검토합니다.

## 저장소 구조

```text
ROS2Unity/
├── docs/
│   ├── book/                 # 교재 원고
│   └── decisions/            # 기술 선택 기록
├── docker/                   # Jazzy + rosbridge 실행 환경
├── ros2_ws/src/              # 교재용 ROS 2 예제 패키지
├── unity_ros_bridge_2022/    # Unity 2022.3.62f3 ROS# 실습 프로젝트
└── scripts/                  # 빌드·검증 보조 스크립트
```

## 빠른 시작

Docker가 실행 중인 컴퓨터에서:

```bash
docker compose up -d --build
```

다른 터미널에서 rosbridge 상태를 확인합니다.

```bash
docker compose exec ros2unity bash
ros2 topic echo /ros2unity/heartbeat
```

`/cmd_vel` 왕복 확인:

```bash
docker compose exec ros2unity bash
ros2 topic pub --once /cmd_vel geometry_msgs/msg/Twist \
  "{linear: {x: 0.2}, angular: {z: 0.5}}"
```

Unity Hub에서 `unity_ros_bridge_2022`를 열고 `Assets/Scenes/ROS2UnityBridge.unity`를 실행합니다. 화면 왼쪽 위에 `rosbridge: CONNECTED`와 heartbeat가 표시되면 정상입니다. 자세한 설정은 [02_개발환경_구축](docs/book/02_개발환경_구축.md)을 참고합니다.

## 교재 목차

1. [프로젝트 방향과 학습 목표](docs/book/00_교재_로드맵.md)
2. [Unity–Jazzy 시스템 구조](docs/book/01_시스템_아키텍처.md)
3. [개발환경 구축](docs/book/02_개발환경_구축.md)
4. [첫 토픽 왕복 실습](docs/book/03_첫_토픽_왕복.md)
5. [기존 PhysicalAI 실습의 Unity 이관표](docs/book/04_PhysicalAI_이관표.md)

## 공개 범위

원고가 집필·검증 중인 동안은 저장소를 **Private**으로 운영하는 것을 권장합니다. 공개 전에는 코드와 교재의 라이선스를 분리하고, 외부 이미지·URDF·에셋의 재배포 권리를 점검해야 합니다. 자세한 정책은 [저장소 공개 정책](docs/decisions/0002-repository-visibility.md)에 기록했습니다.

## 주의

이 저장소에는 Unity, Siemens ROS#, ROS 2 또는 제3자 에셋 원본을 복제하지 않습니다. 설치 스크립트·버전 정보·자체 작성 예제만 관리합니다.
