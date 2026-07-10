# Jazzy rosbridge 1차 검증 기록

- 검증일: 2026-07-10
- 호스트: Apple Silicon macOS + OrbStack Docker
- 컨테이너 아키텍처: ARM64
- 베이스 이미지: `ros:jazzy-ros-base`
- rosbridge_suite: Jazzy 2.7.0 바이너리 패키지

## 검증 결과

| 항목 | 결과 |
|---|---|
| Docker 이미지 빌드 | 성공 |
| `ros2unity_examples` colcon 빌드 | 성공 |
| `rosbridge_websocket` 9090 기동 | 성공 |
| `/ros2unity/heartbeat` 구독 | 성공 (`jazzy-ok:23`) |
| `/cmd_vel` 발행 | 성공 |
| bridge probe 수신 | 성공 (`linear.x=0.200`, `angular.z=0.500`) |
| Unity 2022.3.62f3 프로젝트 컴파일 | 성공 |
| ROS# WebSocket 접속 | 성공 |
| Unity의 `/ros2unity/heartbeat` 수신 | 성공 (`jazzy-ok:37`) |
| Unity의 `/cmd_vel` 구독 등록 | 성공 |
| Unity 로봇 이동 육안 확인 | 대기 |

## 실제 로그 핵심

```text
Rosbridge WebSocket server started on port 9090
Bridge probe ready: publishing /ros2unity/heartbeat and listening /cmd_vel
cmd_vel received: linear.x=0.200 angular.z=0.500
Client connected. 1 clients total.
Subscribed to /cmd_vel
Subscribed to /ros2unity/heartbeat
ROS2UNITY_BRIDGE_SMOKE_OK: jazzy-ok:37
```

## 후속 검증

1. Unity Play 모드에서 ROS의 `/cmd_vel`로 Rigidbody가 움직이는지 확인한다.
2. Unity에서 발행한 메시지를 Jazzy CLI로 확인한다.
3. `/odom`, `/tf`, `/scan`을 단계적으로 추가한다.
