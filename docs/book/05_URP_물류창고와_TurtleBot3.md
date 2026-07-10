# URP 물류창고와 TurtleBot3

## 1. 실습 목표

최소 연결 씬을 실제 로봇 시뮬레이션 형태로 확장한다. Unity 2022.3.62f3의 URP 환경에서 물류창고와 TurtleBot3 Burger를 배치하고 ROS 2 Jazzy의 `/cmd_vel`로 이동시킨다.

## 2. 프로젝트와 씬

```text
Unity 프로젝트: unity_ros_bridge_2022_urp
실습 씬: Assets/Scenes/ROS2UnityWarehouse.unity
렌더 파이프라인: URP 14.0.12
브리지: ROS# 2.2.2 + rosbridge_suite 2.7.0
```

## 3. 에셋 구성 원칙

Unity 공식 Robotics Warehouse 원본은 Unity 2020.3, URP 10, Perception 0.9 preview를 기준으로 제작되었다. 현재 프로젝트에 전체 패키지를 설치하면 구형 패키지 의존성이 URP 14와 충돌할 수 있다. 따라서 다음 파일만 선별해 사용한다.

- `Warehouse.fbx`
- `ShelvingRackA.fbx`
- `ShelvingRackC.fbx`
- TurtleBot3 Burger의 변환 완료 메시
- TurtleBot3 좌우 바퀴와 LDS-01 LiDAR 메시

창고와 TurtleBot3 에셋은 Apache License 2.0이며 원본 저장소, 커밋, 라이선스를 각 `Assets/ThirdParty` 하위에 기록한다.

## 4. 씬 다시 생성

Unity 메뉴에서 다음을 실행하면 URP 재질, 창고 충돌체, 랙, 팔레트와 화물 상자, TurtleBot3, ROS Bridge와 카메라를 다시 구성한다.

```text
ROS2Unity > Build URP Warehouse Scene
```

씬 생성이 끝나면 Console에 다음 메시지가 나타난다.

```text
ROS2UNITY_WAREHOUSE_SETUP_OK
```

장면 전체는 공식 [Robotics Warehouse](https://github.com/Unity-Technologies/Robotics-Warehouse)와 [Robotics Nav2 SLAM Example](https://github.com/Unity-Technologies/Robotics-Nav2-SLAM-Example)의 단위 체계를 따른다. Unity 좌표 `1`은 `1 m`이고, TurtleBot3의 원본 메시 단위는 `mm`이므로 `0.001`을 적용한다.

| 대상 | 적용 기준 | 생성 후 크기 |
|---|---|---|
| 공식 Warehouse | 원본 스케일 `1` | 약 `30.89 × 8.25 × 54.86 m` |
| ShelvingRackA | 배율 상수가 아닌 목표 높이로 자동 정규화 | 약 `2.83 × 3.65 × 0.95 m` |
| TurtleBot3 Burger | 원본 메시 × `0.001` | 약 `0.18 × 0.19 × 0.14 m` |
| 화물과 팔레트 | 랙 FBX에 포함된 원본 메시 사용 | 선반과 일체 배치 |

화물 큐브를 별도로 생성하지 않으므로 중복 적재와 공중 배치가 발생하지 않는다. 로봇은 랙 사이 통로 중앙에서 시작하고 메시 하단을 콜라이더 바닥에 자동 정렬한다. 가까운 추적 카메라·청록색 가시성 링·노란 안전선으로 실제 크기를 유지하면서도 화면에서 쉽게 찾을 수 있다. 터미널로 포커스를 옮겨도 ROS 제어가 멈추지 않도록 백그라운드 실행을 활성화한다.

현재 카메라 구도를 이미지로 점검하려면 다음 메뉴를 실행한다.

```text
ROS2Unity > Capture Warehouse Preview
```

## 5. Jazzy 연결

저장소 루트에서 컨테이너를 시작한다.

```bash
docker compose up -d --build
```

Unity에서 `ROS2UnityWarehouse` 씬을 열고 Play를 누른다. Game 화면 왼쪽 위에서 다음 상태를 확인한다.

```text
rosbridge: CONNECTED
heartbeat: jazzy-ok:...
```

## 6. TurtleBot3 이동

다른 터미널에서 다음 명령을 실행한다.

```bash
docker compose exec ros2unity bash -lc \
  'source /opt/ros/jazzy/setup.bash && ros2 topic pub -r 10 \
  /cmd_vel geometry_msgs/msg/Twist \
  "{linear: {x: 0.15}, angular: {z: 0.2}}"'
```

발행을 멈출 때는 `Ctrl+C`를 누른다. 명령이 끊기면 0.5초 뒤 안전 정지하도록 구성되어 있다.

## 7. 성공 기준

1. Unity Console에 컴파일 오류가 없다.
2. Game 화면에 `CONNECTED`가 표시된다.
3. heartbeat 값이 계속 갱신된다.
4. `/cmd_vel`의 `linear.x`에 따라 로봇이 전진한다.
5. `angular.z`에 따라 로봇이 회전한다.
6. 명령 중단 후 로봇이 정지한다.

다음 단계에서는 이 로봇에 `/odom`, `/tf`, `/scan`을 추가한다.
